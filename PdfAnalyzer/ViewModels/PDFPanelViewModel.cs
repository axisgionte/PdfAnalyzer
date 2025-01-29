using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfAnalyzer.Messages;
using System.Windows.Input;
using System.Collections.Generic;

namespace PdfAnalyzer.ViewModels
{
    public class PDFPanelViewModel : ObservableRecipient
    {
        private PDFDocumentViewModel pdfDocument;
        private ObservableCollection<PDFDocumentViewModel> pdfDocuments;
        private CancellationTokenSource cancellationTokenSource;
        private List<string> csvDocument;

        public ICommand OpenPDFCommand { get; }

        public ObservableCollection<PDFDocumentViewModel> PDFDocuments
        {
            get => pdfDocuments;
            set => SetProperty(ref pdfDocuments, value);
        }

        public PDFDocumentViewModel PDFDocument
        {
            get => pdfDocument;
            set => SetProperty(ref pdfDocument, value);
        }



        public PDFPanelViewModel()
        {
            Messenger.Register<PDFPanelViewModel, UpdateCSVMessage, int>(this, 1, HandleUpdateCSVMessage);

            pdfDocuments = new ObservableCollection<PDFDocumentViewModel>();
            OpenPDFCommand = new RelayCommand(Open);
        }

        // Open file dialog and add selected PDFs to the ObservableCollection
        private void Open()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select PDF",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = true,
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (openFileDialog.ShowDialog() == true)
            {
                PDFDocuments.Clear();
                foreach (var file in openFileDialog.FileNames)
                {
                    PDFDocuments.Add(new PDFDocumentViewModel(file));
                }

                if (csvDocument.Any()) 
                {
                    UpdateCSVMessage();
                }
            }
        }

        // Handle update CSV message by applying text search to each document
        private void HandleUpdateCSVMessage(PDFPanelViewModel recipient, UpdateCSVMessage message)
        {
            csvDocument = message.CSVDocument;
            UpdateCSVMessage();
        }
        

        private async Task UpdateCSVMessage()
        {
            cancellationTokenSource?.Cancel();

            // Reset the cancellation token source for the current operation
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Iterate through each PDF document and apply the update asynchronously
            if (pdfDocuments.Any())
            {
                var documents = pdfDocuments.ToArray();
                var lines = csvDocument.ToList();

                foreach (var document in pdfDocuments)
                {
                    try
                    {
                        // Pass the cancellation token to handle cancellation
                        await document.UpdateFindStatus(lines, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine("Operation was canceled.");
                        return;  // Optionally stop processing further documents
                    }
                    catch (Exception ex)
                    {
                        // Handle other errors (logging or notifying the user)
                        Debug.WriteLine($"Error updating document: {ex.Message}");
                    }
                }
            }
        }
    }
}