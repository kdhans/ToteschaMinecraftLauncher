using Godot;
using System;

public partial class ButtonContainer : HBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var parent = GetParent<ScrollContainer>();
		this.MouseEntered += () => parent.HorizontalScrollMode = ScrollContainer.ScrollMode.Auto;
		this.MouseExited += () => parent.HorizontalScrollMode = ScrollContainer.ScrollMode.ShowNever;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
