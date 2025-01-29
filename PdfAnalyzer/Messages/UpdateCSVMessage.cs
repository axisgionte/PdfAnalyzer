using System.Collections.Generic;

namespace PdfAnalyzer.Messages
{
    public class UpdateCSVMessage
    {
        public List<string> CSVDocument { get; }
        public UpdateCSVMessage(List<string> lines) 
        {
             CSVDocument = lines;
        }
    }
}
