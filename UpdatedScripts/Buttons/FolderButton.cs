using Godot;
using System;

public partial class FolderButton : Button
{
	private FileDialogElement _fileDialog;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_fileDialog = GetNode<FileDialogElement>("/root/LauncherWindow/FileDialogElement");
		this.ButtonDown += OnButtonDown;
	}

	private void OnButtonDown()
	{
		//_fileDialog.ResizeWindow();
		//_fileDialog.Show();
		_fileDialog.Popup();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
