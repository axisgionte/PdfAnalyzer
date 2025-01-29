using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace PdfAnalyzer.ViewModels
{
    public class CSVPanelViewModel : ObservableRecipient
    {
        private CSVDocumentViewModel document;

        public CSVDocumentViewModel Document
        {
            get => document;
            set => SetProperty(ref document, value);
        }

        public ICommand OpenCSVCommand { get; }

        public CSVPanelViewModel()
        {
            OpenCSVCommand = new RelayCommand(Open);
        }

        // Open CSV file using OpenFileDialog
        private void Open()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select CSV",
                Filter = "CSV Files (*.csv)|*.csv",
                Multiselect = false,
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Only create new Document if a file is selected
                if (!string.IsNullOrEmpty(openFileDialog.FileName))
                {
                    Document = new CSVDocumentViewModel(openFileDialog.FileName);
                }
            }
        }
    }
}
