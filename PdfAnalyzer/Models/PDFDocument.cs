using System;
using Syncfusion.Pdf.Parsing;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using PdfAnalyzer.Properties;
using System.Linq;
using PdfAnalyzer.Searchers;

namespace PdfAnalyzer.Models
{
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
                    var searchResult = new HashSet<string>();
                    var findAny = Settings.Default.FindAny;
                    var ahoCorasick = Settings.Default.AhoCorasick;
                    var length = pdfLoadedDocument.Pages.Count;

                    // Find the longest line length
                    int maxLength = lines.Max(line => line.Length);
                    int additionalTextLength = maxLength * 2;

                    for (int i = 0; i < length; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        string pageText = pdfLoadedDocument.Pages[i].ExtractText();

                        if (string.IsNullOrEmpty(pageText) == false)
                        {
                            if (findAny == false)
                            {
                                pageText = pageText.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                            }
                        }

                        // If not the last page, append text from the next page
                        if (i + 1 < length)
                        {
                            string nextPageText = pdfLoadedDocument.Pages[i + 1].ExtractText();

                            if (string.IsNullOrEmpty(nextPageText) == false)
                            {
                                if (findAny == false)
                                {
                                    nextPageText = nextPageText.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                                }
                                pageText += nextPageText.Substring(0, Math.Min(nextPageText.Length, additionalTextLength));
                            }
                        }

                        Dictionary<string, List<int>> results = ahoCorasick
                            ? pageText.AhoCorasickSearch(lines, findAny, cancellationToken)
                            : pageText.DefaultSearch(lines, findAny, cancellationToken);

                        if (findAny == false && results.Any())
                        {
                            return true;
                        }

                        foreach (var result in results)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (searchResult.Contains(result.Key) == false)
                            {
                                searchResult.Add(result.Key);
                            }
                        }

                        if (findAny && searchResult.Count == lines.Count)
                        {
                            // If all lines are found, stop searching
                            return true;
                        }

                    }

                    return false;
                }

            });
        }
    }
}