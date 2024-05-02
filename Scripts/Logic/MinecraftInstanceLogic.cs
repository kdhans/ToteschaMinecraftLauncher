using CmlLib.Core;
using CmlLib.Core.Installer.FabricMC;
using CmlLib.Core.VersionMetadata;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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
        public List<Modpack> InstalledModpacks { get; set; }

        public MinecraftInstanceLogic()
        {
            _httpClient = new System.Net.Http.HttpClient();
        }

        public async Task<bool> TryLaunchMinecraft()
        {
            var modpackSuccessfullyInstalled = false;
            try
            {
                ForceReupdateOfModpack();
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
            var minecraftPath = Path.Combine(Settings.MinecraftInstallationPath, Modpack.Name);
            if (!Directory.Exists(minecraftPath))
                Directory.CreateDirectory(minecraftPath);
            _minecraftPath = new MinecraftPath(minecraftPath);
            await HandleModloaderAsync();
            RemoveFilesNotAssociatedWithModpack();
            _totalInstallProgress = .25f;
            InstallationProgress?.Invoke(this, new InstallationEventArgs(_totalInstallProgress, $"Downloading and installing modpack files..."));
            await DownloadModpackFiles();

        }
        private async Task HandleModloaderAsync()
        {
            if (Modpack.ModLoader.ToLower().Trim() == "fabric")
                await InstallFabric();
            else if (Modpack.ModLoader.ToLower().Trim() != "vanilla")
                throw new NotImplementedException($"Modloader {Modpack.ModLoader} not supported");
            else
                await InstallVanilla();
        }
        private async Task InstallVanilla()
        {
            var mineceaftVersions = await new CMLauncher(_minecraftPath).GetAllVersionsAsync();
            _versionMetadata = mineceaftVersions.First(version => version.Name == Modpack.MineceaftVersion);
        }
        private async Task InstallFabric()
        {
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
                InstallationProgress?.Invoke(this, new InstallationEventArgs(_totalInstallProgress, $"Downloading Mods..."));
                var modpackPath = Path.Combine(Settings.MinecraftInstallationPath, Modpack.Name);
                var progressAmount = .5f / (float)Modpack.Files.Count;
                var filesToDownload = (Settings.DownloadServerFiles) ? Modpack.Files.Where(x=> x.ServerSide) : Modpack.Files.Where(x => x.ClientSide);
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
                process = await launcher.CreateProcessAsync(_versionMetadata.Name, new MLaunchOption()
                {
                    MaximumRamMb = (int)Settings.MemoryToAllocate,
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
                process = await launcher.CreateProcessAsync(_versionMetadata.Name, new MLaunchOption()
                {
                    MaximumRamMb = (int)Settings.MemoryToAllocate
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

            InstallationProgress?.Invoke(this, new InstallationEventArgs(-1, $"Starting minecraft..."));
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
            InstallationProgress?.Invoke(this, new InstallationEventArgs(_totalInstallProgress, _currentLauncherStatus));
        }
        private void OnLauncherProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            float amount = e.ProgressPercentage;
            _totalInstallProgress = .75f + (amount / 400);
            InstallationProgress?.Invoke(this, new InstallationEventArgs(_totalInstallProgress, _currentLauncherStatus));
        }
        private void ExtractZipFile(string zipPath, string extractPath)
        {
            if (zipPath.EndsWith("zip"))
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            else if (zipPath.EndsWith("tar.gz"))
                TarGzReader.ExtractTarGz(zipPath, extractPath);
        }

        private void RemoveFilesNotAssociatedWithModpack()
        {
            if (InstalledModpacks == null || InstalledModpacks?.Count == 0)
                return;

            var installedModpack = InstalledModpacks.SingleOrDefault(x=> x.ID == Modpack.ID);
            if (installedModpack == null)
                return;

            var installedFiles = new Dictionary<string,string>(installedModpack.Files.Select(x=> new KeyValuePair<string, string>(x.Filename, x.InstallationLocation)));
            var toBeInstalledFiles = new Dictionary<string, string>(Modpack.Files.Select(x => new KeyValuePair<string, string>(x.Filename, x.InstallationLocation)));

            foreach (var file in installedFiles)
            {
                var path = string.IsNullOrWhiteSpace(file.Value) ?
                           Path.Combine(Settings.MinecraftInstallationPath, Modpack.Name, file.Key) :
                           Path.Combine(Settings.MinecraftInstallationPath, Modpack.Name, file.Value, file.Key);

                if (!toBeInstalledFiles.ContainsKey(file.Key) && File.Exists(path))
                    File.Delete(path);
            }
        }

        private void ForceReupdateOfModpack()
        {
            if (!(Settings?.ForceDownload ?? false))
                return;

            var name = Modpack?.Name;
            var modpacks = Directory.GetDirectories(Settings!.MinecraftInstallationPath);

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
