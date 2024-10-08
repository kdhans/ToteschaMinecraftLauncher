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
//        //Lock window elements during login
//        //Lock window elements during modpack download

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
