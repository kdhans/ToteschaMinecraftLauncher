using Godot;
using System;
using static ToteschaMinecraftLauncher.Scripts.UIHelpers.LoginHelper;

public partial class LWLoginButton : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.ButtonDown += OnLoginButtonPressed;
	}

	private async void OnLoginButtonPressed()
	{
		var mainWindow = GetNode<OldLauncherWindow>("/root/LauncherWindow");
		var loginWindow = GetNode<LoginWindow>("/root/LauncherWindow/LoginWindow");

		loginWindow.SetErrorText(string.Empty);
		this.Text = "Logging in...";
		this.Disabled = true;
		try
		{
			var result = await mainWindow.TryLoginToMinecraft(GetNode<LineEdit>("/root/LauncherWindow/LoginWindow/VBoxContainer/UsernameGroup/LWUsernameTextBox").Text,
										   GetNode<LineEdit>("/root/LauncherWindow/LoginWindow/VBoxContainer/PasswordGroup/LWPasswordTextBox").Text,
										   false);
			if (!result.Item1)
				throw new ApplicationException(result.Item2);
			await loginWindow.HideAndReenableLoginButton(true);
		}
		catch (Exception ex)
		{
			ResetLoginButton();
			loginWindow.SetErrorText(ex.Message);
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ResetLoginButton()
	{
		this.Text = "Login";
		this.Disabled = false;
	}
}
