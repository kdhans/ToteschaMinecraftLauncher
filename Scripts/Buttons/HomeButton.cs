using Godot;
using System;

public partial class HomeButton : Button
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		this.Pressed += OnButtonPressed;
		OnButtonPressed();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnButtonPressed()
	{
		var relatedScene = GD.Load<PackedScene>("res://Home.tscn");
		
		var launcherWindow = GetNode<LauncherWindow>("/root/LauncherWindow");
		var parentNode = GetNode<MarginContainer>(LauncherWindow.DisplayBoxNodePath);
		launcherWindow.SetupButtonScene(relatedScene, parentNode);
	}
}
