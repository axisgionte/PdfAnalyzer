using System;
using Syncfusion.Pdf.Parsing;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using PdfAnalyzer.Properties;

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

            // If the lines list is empty, return based on the findAllWords flag
            if (lines.Count == 0)
            {
                return false;
            }

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"Document not found {filePath}");
                return false;
            }

            using (var pdfLoadedDocument = new PdfLoadedDocument(filePath))
            {
                bool findAllWords = Settings.Default.FullFind;
                var foundWords = new HashSet<string>();
                var length = pdfLoadedDocument.Pages.Count;

                // Iterate over all the pages
                for (int i = 0; i < length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string pageText = pdfLoadedDocument.Pages[i].ExtractText();

                    foreach (var word in lines)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // If the word is found, add it to the foundWords set
                        if (pageText.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            foundWords.Add(word);
                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();


                    // If looking for all words, and we've already found them all, stop searching
                    if (findAllWords && foundWords.Count == lines.Count)
                    {
                        return true;
                    }

                    // If looking for at least one word, and we've found any, stop searching
                    if (!findAllWords && foundWords.Count > 0)
                    {
                        return true;
                    }
                }

                return findAllWords ? foundWords.Count == lines.Count : foundWords.Count > 0;
            }
        }, cancellationToken);
    }
}