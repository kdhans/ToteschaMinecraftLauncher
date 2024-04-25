using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class ProgressBar : Godot.ProgressBar
{
	private CancellationTokenSource cts;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public async Task StartInfiniteLoading()
	{
		cts = new CancellationTokenSource();
		var valueToAdd = 3;
		Value = 0;
		FillMode = (int)FillModeEnum.BeginToEnd;
		while (!cts.IsCancellationRequested)
		{	
			if (Value >= 100)
			{
				valueToAdd = -3;
				FillMode = (int)FillModeEnum.EndToBegin;
			}
			else if (Value <= 0)
			{
				valueToAdd = 3;
				FillMode = (int)FillModeEnum.BeginToEnd;
			}
			
			this.Value += valueToAdd;
			await Task.Delay(25);
		}
	}
	public void StopInfiniteLoading()
	{
		Value = 0;
		FillMode = (int)FillModeEnum.BeginToEnd;
		cts?.Cancel();
	}
}
