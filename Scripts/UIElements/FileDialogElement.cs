using Godot;
using System;

public partial class FileDialogElement : FileDialog
{
	private LineEdit directory;
	private const int MinimumWidth = 800;
	private const int MinimumHeight = 700;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.DirSelected += OnSelectedDirectory;
	}

	private void OnSelectedDirectory(string dir)
	{
		directory = GetNode<LineEdit>("/root/LauncherWindow/DisplayAreaContainer/MainMargin/Settings/VBoxContainer/DirectoryBoxes/FileLocationBox/DirectoryContainer/DirectoryTextField");
		
		directory.Text = dir;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
}
