using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;

public partial class PageLoadingBar : TextureProgressBar
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
		this.Visible = true;
		cts = new CancellationTokenSource();
		var valueToAdd = 2;
		var radialValueToAdd = 5;
		while (!cts.IsCancellationRequested)
		{
			
			this.RadialInitialAngle = this.RadialInitialAngle >= 360 ? radialValueToAdd : this.RadialInitialAngle += radialValueToAdd;
			if (this.Value >=100 && valueToAdd >= 0)
			{
				valueToAdd = -2;
				radialValueToAdd = 10;
			}
			else if (this.Value <= 0 && valueToAdd <= 0)
			{
				valueToAdd = 2;
				radialValueToAdd = 5;
			}

			this.Value += valueToAdd;


			await Task.Delay(25);
		}
	}

	public void StopInfiniteLoading()
	{
		this.Visible = false;
		cts?.Cancel();
	}
}
