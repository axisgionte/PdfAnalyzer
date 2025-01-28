using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using PdfAnalyzer.Messages;

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
                Document = new CSVDocumentViewModel(openFileDialog.FileName);
            }
        } 

    }
}
