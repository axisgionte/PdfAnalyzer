using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PdfAnalyzer.Messages;

namespace PdfAnalyzer.ViewModels
{
    public class CSVDocumentViewModel : ObservableRecipient
    {
        private readonly CSVDocument document;

        private List<string> csvLines;
        public List<string> CSVLines
        {
            get => csvLines;
            set => SetProperty(ref csvLines, value);
        }

        // Constructor to initialize the ViewModel
        public CSVDocumentViewModel(string fileName)
        {
            document = new CSVDocument(fileName);
            CSVLines = document.Lines ?? new List<string>();  // Safeguard in case Lines is null
            SendUpdateCSVFileMessage();
        }

        // Method to send message for updated CSV file if it contains data
        private void SendUpdateCSVFileMessage()
        {
            if (CSVLines.Any())
            {
                Messenger.Send(new UpdateCSVMessage(CSVLines), 1);
            }
        }
    }
}
