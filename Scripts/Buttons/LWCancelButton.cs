using Godot;
using System;

public partial class LWCancelButton : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.ButtonDown += OnCancelPressed;
	}

	private void OnCancelPressed()
	{
		var loginWindow = GetNode<LoginWindow>("/root/LauncherWindow/LoginWindow");
		_ = loginWindow.HideAndReenableLoginButton(false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
