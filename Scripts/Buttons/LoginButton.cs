using Godot;
using System;
using ToteschaMinecraftLauncher.Scripts.Logic;

public partial class LoginButton : Button
{
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.ButtonDown += OnLoginClicked;
	}

	private async void OnLoginClicked()
	{
		var thisWindow = GetNode<LauncherWindow>("/root/LauncherWindow");
		var scene = GD.Load<PackedScene>("res://login.tscn");
		var loginwindow = (Window)scene.Instantiate();
		thisWindow.AddChild(loginwindow);
		var position = thisWindow.GetScreenPosition();
		loginwindow.Position= new Vector2I((int)position.X, (int)position.Y);
		//loginwindow.Show();
		Disabled = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
