using CmlLib.Core;
using CmlLib.Core.Installer.FabricMC;
using CmlLib.Core.VersionMetadata;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher.Scripts.Contracts;
using static ToteschaMinecraftLauncher.Scripts.UIHelpers.LoginHelper;

namespace ToteschaMinecraftLauncher.Scripts.Logic
{
    public class MinecraftInstanceLogic : IDisposable
    {
        private System.Net.Http.HttpClient _httpClient;
        private MinecraftPath _minecraftPath;
        private MVersionMetadata _versionMetadata;
        public event EventHandler<InstallationEventArgs> InstallationProgress;
        private float _totalInstallProgress = 0.0f;
        private string _currentLauncherStatus = string.Empty;

        public Modpack Modpack { get; set; }
        public ToteschaSettings Settings { get; set; }
        public MinecraftSession Session { get; set; }
        public MinecraftInstanceLogic()
        {
            _httpClient = new System.Net.Http.HttpClient();
        }

        public async Task<bool> TryLaunchMinecraft()
        {
            var modpackSuccessfullyInstalled = false;
            try
            {
                GD.Print("Starting install");
                InstallationProgress?.Invoke(this, new InstallationEventArgs(-1, "Starting install.."));
                await InstallModpack();
                InstallationProgress?.Invoke(this, new InstallationEventArgs(_totalInstallProgress, "Getting Minecraft Setup.."));
                modpackSuccessfullyInstalled = await LaunchMinecraft();
            }
            catch (Exception ex)
            {
                InstallationProgress?.Invoke(this, new InstallationEventArgs(0, $"Could not launch Minecraft: {ex.Message}"));
            }
            return modpackSuccessfullyInstalled;
        }

        private async Task InstallModpack()
        {

            GD.Print("Checking Installation Path");
            if (!Directory.Exists(Settings.MinecraftInstallationPath))
                Directory.CreateDirectory(Settings.MinecraftInstallationPath);


            _minecraftPath = new MinecraftPath(Settings.MinecraftInstallationPath);

            GD.Print("Handling Mod Loader");
            await HandleModloaderAsync();
            _totalInstallProgress = .25f;
            InstallationProgress?.Invoke(this, new InstallationEventArgs(_totalInstallProgress, $"Downloading and installing modpack files..."));

            GD.Print("Downloading Files");
            await DownloadModpackFiles();

        }
        private async Task HandleModloaderAsync()
        {
            if (Modpack.ModLoader.ToLower().Trim() == "fabric")
                await InstallFabric();
            else if (Modpack.ModLoader.ToLower().Trim() != "vanilla")
                throw new NotImplementedException($"Modloader {Modpack.ModLoader} not supported");
        }
        private async Task InstallFabric()
        {
            GD.Print("Installing fabric");
            var fabricVersionLoader = new FabricVersionLoader();
            var fabricVersions = await fabricVersionLoader.GetVersionMetadatasAsync();
            var selectFabricVersion = fabricVersions.Where(version => version.Name.EndsWith(Modpack.MineceaftVersion)).FirstOrDefault();
            if (selectFabricVersion == null)
                throw new KeyNotFoundException($"Fabric version not found: {Modpack.MineceaftVersion}");

            InstallationProgress?.Invoke(this, new InstallationEventArgs(-1, $"Downloading and installing Fabric {selectFabricVersion.Name}..."));
            var fabric = fabricVersions.GetVersionMetadata(selectFabricVersion.Name);
            await fabric.SaveAsync(_minecraftPath);
            _versionMetadata = selectFabricVersion;
        }
        private async Task DownloadModpackFiles()
        {
            if (Modpack.Files?.Any() ?? false)
            {

                GD.Print("Mods are downloading");
                var progressAmount = .5f / (float)Modpack.Files.Count;
                var filesToDownload = (Settings.DownloadServerFiles) ? Modpack.Files : Modpack.Files.Where(x => !x.ServerSide);
                if (filesToDownload?.Any() ?? false)
                {

            GD.Print("Mods are found to download");
                    await Task.WhenAll(filesToDownload.Select(file => DownloadFileAsync(file.DownloadURL,
                                                                                 file.Filename,
                                                                                 Settings.MinecraftInstallationPath,
                                                                                 file.InstallationLocation,
                                                                                 progressAmount)));
                }
            }
            else
                _totalInstallProgress += .5f;
        }
        async Task DownloadFileAsync(string url, string filename, string mainDirectory, string downloadPath, float progressAmount)
        {
            InstallationProgress?.Invoke(this, new InstallationEventArgs(_totalInstallProgress, $"Downloading {filename}..."));
            GD.Print($"Installing {mainDirectory} {downloadPath} {filename}");
            var installationPath = string.IsNullOrWhiteSpace(downloadPath) ?
                                    mainDirectory : 
                                    Path.Combine(mainDirectory, downloadPath);
            GD.Print("Installation path: " + installationPath);                       
            var installationFile = Path.Combine(installationPath, filename);

            GD.Print("Installation file: " + installationFile);                   
            if (Directory.Exists(installationPath))
                Directory.CreateDirectory(installationPath);
            if (File.Exists(installationFile))
            {
                if (Settings.ForceDownload)
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
            _totalInstallProgress += progressAmount;

        }
        private async Task<bool> LaunchMinecraft()
        {
            var launchOption = new MLaunchOption()
            {
                MaximumRamMb = (int)Settings.MemoryToAllocate,
                DockName = $"Totescha Minecraft {Modpack.MineceaftVersion}",
                Session = new CmlLib.Core.Auth.MSession()
                {
                    AccessToken = Session.accessToken,
                    Username = Session.username,
                    UserType = Session.userType,
                    UUID = Session.uuid,
                    Xuid = Session.xboxUserId
                }
            };
            GD.Print($"Max {(int)Settings.MemoryToAllocate}");
            System.Diagnostics.Process process;
            var launcher = new CMLauncher(_minecraftPath);
            launcher.ProgressChanged += OnLauncherProgressChanged;
            launcher.FileChanged += OnLauncherFileChanged;
            if (!string.IsNullOrEmpty(Session?.accessToken))
                process = await launcher.CreateProcessAsync(_versionMetadata.Name, launchOption);
            else
                process = await launcher.CreateProcessAsync(_versionMetadata.Name, new MLaunchOption()
                {
                    MaximumRamMb = (int)Settings.MemoryToAllocate,
                    DockName = $"Totescha Minecraft {Modpack.MineceaftVersion}"
                });
            GD.Print($"Process: {process.StartInfo.FileName} Parameters: {process.StartInfo.Arguments}");
            return process.Start();
        }

        private void OnLauncherFileChanged(CmlLib.Core.Downloader.DownloadFileChangedEventArgs e)
        {
            _currentLauncherStatus = $"Configuring: {e.FileName} ({e.ProgressedFileCount}/{e.TotalFileCount})";
            InstallationProgress?.Invoke(this, new InstallationEventArgs(_totalInstallProgress, _currentLauncherStatus));
        }

        private void OnLauncherProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            float amount = e.ProgressPercentage;
            _totalInstallProgress = .75f + (amount/400);
            InstallationProgress?.Invoke(this, new InstallationEventArgs(_totalInstallProgress, _currentLauncherStatus));
        }

        public void Dispose()
        {
            ((IDisposable)_httpClient).Dispose();
        }
    }
}
