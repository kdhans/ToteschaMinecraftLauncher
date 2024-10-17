using System.Threading.Tasks;
using Godot;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Scenes
{
	public partial class LauncherWindow : Control
	{
		public static MainControl _mainControl;
		private const int MinimumWidth = 1800;
		private const int MinimumHeight = 900;

		private static string? _sceneName;
		private static Node? _selectedNode;

		//On Load
		public override async void _Ready()
		{
			_mainControl = GetNode<MainControl>("/root/MainControl");

			//Start by disabling all nodes
			DisableNodes(disableAllNodes:true);

			ResizeWindow();
			var thisWindow = GetWindow();
			thisWindow.CloseRequested += OnShutdown;

			//Start on the Home scene
			var homeScene = GD.Load<PackedScene>("res://Home.tscn");
			SetSceneInDisplayArea(homeScene);

			//Once the home scene is loaded, enable all nodes.
			//This will be triggered from the home scene when the server details are loaded.

		}
		//On Shutdown
		private void OnShutdown() => _mainControl.OnShutdown();

		public void SetSceneInDisplayArea(PackedScene scene)
		{
			if (scene.ResourcePath != _sceneName)
				_sceneName = scene.ResourcePath;
			else
				return;

			if (_selectedNode != null)
				_selectedNode.QueueFree();
			_selectedNode = scene.Instantiate();

			var displayArea = GetNode<MarginContainer>("/root/MainControl/LauncherWindow/DisplayAreaContainer/MainMargin");
			displayArea.AddChild(_selectedNode);
		}
		public void DisableNodes(bool? disableAllNodes = null, bool? disableLoginNodes = null, bool? disableModpackNodes = null)
		{
			if (disableAllNodes!= null)
				DisableAllNodes((bool)disableAllNodes);
			if (disableLoginNodes != null)
				DisableLoginNodes((bool)disableLoginNodes);
			if (disableModpackNodes != null)
				DisableModpackNodes((bool)disableModpackNodes);
		}
		private void DisableAllNodes(bool disabled)
		{
			GetNode<Button>("/root/MainControl/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/SettingsButton").Disabled = disabled;
			GetNode<Button>("/root/MainControl/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/DetailsButton").Disabled = disabled;
			GetNode<Button>("/root/MainControl/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/KeybindButton").Disabled = disabled;
			GetNode<Button>("/root/MainControl/LauncherWindow/FooterContainer/LoginMargin/LoginButton").Disabled = disabled;
			GetNode<Button>("/root/MainControl/LauncherWindow/FooterContainer/LaunchButtonContainer/LaunchButton").Disabled = disabled;
		}
		private void DisableModpackNodes(bool disabled)
		{
			GetNode<Button>("/root/MainControl/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/DetailsButton").Disabled = disabled;
			GetNode<Button>("/root/MainControl/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/KeybindButton").Disabled = disabled;
			GetNode<Button>("/root/MainControl/LauncherWindow/FooterContainer/LaunchButtonContainer/LaunchButton").Disabled = disabled;
		}
		private void DisableLoginNodes(bool disabled)
		{
			GetNode<Button>("/root/MainControl/LauncherWindow/FooterContainer/LoginMargin/LoginButton").Disabled = disabled;
			GetNode<Button>("/root/MainControl/LauncherWindow/FooterContainer/LaunchButtonContainer/LaunchButton").Disabled = disabled;
		}

		public void UpdateStatusBar(double loadingBarValue = 0.0)
		{
			GetNode<ProgressBar>("/root/MainControl/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar").StopInfiniteLoading();

			if (loadingBarValue >= 0)
				GetNode<ProgressBar>("/root/MainControl/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar").Value = loadingBarValue;
			else
				GetNode<ProgressBar>("/root/MainControl/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar").StartInfiniteLoading();
		}
		public void UpdateStatusText(string statusText) => GetNode<Label>("/root/MainControl/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressLabel").Text = statusText;

		public async Task LaunchMinecraftAsync()
		{
			DisableNodes(disableAllNodes: true);
			var result = await _mainControl.TryStartMinecraftAsync(UpdateStatusBar, UpdateStatusText);
			DisableNodes(disableAllNodes: false);
		}
		private void ResizeWindow()
		{
			var window = GetWindow();
			window.ContentScaleSize = new Vector2I(MinimumWidth, MinimumHeight);


			var displayDPI = DisplayServer.ScreenGetDpi();
			var display = DisplayServer.GetDisplaySafeArea();
			var developerDPI = 120;
			var displaySizeOnStandardScreen = ((float)MinimumWidth / (float)developerDPI);


			if (displayDPI > developerDPI)
			{
				var displaySizeOnPlayerScreen = ((float)MinimumWidth / (float)displayDPI);
				var ratio = (1f - displaySizeOnPlayerScreen / displaySizeOnStandardScreen) + 1f;

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
}
