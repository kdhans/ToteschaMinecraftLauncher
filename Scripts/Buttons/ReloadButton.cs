using Godot;
using System;

public partial class ReloadButton : TextureButton
{
	[Signal]
	public delegate void OnRefreshPressedEventHandler();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		this.Pressed += OnButtonPressed;
	}

	private void OnButtonPressed()
	{
		EmitSignal(SignalName.OnRefreshPressed);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
