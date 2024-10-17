using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher.UpdatedScripts.Contracts;
using ToteschaMinecraftLauncher.UpdatedScripts.Controllers;
using ToteschaMinecraftLauncher.UpdatedScripts.Exceptions;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Scenes
{
    public partial class MainControl : Control
    {

        private static readonly SettingsController _settingsController = new SettingsController();
        private static readonly ModpackController _modpackController = new ModpackController();
        private static readonly LoginController _loginController = new LoginController();
        private static readonly MinecraftController _minecraftController = new MinecraftController();
        private static readonly WebController _webController = new WebController();
        private string? _settingsDirectory;

        public override async void _Ready()
        {
            _settingsDirectory = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ToteschaMinecraft_Ender");
            LoadSettings();
        }
        public void OnShutdown() => SaveSettings();
        public ToteschaSettings GetSettings() => _settingsController.Settings;
        public void UpdateSettings(ToteschaSettings settings) => _settingsController.UpdateSettings(settings);
        private void LoadSettings() => _settingsController.LoadSettings(_settingsDirectory);
        private void SaveSettings() => _settingsController.SaveSettings(_settingsDirectory);
        public async Task<ToteschaHttpResponse<T>> GetJsonWebRequestAsync<T>(string url) => await _webController.GetJsonWebRequestAsync<T>(url);
        public async Task<ToteschaHttpResponse<ImageTexture>> GetImageDataAsync(string url) => await _webController.GetImageDataAsync(url);
        public async Task<Tuple<ToteschaServerDetails?, string>> GetServerDetailsAsync(bool forceReload = false)=> await _modpackController.GetServerDetailsAsync(_settingsController.Settings.ServerURL, forceReload);     
        public void SetModpack(string modpackName) => _modpackController.SetModpack(modpackName);

        public async Task<bool> TryStartMinecraftAsync(Action<double> updateStatusBar, Action<string> updateStatusText) => 
            await _minecraftController.TryStartMinecraftAsync(_settingsController, _modpackController, _loginController, updateStatusBar, updateStatusText);
    }
}
