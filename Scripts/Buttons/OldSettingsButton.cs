using Godot;
using System;

public partial class OldSettingsButton : Button
{
	// Called when the node enters the scene tree for the first time
	public override void _Ready()
	{
		base._Ready();
		this.Pressed += OnButtonPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnButtonPressed()
	{
		var relatedScene = GD.Load<PackedScene>("res://Settings.tscn");
		
		var launcherWindow = GetNode<OldLauncherWindow>("/root/LauncherWindow");
		var parentNode = GetNode<MarginContainer>(OldLauncherWindow.DisplayBoxNodePath);
		launcherWindow.SetupSceneForButton(relatedScene, parentNode);
	}
}
