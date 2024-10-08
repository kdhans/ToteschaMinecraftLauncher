//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Godot;
//using ToteschaMinecraftLauncher.UpdatedScripts.Controllers;
//using ToteschaMinecraftLauncher.UpdatedScripts.Contracts;
//using System.IO;

//namespace ToteschaMinecraftLauncher.UpdatedScripts.Scenes
//{
//    public partial class LauncherWindow : Control
//    {
//        private readonly SettingsController _settingsController = new SettingsController();
//        private const int MinimumWidth = 1400;
//        private const int MinimumHeight = 750;
//        private string? _settingsDirectory;

//        //On Load
//        public override async void _Ready()
//        {
//            _settingsDirectory = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ToteschaMinecraft_Ender");
//            _settingsController.LoadSettings(_settingsDirectory, out ToteschaSettings settings);
//            ResizeWindow();
//            await _webController.LoadServerDetailsAsync(settings.ServerURL);

//            var thisWindow = GetWindow();
//            thisWindow.CloseRequested += OnShutdown;

//        }
//        //On Shutdown
//        private void OnShutdown()
//        {
//            _settingsController.SaveSettings(_settingsDirectory);
//        }

//        //Lock window elements during web refreshes
//        private void LockWindowElements(bool lockElements)
//        {
//            GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/SettingsButton").Disabled = lockElements;
//            GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/DetailsButton").Disabled = lockElements;
//            GetNode<Button>("/root/LauncherWindow/FooterContainer/LoginMargin/LoginButton").Disabled = lockElements;
//        }
//        //Update status bar
//        private void UpdateStatusBar(string statusText, double loadingBarValue = 0.0)
//        {
//            GetNode<ProgressBar>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar").StopInfiniteLoading();
//            GetNode<Label>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressLabel").Text = statusText;

//            if (loadingBarValue > 1)
//                GetNode<ProgressBar>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar").Value = loadingBarValue;
//            else
//                GetNode<ProgressBar>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar").StartInfiniteLoading();
//        }

//        //Handle Home Button Click
//        //Handle Pack Details Click
//        //Handle Settings Click
//        //Handle Login Click
//        //Handle Play Click

//        //Handle Window Resize
//        private void ResizeWindow()
//        {
//            OS.WindowSize = new Vector2(1280, 720);
//        }
//    }
//}
