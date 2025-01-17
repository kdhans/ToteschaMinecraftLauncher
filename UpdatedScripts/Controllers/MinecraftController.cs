using CmlLib.Core.Installer.FabricMC;
using CmlLib.Core.VersionMetadata;
using CmlLib.Core;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher.Scripts.Contracts;
using ToteschaMinecraftLauncher.Scripts.Logic;
using ToteschaMinecraftLauncher.UpdatedScripts.Contracts;
using ToteschaMinecraftLauncher.UpdatedScripts.Scenes;
using CmlLib.Core.Installer;
using CmlLib.Core.ModLoaders.FabricMC;
using CmlLib.Core.Installer.Forge;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Controllers
{
    public class MinecraftController
    {

        private Modpack _modpack;
        private ToteschaSettings _settings;
        private double _totalInstallProgress = 0.0f;
        private LauncherWindow _window = null;
        private MinecraftPath _minecraftPath;
        private string _versionName;
        private MinecraftLauncher _launcher;
        private System.Net.Http.HttpClient _httpClient;

        internal async Task<bool> TryStartMinecraftAsync(SettingsController settingsController, ModpackController modpackController, LoginController loginController, Action<double> updateStatusBar, Action<string> updateStatusText)
        {
            throw new NotImplementedException();
        }
        private void SendUpdateToWindow(double installationPercentage, string installationStatus)
        {
            _window.UpdateStatusBar(installationPercentage);
            _window.UpdateStatusText(installationStatus);
        }

        public async Task<bool> TryLaunchMinecraft()
        {
            var modpackSuccessfullyInstalled = false;
            var installationPath = (_settings.DownloadOnlyServerFiles) ? Path.Combine(_settings.MinecraftInstallationPath, "Server") : _settings.MinecraftInstallationPath;

            try
            {
                ForceReupdateOfModpack(installationPath);
                SendUpdateToWindow(-1, "Starting install..");
                await InstallModpack(installationPath);

                if (!_settings.DownloadOnlyServerFiles)
                {
                    SendUpdateToWindow(_totalInstallProgress, "Getting Minecraft Setup..");
                    modpackSuccessfullyInstalled = await LaunchMinecraft();
                }
                else
                    modpackSuccessfullyInstalled = true;
            }
            catch (Exception ex)
            {
                SendUpdateToWindow(0, $"Could not launch Minecraft: {ex.Message}");
            }

            return modpackSuccessfullyInstalled;
        }

        private async Task InstallModpack(string minecraftInstallationPath)
        {
            var minecraftPath = Path.Combine(minecraftInstallationPath, _modpack.Name);
            GD.Print(minecraftPath);


            if (!Directory.Exists(minecraftPath))
            {
                var result = Directory.CreateDirectory(minecraftPath);
                GD.Print(result.FullName);
            }

            if (!_settings.DownloadOnlyServerFiles)
            {
                _minecraftPath = new MinecraftPath(minecraftPath);
                await HandleModloaderAsync();
            }

            RemoveFilesNotAssociatedWithModpack(minecraftInstallationPath);
            _totalInstallProgress = .25f;
            SendUpdateToWindow(_totalInstallProgress, $"Downloading and installing modpack files...");
            await DownloadModpackFiles(minecraftInstallationPath);

        }
        private async Task HandleModloaderAsync()
        {
            switch (_modpack.ModLoader.ToLower().Trim())
            {
                case "fabric":
                    await InstallFabric();
                    break;
                case "forge":
                    await InstallForge();
                    break;
                case "neoforge":
                    await InstallNeoForge();
                    break;
                case "vanilla":
                    await InstallVanilla();
                    break;
                default:
                        throw new NotImplementedException($"Modloader {_modpack.ModLoader} not supported");
            }
        }

        private async Task InstallNeoForge()
        {
            throw new NotImplementedException();
        }

        private async Task InstallForge()
        {
            string selectForgeVersion = null;
            var forgeInstaller = new ForgeInstaller(_launcher);
            if (!string.IsNullOrWhiteSpace(_modpack.ModLoaderVersion))
                selectForgeVersion = forgeInstaller.

            throw new NotImplementedException();
        }

        private async Task InstallVanilla()
        {
            var mineceaftVersions = await _launcher.GetAllVersionsAsync();
            _versionName = mineceaftVersions.First(version => version.Name == _modpack.MineceaftVersion).Name;
        }
        private async Task InstallFabric()
        {
            string selectFabricVersion = null;
            var fabricInstaller = new FabricInstaller(_httpClient);
            if (!string.IsNullOrWhiteSpace(_modpack.ModLoaderVersion))
                selectFabricVersion = await fabricInstaller.Install(_modpack.MineceaftVersion, _modpack.ModLoaderVersion, _minecraftPath);
            else
                selectFabricVersion = await fabricInstaller.Install(_modpack.MineceaftVersion, _minecraftPath);

            if (string.IsNullOrWhiteSpace(selectFabricVersion))
                throw new KeyNotFoundException($"Fabric version not found: {_modpack.MineceaftVersion}");
            
            _versionName = selectFabricVersion;
        }
        private async Task DownloadModpackFiles(string minecraftInstallationPath)
        {
            if (_modpack.Files?.Any() ?? false)
            {
                SendUpdateToWindow(_totalInstallProgress, $"Downloading Mods...");
                var modpackPath = Path.Combine(minecraftInstallationPath, _modpack.Name);
                var progressAmount = .5f / (float)_modpack.Files.Count;
                var filesToDownload = (_settings.DownloadOnlyServerFiles) ? _modpack.Files.Where(x => x.ServerSide) : _modpack.Files.Where(x => x.ClientSide);
                if (filesToDownload?.Any() ?? false)
                    await Task.WhenAll(filesToDownload.Select(file => DownloadFileAsync(file.DownloadURL,
                                                                                 file.Filename,
                                                                                 modpackPath,
                                                                                 file.InstallationLocation,
                                                                                 progressAmount,
                                                                                 file.RequiresZipExtraction)));

            }
            else
                _totalInstallProgress += .5f;
        }
        async Task DownloadFileAsync(string url, string filename, string mainDirectory, string downloadPath, float progressAmount, bool requiresExtraction)
        {
            var installationPath = string.IsNullOrWhiteSpace(downloadPath) ?
                                    mainDirectory :
                                    Path.Combine(mainDirectory, downloadPath);

            var installationFile = Path.Combine(installationPath, filename);

            if (!Directory.Exists(installationPath))
                Directory.CreateDirectory(installationPath);

            if (File.Exists(installationFile))
            {
                if (_settings.ForceDownload)
                    File.Delete(installationFile);
                else
                {
                    _totalInstallProgress += progressAmount;
                    return;
                }
            }

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Create(installationFile);
            await contentStream.CopyToAsync(fileStream);
            if (requiresExtraction)
                ExtractZipFile(installationFile, installationPath);
            _totalInstallProgress += progressAmount;

        }
        private async Task<bool> LaunchMinecraft()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            System.Diagnostics.Process process;
            bool launched = false;
            var launcher = new CMLauncher(_minecraftPath);
            launcher.ProgressChanged += OnLauncherProgressChanged;
            launcher.FileChanged += OnLauncherFileChanged;
            if (!string.IsNullOrEmpty(Session?.accessToken))
                process = await launcher.CreateProcessAsync(_versionName.Name, new MLaunchOption()
                {
                    MaximumRamMb = (int)_settings.MemoryToAllocate,
                    //#TODO: Setup new login logic here
                    Session = new CmlLib.Core.Auth.MSession()
                    {
                        AccessToken = Session.accessToken,
                        Username = Session.username,
                        UserType = Session.userType,
                        UUID = Session.uuid,
                        Xuid = Session.xboxUserId
                    }
                });
            else
                process = await launcher.CreateProcessAsync(_versionName.Name, new MLaunchOption()
                {
                    MaximumRamMb = (int)_settings.MemoryToAllocate
                });
            process.StartInfo.UseShellExecute = true;
            if (isMac)
            {
                var javaRuntimePath = process.StartInfo.FileName.Replace("/bin/java", "/jre.bundle/Contents/Home/bin/java");
                process.StartInfo.FileName = javaRuntimePath;
            }

            process.Start();
            return await CheckForMinecraftWindow(launched, isMac, process);
        }
        private async Task<bool> CheckForMinecraftWindow(bool launched, bool isMac, System.Diagnostics.Process minecraft)
        {
            int wait = 0, waitInterval = 100; //100 ms = .1 sec
            int totalWaitCount = 60 * 10 * waitInterval * 10; //10 minutes wait max

            SendUpdateToWindow(-1, $"Starting minecraft...");
            while (!launched && wait < totalWaitCount && !minecraft.HasExited)
            {

                launched = (isMac) ? (!minecraft.HasExited && wait > (waitInterval * 10 * 20)) :
                            (minecraft.MainWindowHandle != IntPtr.Zero);
                await Task.Delay(waitInterval);
                minecraft.Refresh();
                wait += waitInterval;
            }

            return launched;
        }
        private void OnLauncherFileChanged(CmlLib.Core.Downloader.DownloadFileChangedEventArgs e)
        {
            _currentLauncherStatus = $"Configuring: {e.FileName} ({e.ProgressedFileCount}/{e.TotalFileCount})";
            SendUpdateToWindow(_totalInstallProgress, _currentLauncherStatus);
        }
        private void OnLauncherProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            float amount = e.ProgressPercentage;
            _totalInstallProgress = .75f + (amount / 400);
            SendUpdateToWindow(_totalInstallProgress, _currentLauncherStatus);
        }
        private void ExtractZipFile(string zipPath, string extractPath)
        {
            if (zipPath.EndsWith("zip"))
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            else if (zipPath.EndsWith("tar.gz"))
                TarGzReader.ExtractTarGz(zipPath, extractPath);
        }

        private void RemoveFilesNotAssociatedWithModpack(string minecraftInstallationPath)
        {
            if (InstalledModpacks == null || InstalledModpacks?.Count == 0)
                return;

            var installedModpack = InstalledModpacks.SingleOrDefault(x => x.ID == _modpack.ID);
            if (installedModpack == null)
                return;

            var installedFiles = new Dictionary<string, string>(installedModpack.Files.Select(x => new KeyValuePair<string, string>(x.Filename, x.InstallationLocation)));
            var toBeInstalledFiles = new Dictionary<string, string>(_modpack.Files.Select(x => new KeyValuePair<string, string>(x.Filename, x.InstallationLocation)));

            foreach (var file in installedFiles)
            {
                var path = string.IsNullOrWhiteSpace(file.Value) ?
                           Path.Combine(minecraftInstallationPath, _modpack.Name, file.Key) :
                           Path.Combine(minecraftInstallationPath, _modpack.Name, file.Value, file.Key);

                if (!toBeInstalledFiles.ContainsKey(file.Key) && File.Exists(path))
                    File.Delete(path);

            }
        }

        private void ForceReupdateOfModpack(string installationPath)
        {
            if (!(_settings?.ForceDownload ?? false))
                return;

            if (!Directory.Exists(installationPath))
                return;

            var name = _modpack?.Name;
            var modpacks = Directory.GetDirectories(installationPath);

            if (!modpacks.Any() || name == null)
                return;

            foreach (var modpack in modpacks)
            {
                var folder = modpack.Split(Path.DirectorySeparatorChar).Last();
                if (string.IsNullOrWhiteSpace(folder))
                    continue;

                if (folder == name)
                    Directory.Delete(modpack, true);
            }
        }
        public void Dispose()
        {
            ((IDisposable)_httpClient).Dispose();
        }
    }
}
