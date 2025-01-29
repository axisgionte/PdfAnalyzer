using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
            lines = File.ReadLines(filePath).ToList();
        }
        else
        {
            Debug.WriteLine($"Document not found {filePath}");
        }
    }
}
