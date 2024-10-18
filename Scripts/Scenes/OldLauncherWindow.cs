using Godot;
using Newtonsoft.Json;
using SevenZip.Compression.LZ;
using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher;
using ToteschaMinecraftLauncher.Scripts.Helpers;
using ToteschaMinecraftLauncher.Scripts.Logic;
using ToteschaMinecraftLauncher.Scripts.UIHelpers;
using static ToteschaMinecraftLauncher.Scripts.UIHelpers.LoginHelper;

#nullable enable
public partial class OldLauncherWindow : Control
{
	public OldToteschaSettings? ToteschaSettings { get; set; }
	public ServerDetails? ServerDetails { get; set; }
	public OldModpack? SelectedModpack { get; set; }
	public MinecraftSession? Session { get; set; }
	public string? LatestNews { get; set; }
	public ImageTexture? LatestNewsPicture { get; set; }
	public bool IsLoadingServerData { get; set; } = true;
	public bool IsLoggingIn { get; set; } = true;

	public static Node? SelectedNode { get; set; }
	public static string DisplayBoxNodePath = "/root/LauncherWindow/DisplayAreaContainer/MainMargin";
	private static string? sceneName = null;
	private const int MinimumWidth = 1400;
	private const int MinimumHeight = 750;
	private string? settingsDirectory;
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		settingsDirectory = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ToteschaMinecraft");
		LoadSettings();
		ResizeAppWindow();
		var window = GetWindow();
		window.CloseRequested += OnCloseRequested;
		await LoadServerDetailsAsync();

		var helperClient = new SystemHelper();
		await TryLoginToMinecraft(ToteschaSettings!.Username, ToteschaSettings!.Password);
	}

	private void OnCloseRequested() => SaveSettings();
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void SetupSceneForButton(PackedScene scene, Node parentNode)
	{
		if (scene.ResourcePath != sceneName)
			sceneName = scene.ResourcePath;
		else
			return;

		if (SelectedNode != null)
			SelectedNode.QueueFree();
		SelectedNode = scene.Instantiate();
		parentNode.AddChild(SelectedNode);
	}
	public async Task LoadServerDetailsAsync()
	{
		IsLoadingServerData = true;
		SetLoadingStateForUI(false);
		var webHelper = GetNode<WebHelper>("WebHelper");
		var result = await webHelper.CallJsonGetRequestAsync<ServerDetails>(ToteschaSettings!.ServerURL);
		string statusText;

		if (result.Error != null)
			statusText = result.Error;
		
		else
		{
			ServerDetails = result.Data;
			statusText = "Select a modpack and press Launch to begin!";
			await GetLatestNews();
		}
		SetLoadingStateForUI(true, statusText);
		IsLoadingServerData = false;
	}
	public void SaveSettings()
	{
		ToteschaSettings!.LastSelectedModpack = SelectedModpack?.Name;

		if (string.IsNullOrWhiteSpace(settingsDirectory))
			return;

		var fileHelper = new FileHelper();
		var filename = Path.Combine(settingsDirectory, "settings.json");

		if (!Directory.Exists(settingsDirectory))
			Directory.CreateDirectory(settingsDirectory);

		try
		{
			var contents = JsonConvert.SerializeObject(ToteschaSettings);
			if (!fileHelper.TryWriteAppTextFile(filename, contents))
				throw new ApplicationException("File did not write");
		}
		catch
		{/*Do nothing*/}
	}
	public void ResizeAppWindow()
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


	public async Task<Tuple<bool, string>> TryLoginToMinecraft(string encyUsername, string encyPassword, bool isEncrypted = true)
	{
		IsLoggingIn = true;
		var loginButton = GetNode<Button>("FooterContainer/LoginMargin/LoginButton");
		Tuple<bool, string> success;
		var encryptor = new ToteschaEncryptor();
		try
		{
			DisableButtons();
			var username = isEncrypted ? await encryptor.DecryptStringAsync(encyUsername) : encyUsername;
			var password = isEncrypted ? await encryptor.DecryptStringAsync(encyPassword) : encyPassword;
			Session = await GetMinecraftSession(username, password);
			success = new Tuple<bool, string>(true, string.Empty);
			IsLoggingIn = false;
			EnableButtons();
			loginButton.Text = $"      Welcome, \n      {Session.username}!";
		}
		catch (Exception ex)
		{
			IsLoggingIn = false;
			EnableButtons();            
			success = new Tuple<bool, string>(false, ex.Message);
		}
		return success;
	}
	public async Task TryLaunchMinecraft()
	{
		bool loaded = false;
		var thisWindow = GetTree();
		SetLoadingStateForUI(false, updateStatusBar: false);

		if (string.IsNullOrWhiteSpace(Session?.accessToken))
			GetNode<LoginButton>("FooterContainer/LoginMargin/LoginButton").ClickButtonAsync();
		var userFinishedUp = GetNode<LoginWindow>("LoginWindow").UserFinishedUp;
		while (string.IsNullOrWhiteSpace(Session?.accessToken) && !userFinishedUp)
		{
			await Task.Delay(100);
			userFinishedUp = GetNode<LoginWindow>("LoginWindow").UserFinishedUp;
		}
		using (var instance = new MinecraftInstanceLogic()
		{
			Session = Session,
			Modpack = SelectedModpack,
			Settings = ToteschaSettings,
			InstalledModpacks = ToteschaSettings!.InstalledModpacks
		})
		{
			instance.InstallationProgress += OnInstallationProgressChanged;
			loaded = await instance.TryLaunchMinecraft();
		}

		MarkModpackAsInstalled();
		DeleteOldModpacks();
		SetLoadingStateForUI(true, updateStatusBar: false);
		OnInstallationProgressChanged(this, new ToteschaMinecraftLauncher.Scripts.Contracts.OldInstallationEventArgs(0, "Complete!"));
		if (loaded && ToteschaSettings.CloseLaucherAfterDownload)
			thisWindow.Quit();
	}

	public void DisableButtons() => SetLoadingStateForUI(false, updateStatusBar: false);
	public void EnableButtons() => SetLoadingStateForUI(true);

	private void SetLoadingStateForUI(bool enabled, string statusText = "", bool updateStatusBar = true)
	{
		var loadingBar = GetNode<ProgressBar>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar");
		if (!enabled && updateStatusBar)
		{
			SelectedModpack = null;
			_ = loadingBar.StartInfiniteLoading();
		}
		else if (updateStatusBar)
			loadingBar.StopInfiniteLoading();

		var disabled = !enabled;

		GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/SettingsButton").Disabled = disabled;
		GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/DetailsButton").Disabled = disabled || SelectedModpack == null;
		GetNode<TextureButton>("/root/LauncherWindow/FooterContainer/LaunchButtonContainer/LaunchButton").Disabled = disabled || SelectedModpack == null || IsLoggingIn;
		if (updateStatusBar)
			GetNode<Label>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressLabel").Text = statusText;
		GetNode<Button>("FooterContainer/LoginMargin/LoginButton").Disabled = disabled || IsLoggingIn;
	}
	private async Task GetLatestNews()
	{
		var helper = GetNode<WebHelper>("WebHelper");
		var image = await helper.GetImageDataAsync(ServerDetails?.NewsImageUrl ?? string.Empty);

		LatestNewsPicture = image.Data;
		LatestNews = ServerDetails?.News ?? string.Empty;
	}
	private void LoadSettings()
	{
		if (string.IsNullOrWhiteSpace(settingsDirectory))
			return;

		var fileHelper = new FileHelper();
		var filename = Path.Combine(settingsDirectory, "settings.json");

		if (!Directory.Exists(settingsDirectory))
			Directory.CreateDirectory(settingsDirectory);
		else if (File.Exists(filename) && fileHelper.TryReadAppTextFile(filename, out var contents))
		{
			try
			{
				ToteschaSettings = JsonConvert.DeserializeObject<OldToteschaSettings>(contents);
			}
			catch
			{ /*Do nothing = Null handle will take place on next line*/}
		}

		ToteschaSettings ??= new OldToteschaSettings()
		{
			ForceDownload = false,
			CloseLaucherAfterDownload = true,
			InstalledModpacks = new System.Collections.Generic.List<OldModpack>(),
			DownloadOnlyServerFiles = false,
			CleanUpOldPacks = true,
			MinecraftInstallationPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "ToteschaMinecraft"),
			ServerURL = "https://modpack.totescha.com",
			MemoryToAllocate = 0,
			MaxMemory = 0,
		};
		GetNode<Label>("/root/LauncherWindow/FooterContainer/LaunchButtonContainer/LaunchLabel").Text = (ToteschaSettings!.DownloadOnlyServerFiles) ? "Download" : "Launch";
		GetSystemMemoryDetails();    
	}
	private void GetSystemMemoryDetails()
	{
		var memoryHelperClient = new SystemHelper();

		var backgroundSafeMemory = memoryHelperClient.AvailableMemory() / 1048576.0;
		var reccomended = .6 * backgroundSafeMemory;

		double i = 0, j = 0;
		while (i < reccomended && i < 10240)
			i += 512;

		while (j + 512 <= backgroundSafeMemory)
			j += 512;
		ToteschaSettings!.MaxMemory = j;

		if (ToteschaSettings.MemoryToAllocate != 0 && ToteschaSettings.MemoryToAllocate > j)
			ToteschaSettings.MemoryToAllocate = i;

	}
	private void OnInstallationProgressChanged(object? sender, ToteschaMinecraftLauncher.Scripts.Contracts.OldInstallationEventArgs e)
	{
		var loadingBar = GetNode<ProgressBar>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar");
		if (e.InstallationPercentage >= 0)
		{
			loadingBar.StopInfiniteLoading();
			loadingBar.Value = e.InstallationPercentage * 100;
		}
		else
			loadingBar.StartInfiniteLoading();
		GetNode<Label>("FooterContainer/MarginContainer/ProgressBarContainer/ProgressLabel").Text = e.InstallationStatus.ToString();
	}
	private void MarkModpackAsInstalled()
	{
		if (SelectedModpack == null)
			return;

		if (ToteschaSettings!.InstalledModpacks == null)
			ToteschaSettings.InstalledModpacks = new System.Collections.Generic.List<OldModpack>();
		ToteschaSettings.InstalledModpacks.RemoveAll(x => x.ID == SelectedModpack.ID);
		ToteschaSettings.InstalledModpacks.Add(SelectedModpack);
		SaveSettings();
	}
	private void DeleteOldModpacks()
	{
		if (!(ToteschaSettings?.CleanUpOldPacks ?? true) ||
			ServerDetails?.Modpacks == null ||
			string.IsNullOrWhiteSpace(ToteschaSettings?.MinecraftInstallationPath))
			return;

		var names = ServerDetails?.Modpacks?.Select(x => x.Name).ToList();
		var modpacks = Directory.GetDirectories(ToteschaSettings!.MinecraftInstallationPath).Where(path => !path.Contains(Path.Combine(ToteschaSettings!.MinecraftInstallationPath, "Server"))).ToList();
		var serverModpacks = Directory.GetDirectories(Path.Combine(ToteschaSettings!.MinecraftInstallationPath, "Server")).ToList();

		modpacks.AddRange(serverModpacks);

		if (!modpacks.Any() || names == null || !names.Any())
			return;

		foreach (var modpack in modpacks)
		{
			var folder = modpack.Split(Path.DirectorySeparatorChar).Last();
			if (string.IsNullOrWhiteSpace(folder))
				continue;

			if (!names.Contains(folder))
				Directory.Delete(modpack, true);
		}
	}
}
