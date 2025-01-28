using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfAnalyzer.Messages
{
    public class UpdateCSVMessage
    {
        public List<string> CSVDocument { get;  }
        public UpdateCSVMessage(List<string> lines) 
        {
             CSVDocument = lines;
        }
    }
}
