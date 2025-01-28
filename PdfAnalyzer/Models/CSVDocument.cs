using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class CSVDocument:IDocument
{
    private List<string> lines;
    private readonly string filePath;

    public CSVDocument(string filePath)
	{
        this.filePath = filePath;
        Load();
	}
    
    public string FilePath => filePath;
    public List<string> Lines => lines ?? new List<string>();
    
    private void Load()
    {

        if (File.Exists(filePath))
        {
            var line = File.ReadAllLines(filePath);
            lines = line.ToList();
        }
        else
        {
            Debug.WriteLine($"Document not found {filePath}");
        }
    }
}
