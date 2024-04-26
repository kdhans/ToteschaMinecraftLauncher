using Godot;
using System;

public partial class LWLoginButton : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.ButtonDown += OnLoginButtonPressed;
	}

	private async void OnLoginButtonPressed()
	{
		var mainWindow = GetNode<LauncherWindow>("/root/LauncherWindow");
		var loginWindow = GetNode<LoginWindow>("/root/LauncherWindow/LoginWindow");
		await loginWindow.HideAndReenableLoginButton(true);
		mainWindow.ToteschaSettings.Username = loginWindow.Username;
		mainWindow.ToteschaSettings.Password = loginWindow.Password;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
