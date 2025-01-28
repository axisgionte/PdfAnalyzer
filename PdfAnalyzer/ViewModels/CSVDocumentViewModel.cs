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

        public CSVDocumentViewModel(string fileName)
        {
            document = new CSVDocument(fileName);
            CSVLines = document.Lines;
            SendUpdateCSVFileMessage();
        }

        private void SendUpdateCSVFileMessage() 
        {
            if (document.Lines.Any())
            {
                Messenger.Send(new UpdateCSVMessage(document.Lines), 1);
            }
        }
    }
}
