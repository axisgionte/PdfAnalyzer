using System;

public interface IDocument
{
	public Load(string path);
	GetLines();
	Find(string text);
}
