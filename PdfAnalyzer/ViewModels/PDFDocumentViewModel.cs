using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PdfAnalyzer.Models;

namespace PdfAnalyzer.ViewModels
{
    public class PDFDocumentViewModel : ObservableRecipient
    {
        private readonly PDFDocument pdfDocument;
        private bool isTextFound;

        // Property to track if specific text is found in the PDF
        public bool IsTextFound
        {
            get => isTextFound;
            set => SetProperty(ref isTextFound, value);
        }

        // Constructor to initialize PDFDocument with the file path
        public PDFDocumentViewModel(string filePath)
        {
            pdfDocument = new PDFDocument(filePath);
        }

        // File path to be used in the view or other logic
        public string FilePath => pdfDocument.FilePath;

        // Method to update the FindState (text search status) asynchronously
        internal async Task UpdateFindStatus(List<string> lines, CancellationToken token)
        {
            try
            {
                IsTextFound = await pdfDocument.FindTextAsync(lines, token);
            }
            catch (OperationCanceledException)
            {
                // Handle the cancellation here if needed (logging, setting UI state, etc.)
                IsTextFound = false;
            }
        }
    }
}
