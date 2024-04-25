using Godot;
using System;
using System.IO;

public partial class FileHelper : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public bool TryReadAppTextFile(string fileName, out string contents)
	{
		contents = null;
		var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
			return false;
		}

		var filePath = Path.Combine(path, fileName);
		if (!File.Exists(filePath))
			return false;

		bool fileRead = false;
		try { contents = File.ReadAllText(filePath); fileRead = true; } catch { }

		return fileRead;
	}

	public bool TryWriteAppTextFile(string fileName, string contents) 
	{
        var path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return false;
        }

        var filePath = Path.Combine(path, fileName);
        if (!File.Exists(filePath))
            return false;

        bool fileWritten = false;
        try { File.WriteAllText(filePath, contents); fileWritten = true; } catch { }

        return fileWritten;
    }
}
