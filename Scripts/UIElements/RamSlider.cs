using Godot;
using System;

public partial class RamSlider : HSlider
{
	private OldLauncherWindow launcherWindow;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		launcherWindow = GetNode<OldLauncherWindow>("/root/LauncherWindow");
		var toteschaSettings = launcherWindow.ToteschaSettings;
		this.MaxValue = toteschaSettings.MaxMemory;
		this.Value = toteschaSettings.MemoryToAllocate;
		this.ValueChanged += OnValueChanged;
		OnValueChanged(this.Value);
	}

	private void OnValueChanged(double value)
	{
		var label = GetParent().GetChild<Label>(1);
		label.Text = $" {value/1024} GB";
		launcherWindow.ToteschaSettings.MemoryToAllocate = value;
		this.TooltipText = $"{(value/MaxValue):P} of available RAM";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
