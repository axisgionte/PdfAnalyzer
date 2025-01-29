using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PdfAnalyzer.Properties;

namespace PdfAnalyzer.ViewModels
{
    public class CSVPanelViewModel : ObservableRecipient
    {
        private CSVDocumentViewModel document;
        private bool fullFind;
        private int threadCount;
        public CSVDocumentViewModel Document
        {
            get => document;
            set => SetProperty(ref document, value);
        }

        public bool FullFind
        {
            get => fullFind;
            set
            {
                if (SetProperty(ref fullFind, value))
                {
                    Settings.Default.FullFind = value;
                    Settings.Default.Save();
                }
            }
        }

        public int ThreadCound
        {
            get => threadCount;
            set
            {
                if (SetProperty(ref threadCount, value))
                {
                    Settings.Default.ThreadCount = value;
                    Settings.Default.Save();
                }
            }
        }

        public ICommand OpenCSVCommand { get; }

        public CSVPanelViewModel()
        {
            OpenCSVCommand = new RelayCommand(Open);
            FullFind = Settings.Default.FullFind;
            ThreadCound = Settings.Default.ThreadCount;
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
