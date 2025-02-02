using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PdfAnalyzer.Models
{
    public class CSVDocument : IDocument
    {
        private List<string> lines;
        private readonly string filePath;

        public CSVDocument(string filePath)
        {
            this.filePath = filePath;
            lines = new List<string>();
            Load();
        }

        public string FilePath => filePath;
        public List<string> Lines => lines;

        // Load the file content into the lines list
        private void Load()
        {
            if (File.Exists(filePath))
            {
                var rows = File.ReadAllLines(filePath);
                if(rows.Length > 0)
                {
                    lines = rows.ToHashSet().ToList();
                } 
            }
            else
            {
                Debug.WriteLine($"Document not found {filePath}");
            }
        }
    }
}