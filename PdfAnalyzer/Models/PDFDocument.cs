using System;
using Syncfusion.Pdf.Parsing;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

public class PDFDocument : IDocument
{
    private readonly string filePath;

    public PDFDocument(string path)
    {
        filePath = path;
    }

    public string FilePath => filePath;

    public bool FindText(List<string> lines)
    {
        if (!File.Exists(filePath))
        {
            Debug.WriteLine($"Document not found {filePath}");
            return false;
        }


        using (var pdfLoadedDocument = new PdfLoadedDocument(filePath))
        {
            var foundWords = new List<string>();

            var length = pdfLoadedDocument.Pages.Count;
            for (int i = 0; i < length; i++)
            {
                string pageText = pdfLoadedDocument.Pages[i].ExtractText();

                foreach (var word in lines)
                {
                    if (pageText.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (!foundWords.Contains(word))
                            foundWords.Add(word);
                    }
                }
            }

            return foundWords.Count > 0 && foundWords.Count == lines.Count;
        }
    }
}
