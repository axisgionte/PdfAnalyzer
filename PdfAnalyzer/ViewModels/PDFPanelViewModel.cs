using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using PdfAnalyzer.Messages;
using System.Collections.Generic;
using System.Linq;

namespace PdfAnalyzer.ViewModels
{
    public class PDFPanelViewModel : ObservableRecipient
    {
        PDFDocumentViewModel pdfDocument;
        ObservableCollection<PDFDocumentViewModel> pdfDocuments;

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

            }
        }
        private void HandleUpdateCSVMessage(PDFPanelViewModel recipient, UpdateCSVMessage message)
        {
            if (pdfDocuments.Any())
            {  
                foreach (var document in pdfDocuments)
                {
                    document.UpdateFindStatus(message.CSVDocument); 
                }
            }
        }
    }  
}