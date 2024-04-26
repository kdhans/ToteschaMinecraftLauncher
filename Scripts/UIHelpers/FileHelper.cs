using Godot;
using System;
using System.IO;
using System.Text;

public partial class FileHelper : Node
{
	public bool TryReadAppTextFile(string fileName, out string contents)
	{
		contents = null;
		
		bool fileRead = false;
		try { contents = File.ReadAllText(fileName, Encoding.Unicode); fileRead = true; } catch { }

		return fileRead;
	}

	public bool TryWriteAppTextFile(string fileName, string contents) 
	{

		bool fileWritten = false;
		try { File.WriteAllText(fileName, contents, encoding: Encoding.Unicode); fileWritten = true; } catch { }

		return fileWritten;
	}
}
