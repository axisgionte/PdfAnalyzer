using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PdfAnalyzer.ViewModels
{
    public class PDFDocumentViewModel : ObservableRecipient
    {
        private readonly PDFDocument pdfDocument;
        private bool findState;

        public bool FindState
        {
            get => findState;
            set => SetProperty(ref findState, value);
        }

        public PDFDocumentViewModel(string filePath)
        {
            pdfDocument = new PDFDocument(filePath);
        }

        public string FilePath => pdfDocument.FilePath;


        internal void UpdateFindStatus(List<string> lines)
        {
            FindState = pdfDocument.FindText(lines);
        }
    }
}
