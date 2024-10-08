using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using ToteschaMinecraftLauncher.UpdatedScripts.Controllers;
using ToteschaMinecraftLauncher.UpdatedScripts.Contracts;
using System.IO;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Scenes
{
	public partial class LauncherWindow : Control
	{
		private readonly SettingsController _settingsController = new SettingsController();
		private const int MinimumWidth = 1800;
		private const int MinimumHeight = 900;
		private string? _settingsDirectory;

		private static string? _sceneName;
		private static Node? _selectedNode;

		//On Load
		public override async void _Ready()
		{
			_settingsDirectory = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ToteschaMinecraft_Ender");
			//_settingsController.LoadSettings(_settingsDirectory, out ToteschaSettings settings);
			ResizeWindow();
			//await _webController.LoadServerDetailsAsync(settings.ServerURL);

			var thisWindow = GetWindow();
			thisWindow.CloseRequested += OnShutdown;

		}
		//On Shutdown
		private void OnShutdown()
		{
			//_settingsController.SaveSettings(_settingsDirectory);
		}


		public void LockNodes(bool? lockAllNodes = null, bool? lockModpackNodes = null, bool? lockLoginNodes = null)
		{
			if (lockAllNodes!= null)

        }

		private void LockAllNodes(bool lockElements)
		{
			GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/SettingsButton").Disabled = lockElements;
			GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/DetailsButton").Disabled = lockElements;
            GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/KeybindButton").Disabled = lockElements;
            GetNode<Button>("/root/LauncherWindow/FooterContainer/LoginMargin/LoginButton").Disabled = lockElements;
            GetNode<Button>("/root/LauncherWindow/FooterContainer/LaunchButtonContainer/LaunchButton").Disabled = lockElements;
        }
        private void LockModpackNodes(bool lockElements)
        {
            GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/DetailsButton").Disabled = lockElements;
            GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/KeybindButton").Disabled = lockElements;
            GetNode<Button>("/root/LauncherWindow/FooterContainer/LaunchButtonContainer/LaunchButton").Disabled = lockElements;
        }
        private void LockLoginNodes(bool lockElements)
		{
            GetNode<Button>("/root/LauncherWindow/FooterContainer/LoginMargin/LoginButton").Disabled = lockElements;
            GetNode<Button>("/root/LauncherWindow/FooterContainer/LaunchButtonContainer/LaunchButton").Disabled = lockElements;
        }
		public void UpdateStatusBar(double loadingBarValue = 0.0)
		{
			GetNode<ProgressBar>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar").StopInfiniteLoading();

			if (loadingBarValue >= 0)
				GetNode<ProgressBar>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar").Value = loadingBarValue;
			else
				GetNode<ProgressBar>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar").StartInfiniteLoading();
		}
        public void UpdateStatusText(string statusText) => GetNode<Label>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressLabel").Text = statusText;



        //Handle updating scene when a menu button is clicked
        public void SetSceneInDisplayArea(PackedScene scene)
		{
			if (scene.ResourcePath != _sceneName)
				_sceneName = scene.ResourcePath;
			else
				return;

			if (_selectedNode != null)
				_selectedNode.QueueFree();
			_selectedNode = scene.Instantiate();

			var displayArea = GetNode<MarginContainer>("/root/LauncherWindow/DisplayAreaContainer/MainMargin");
			displayArea.AddChild(_selectedNode);
		}

		//Handle Window Resize
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
