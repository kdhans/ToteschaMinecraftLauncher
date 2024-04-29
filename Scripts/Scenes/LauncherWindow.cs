using Godot;
using Newtonsoft.Json;
using SevenZip.Compression.LZ;
using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher;
using ToteschaMinecraftLauncher.Scripts.Helpers;
using ToteschaMinecraftLauncher.Scripts.Logic;
using ToteschaMinecraftLauncher.Scripts.UIHelpers;
using static ToteschaMinecraftLauncher.Scripts.UIHelpers.LoginHelper;

#nullable enable
public partial class LauncherWindow : Control
{
	public ToteschaSettings? ToteschaSettings { get; set; }
	public ServerDetails? ServerDetails { get; set; }
	public Modpack? SelectedModpack { get; set; }
	public MinecraftSession? Session { get; set; }
	public string? LatestNews { get; set; }
	public ImageTexture? LatestNewsPicture { get; set; }
	public bool IsLoadingServerData { get; set; } = true;
	public static Node? SelectedNode { get; set; }
	public static string DisplayBoxNodePath = "/root/LauncherWindow/DisplayAreaContainer/MainMargin";

	private static string? sceneName = null;
	private const float PercentOfDisplaySafeArea = 0.55f;
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
		await TryLoginToMinecraft(ToteschaSettings?.Username, ToteschaSettings?.Password);
	}

	private void OnCloseRequested()
	{
		SaveSettings();
	}

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
		string statusText = null;
		bool enableModpackRequiredButtons = true;

		if (result.Error != null)
		{
			statusText = result.Error;
			enableModpackRequiredButtons = false;
		}
		else
		{
			ServerDetails = result.Data;
			statusText = "Select a modpack and press Launch to begin!";
			await GetLatestNews();
		}
		SetLoadingStateForUI(true, statusText, enableModpackRequiredButtons);
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
				throw new ApplicationException("File did not write") ;
		}
		catch (Exception ex) 
		{
			GD.Print($"Could not save {filename} - {ex.Message}");
		}
	}
	public void ResizeAppWindow()
	{
		var window = GetWindow();
		window.ContentScaleSize = new Vector2I(MinimumWidth, MinimumHeight);
		
		
		var displayDPI = DisplayServer.ScreenGetDpi();
		var display = DisplayServer.GetDisplaySafeArea();
		var developerDPI = 120;		
		var displaySizeOnStandardScreen = ((float)MinimumWidth/(float)developerDPI);


		if (displayDPI > developerDPI)
		{
			var displaySizeOnPlayerScreen = ((float)MinimumWidth/(float)displayDPI);
			var ratio = (1f - displaySizeOnPlayerScreen/displaySizeOnStandardScreen) + 1f;

			var length = MinimumWidth * ratio;
			var width = MinimumHeight * ratio;

			//Resize the window
			window.Size = new Vector2I((int)length, (int)width);

			//Calculate the new position of the window. Center the window on the screen.
			//The position is calculated by subtracting the window size from the display size and dividing by 2.
			window.Position = new Vector2I((int)(display.Size.X - length) / 2, (int)(display.Size.Y - width) / 2);
		}
	}
	private void SetLoadingStateForUI(bool enabled, string statusText = "", bool modpackRequiredButtons = true, bool triggeredFromLaunchButton = false)
	{
		var loadingBar = GetNode<ProgressBar>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar");
		if (!enabled && !triggeredFromLaunchButton)
		{
			SelectedModpack = null;
			_ = loadingBar.StartInfiniteLoading();
		}
		else if (!triggeredFromLaunchButton)
			loadingBar.StopInfiniteLoading();
		GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/SettingsButton").Disabled = !enabled;
		GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/DetailsButton").Disabled = !(enabled && modpackRequiredButtons);
		GetNode<TextureButton>("/root/LauncherWindow/FooterContainer/LaunchButtonContainer/LaunchButton").Disabled = !(enabled && modpackRequiredButtons);
		if (!triggeredFromLaunchButton)
			GetNode<Label>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressLabel").Text = statusText;
		GetNode<Button>("FooterContainer/LoginMargin/LoginButton").Disabled = !enabled;
	}
	private async Task GetLatestNews()
	{
		var helper = GetNode<WebHelper>("WebHelper");
		var image = await helper.GetImageDataAsync(ServerDetails.NewsImageUrl);

		LatestNewsPicture = image.Data;
		LatestNews = ServerDetails.News ?? string.Empty;
	}
	private void LoadSettings()
	{
		//TODO: Load settings file
		LoadSettingsFromFile();
		if (ToteschaSettings == null)
		{
			ToteschaSettings = new ToteschaSettings()
			{
				ForceDownload = false,
				DownloadSettingsFile = true,
				InstalledModpacks = new System.Collections.Generic.List<Modpack>(),
				DownloadServerFiles = false,
				CleanUpOldPacks = true,
				MinecraftInstallationPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "ToteschaMinecraft"),
				ServerURL = "https://minecraft.totescha.com",
				MemoryToAllocate = 0,
				MaxMemory = 0,
			};
		}

		GetSystemMemoryDetails();
	}
	private void LoadSettingsFromFile()
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
				ToteschaSettings = JsonConvert.DeserializeObject<ToteschaSettings>(contents);
			} catch 
			{
				GD.Print($"Could not read {filename}");
			}
		}
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
		ToteschaSettings.MaxMemory = j;

		if (ToteschaSettings.MemoryToAllocate != 0 && ToteschaSettings.MemoryToAllocate > j)
			ToteschaSettings.MemoryToAllocate = i;

	}
	public async Task<Tuple<bool, string>> TryLoginToMinecraft(string encyUsername, string encyPassword, bool isEncrypted = true)
	{
		var loginButton = GetNode<Button>("FooterContainer/LoginMargin/LoginButton");
		var launchButton = GetNode<TextureButton>("FooterContainer/LaunchButtonContainer/LaunchButton");
		Tuple<bool, string> success;
		var encryptor = new ToteschaEncryptor();
		try
		{
			loginButton.Disabled = true;
			launchButton.Disabled = true;
			var username = isEncrypted ? await encryptor.DecryptStringAsync(encyUsername) : encyUsername;
			var password = isEncrypted ? await encryptor.DecryptStringAsync(encyPassword) : encyPassword;
			Session = await GetMinecraftSession(username,password);
			success = new Tuple<bool, string>(true, string.Empty);
			loginButton.Disabled = false;
			launchButton.Disabled = false;
			loginButton.Text = $"      Welcome, {System.Environment.NewLine}      {Session.username}!";
		}
		catch (Exception ex)
		{
			loginButton.Disabled = false;
			launchButton.Disabled = false;
			success = new Tuple<bool, string>(false, ex.Message);
		}
		return success;
	}
	public async Task TryLaunchMinecraft()
	{
		bool loaded = false;
		var thisWindow = GetTree();
		SetLoadingStateForUI(false, triggeredFromLaunchButton: true);

		if (string.IsNullOrWhiteSpace(Session?.accessToken))
			GetNode<LoginButton>("FooterContainer/LoginMargin/LoginButton").ClickButtonAsync();
		var userFinishedUp = GetNode<LoginWindow>("LoginWindow").UserFinishedUp;
		while (string.IsNullOrWhiteSpace(Session?.accessToken) && !userFinishedUp)
		{
			await Task.Delay(100);
			userFinishedUp = GetNode<LoginWindow>("LoginWindow").UserFinishedUp;
		}
		using (var instance = new MinecraftInstanceLogic() {
			Session = Session,
			Modpack = SelectedModpack,
			Settings = ToteschaSettings
		})
		{
			instance.InstallationProgress += OnInstallationProgressChanged;
			loaded = await instance.TryLaunchMinecraft();
		}

		SetLoadingStateForUI(true, triggeredFromLaunchButton: true);
		OnInstallationProgressChanged(this, new ToteschaMinecraftLauncher.Scripts.Contracts.InstallationEventArgs(0, "Complete!"));
		if (loaded)
			thisWindow.Quit();		
	}

	private void OnInstallationProgressChanged(object? sender, ToteschaMinecraftLauncher.Scripts.Contracts.InstallationEventArgs e)
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
}
