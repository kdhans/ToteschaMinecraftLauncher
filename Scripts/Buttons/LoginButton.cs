using Godot;
using System;
using System.Threading.Tasks;

public partial class LoginButton : Button
{
	LoginWindow loginWindow;
	OldLauncherWindow launcherWindow;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		launcherWindow = GetNode<OldLauncherWindow>("/root/LauncherWindow");
		loginWindow = GetNode<LoginWindow>("/root/LauncherWindow/LoginWindow");
		this.ButtonDown += OnLoginClickedAsync;
	}
	public async void ClickButtonAsync() => OnLoginClickedAsync();

	private async void OnLoginClickedAsync()
	{
		var username = launcherWindow.ToteschaSettings.Username;
		var password = launcherWindow.ToteschaSettings.Password;

		if (!string.IsNullOrWhiteSpace(username) && 
			!string.IsNullOrWhiteSpace(password)) 
		{
			await loginWindow.SetUsernameAndPassword(username, password);
		}
		loginWindow.UserFinishedUp = false;
		loginWindow.ResizeWindow();
		loginWindow.Show();
		Disabled = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
