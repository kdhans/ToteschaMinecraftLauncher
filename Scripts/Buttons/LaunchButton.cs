using Godot;
using System;

public partial class LaunchButton : TextureButton
{
	private LauncherWindow window;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		this.Pressed += OnButtonPressed;
		window = GetNode<LauncherWindow>("/root/LauncherWindow");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public async void OnButtonPressed()
	{
		await window.TryLaunchMinecraft();
	}
}
