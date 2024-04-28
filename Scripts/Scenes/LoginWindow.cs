using Godot;
using System;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher.Scripts.UIHelpers;

public partial class LoginWindow : Window
{
	public string Username { get; private set; }
	public string Password { get; private set; }

	private const float PercentOfDisplaySafeArea = 0.25f;
	private const int MinimumWidth = 450;
	private const int MinimumHeight = 550;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.CloseRequested += () => _ = HideAndReenableLoginButton(false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ResizeWindow()
	{
		var window = GetWindow();
		window.ContentScaleSize = new Vector2I(MinimumWidth, MinimumHeight);
		
		
		var displayDPI = DisplayServer.ScreenGetDpi();
		var display = DisplayServer.GetDisplaySafeArea();
		var standardDPI = 120;		

		var displaySizeOnStandardScreen = ((float)MinimumWidth/(float)standardDPI);


		if (displayDPI > standardDPI)
		{
			var actualScreenEquity = ((float)MinimumWidth/(float)displayDPI);
			var ratio = 1+ actualScreenEquity/displaySizeOnStandardScreen;

			var length = MinimumWidth * ratio;
			var width = MinimumHeight * ratio;

			//Resize the window
			window.Size = new Vector2I((int)length, (int)width);

			//Calculate the new position of the window. Center the window on the screen.
			//The position is calculated by subtracting the window size from the display size and dividing by 2.
			window.Position = new Vector2I((int)(display.Size.X - length) / 2, (int)(display.Size.Y - width) / 2);
		}

		var loginButton = GetNode<LWLoginButton>("/root/LauncherWindow/LoginWindow/VBoxContainer/ButtonGroup/LWLoginButton");
		loginButton.ResetLoginButton();
	}

	public async Task HideAndReenableLoginButton(bool isLoginSuccessful)
	{
		var loginButton = GetNode<Button>("/root/LauncherWindow/FooterContainer/LoginMargin/LoginButton");
		if (isLoginSuccessful)
		{
			var encryptor = new ToteschaEncryptor();
			var username = GetNode<LineEdit>("/root/LauncherWindow/LoginWindow/VBoxContainer/UsernameGroup/LWUsernameTextBox").Text;
			var password = GetNode<LineEdit>("/root/LauncherWindow/LoginWindow/VBoxContainer/PasswordGroup/LWPasswordTextBox").Text;
			Username = await encryptor.EncryptStringAsync(username);
			Password = await encryptor.EncryptStringAsync(password);
			GetNode<LineEdit>("/root/LauncherWindow/LoginWindow/VBoxContainer/UsernameGroup/LWUsernameTextBox").Text = string.Empty;
			GetNode<LineEdit>("/root/LauncherWindow/LoginWindow/VBoxContainer/PasswordGroup/LWPasswordTextBox").Text = string.Empty;
		}

		loginButton.Disabled = false;
		this.Hide();
	}
	public async Task SetUsernameAndPassword(string username, string password)
	{
		var encryptor = new ToteschaEncryptor();
		try
		{
			GetNode<LineEdit>("/root/LauncherWindow/LoginWindow/VBoxContainer/UsernameGroup/LWUsernameTextBox").Text =
				await encryptor.DecryptStringAsync(username);
			GetNode<LineEdit>("/root/LauncherWindow/LoginWindow/VBoxContainer/PasswordGroup/LWPasswordTextBox").Text =
				await encryptor.DecryptStringAsync(password);
		}
		catch
		{
			GetNode<LauncherWindow>("/root/LauncherWindow").ToteschaSettings.Username = string.Empty;
			GetNode<LauncherWindow>("/root/LauncherWindow").ToteschaSettings.Password = string.Empty;
		}
	}
	public string SetErrorText(string errorText) => GetNode<Label>("VBoxContainer/ErrorText").Text = errorText;
}
