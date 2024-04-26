using Godot;
using System;

public partial class LoginWindow : Window
{
	private const float PercentOfDisplaySafeArea = 0.25f;
	private const int MinimumWidth = 450;
	private const int MinimumHeight = 500;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.CloseRequested += this.Hide;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ResizeWindow()
	{
		var window = GetWindow();
		var display = DisplayServer.GetDisplaySafeArea();
		window.ContentScaleSize = new Vector2I(MinimumWidth, MinimumHeight);

		//If the monitor resolution is more than double the size, set it PercentOfDisplaySafeArea size
		if (display.Size.X / 2 > MinimumWidth)
		{
			var ratio = (display.Size.X * PercentOfDisplaySafeArea) / MinimumWidth;
			var length = MinimumWidth * ratio;
			var width = MinimumHeight * ratio;

			//Resize the window
			window.Size = new Vector2I((int)length, (int)width);

			//Calculate the new position of the window. Center the window on the screen.
			//The position is calculated by subtracting the window size from the display size and dividing by 2.
			window.Position = new Vector2I((int)(display.Size.X - length) / 2, (int)(display.Size.Y - width) / 2);
		}
	}
}
