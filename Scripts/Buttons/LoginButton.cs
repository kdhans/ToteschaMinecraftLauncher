using Godot;
using System;
using ToteschaMinecraftLauncher.Scripts.Logic;

public partial class LoginButton : Button
{
	LoginWindow loginWindow;
	LauncherWindow launcherWindow;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		launcherWindow = GetNode<LauncherWindow>("/root/LauncherWindow");
		loginWindow = GetNode<LoginWindow>("/root/LauncherWindow/LoginWindow");
		this.ButtonDown += OnLoginClicked;
	}

	private void OnLoginClicked()
	{
		loginWindow.ResizeWindow();
		loginWindow.Show();
		Disabled = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
