using System;
using Syncfusion.Pdf.Parsing;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

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

            // Check if the file exists
            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"Document not found {filePath}");
                return false;
            }

            using (var pdfLoadedDocument = new PdfLoadedDocument(filePath))
            {
                var foundWords = new HashSet<string>();
                var length = pdfLoadedDocument.Pages.Count;

                // Iterate through all the pages
                for (int i = 0; i < length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string pageText = pdfLoadedDocument.Pages[i].ExtractText();

                    // Look for each word in the page text
                    foreach (var word in lines)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (pageText.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            foundWords.Add(word);
                        }
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }

                // Return true if all lines are found
                return foundWords.Count == lines.Count;
            }
        }, cancellationToken);
    }
}