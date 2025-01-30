using System;
using Syncfusion.Pdf.Parsing;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using PdfAnalyzer.Properties;
using System.Linq;

public class PDFDocument : IDocument
{
    private readonly string filePath;

    public PDFDocument(string path)
    {
        filePath = path;
    }

    public string FilePath => filePath;

    public Task<bool> FindTextAsync(List<string> lines, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If lines are empty, return based on the findAllWords flag
            if (lines.Count == 0)
            {
                return false;
            }

            if (File.Exists(filePath) == false)
            {
                Debug.WriteLine($"Document not found {filePath}");
                return false;
            }

            using (var pdfLoadedDocument = new PdfLoadedDocument(filePath))
            {
                var findAllWords = Settings.Default.FullFind;
                var foundWords = new HashSet<string>();
                var length = pdfLoadedDocument.Pages.Count;



                // Find the longest word length
                int maxLength = lines.Max(word => word.Length);
                int additionalTextLength = maxLength * 2;  // Double the length of the longest word

                for (int i = 0; i < length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string pageText = pdfLoadedDocument.Pages[i].ExtractText();

                    // Check if page text is not null or empty, and remove spaces and line breaks
                    if (string.IsNullOrEmpty(pageText) == false)
                    {
                        if (findAllWords == false)
                        {
                            pageText = pageText.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                        }
                    }

                    // If not the last page, append text from the next page
                    if (i + 1 < length)
                    {
                        string nextPageText = pdfLoadedDocument.Pages[i + 1].ExtractText();

                        // Check if next page text is not null or empty, and clean it
                        if (string.IsNullOrEmpty(nextPageText) == false)
                        {
                            if (findAllWords == false)
                            {
                                nextPageText = nextPageText.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                            }
                            pageText += nextPageText.Substring(0, Math.Min(nextPageText.Length, additionalTextLength));
                        }
                    }

                    // Search for each word in the page text
                    foreach (var word in lines)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (string.IsNullOrEmpty(pageText) == false && pageText.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            foundWords.Add(word);
                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    // If all words are found, stop searching
                    if (findAllWords && foundWords.Count == lines.Count)
                    {
                        return true;
                    }

                    // If any word is found, stop searching
                    if (findAllWords == false && foundWords.Count > 0)
                    {
                        return true;
                    }
                }

                return findAllWords ? foundWords.Count == lines.Count : foundWords.Count > 0;
            }

        });
    }

}