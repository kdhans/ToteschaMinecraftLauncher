using Godot;
using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher;
using ToteschaMinecraftLauncher.Scripts.Helpers;

#nullable enable
public partial class LauncherWindow : Control
{
	public ToteschaSettings ToteschaSettings { get; set; }
	public ServerDetails ServerDetails { get; set; }
	public Modpack SelectedModpack { get; set; }
	public string LatestNews { get; set; }
	public ImageTexture LatestNewsPicture { get; set; }
	public bool IsLoadingServerData { get; set; } = true;
	public static Node? SelectedNode { get; set; }
	public static string DisplayBoxNodePath = "/root/LauncherWindow/DisplayAreaContainer/MainMargin";

	private static string? sceneName = null;
	private const float PercentOfDisplaySafeArea = 0.65f;
	private const int MinimumWidth = 1400;
	private const int MinimumHeight = 750;
	private string settingsDirectory;
	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		settingsDirectory = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "ToteschaMinecraft");
		LoadSettings();
		ResizeAppWindow();
		await LoadServerDetailsAsync();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetupButtonScene(PackedScene scene, Node parentNode)
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
		var result = await webHelper.CallJsonGetRequestAsync<ServerDetails>("https://script.google.com/macros/s/AKfycbzg9gGz0lTj0hh_jr5ySLwoz-NUNQcJAg9aRqBf-L5_tR0KWOBP0CpZ-XYcPF56Sc1z/exec");
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
	
	
	private void ResizeAppWindow()
	{
		var window = GetWindow();
		var display = DisplayServer.GetDisplaySafeArea();

		//If the monitor resolution is more than double the size, set it PercentOfDisplaySafeArea size
		if (display.Size.X / 2 > MinimumWidth)
		{
			var ratio = (display.Size.X * PercentOfDisplaySafeArea) / MinimumWidth;
			var length = MinimumWidth * ratio;
			var width = MinimumHeight * ratio;

			//Resize the window
			window.Size = new Vector2I((int)length, (int)width);

			//Calculate the new position of the window. Center the window on the screen.
			//The position is calculated by subtracting the window size from the display size and dividing by 2.
			window.Position = new Vector2I((int)(display.Size.X - length) / 2, (int)(display.Size.Y - width) / 2);
		}
	}
	private async Task GetLatestNews()
	{
		var helper = GetNode<WebHelper>("WebHelper");
		var image = await helper.GetImageDataAsync(ServerDetails.NewsImageUrl);

		LatestNewsPicture = image.Data;
		LatestNews = ServerDetails.News ?? string.Empty;
	}
	private void SetLoadingStateForUI(bool enabled, string statusText = "", bool modpackRequiredButtons = true)
	{
		var loadingBar = GetNode<ProgressBar>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressBar");
		if (!enabled)
		{
			SelectedModpack = null;
			loadingBar.StartInfiniteLoading();
		}
		else
			loadingBar.StopInfiniteLoading();
		GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/SettingsButton").Disabled = !enabled;
		GetNode<Button>("/root/LauncherWindow/DisplayAreaContainer/MenuMargin/MenuContainer/DetailsButton").Disabled = !(enabled && modpackRequiredButtons);
		GetNode<TextureButton>("/root/LauncherWindow/FooterContainer/LaunchButtonContainer/LaunchButton").Disabled = !(enabled && modpackRequiredButtons);
		GetNode<Label>("/root/LauncherWindow/FooterContainer/MarginContainer/ProgressBarContainer/ProgressLabel").Text = statusText;

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
				ServerURL = "https://minecraft.totescha.com"
			};
			GetSystemMemoryDetails();
		}
	}
	private void LoadSettingsFromFile()
	{
		if (!Directory.Exists(settingsDirectory))
			Directory.CreateDirectory(settingsDirectory);
		else if (File.Exists(Path.Combine(settingsDirectory, "settings.json")))
		{

		}
	}
	public void SaveSettings()
	{
		//Save settings to json file
	}

	private void GetSystemMemoryDetails()
	{
		var memoryHelperClient = new MemoryHelper();

		var backgroundSafeMemory = memoryHelperClient.AvailableMemory() / 1048576.0;
		var reccomended = .6 * backgroundSafeMemory;

		double i = 0, j = 0;
		while (i < reccomended && i < 10240)
			i += 512;
		ToteschaSettings.MemoryToAllocate = i;
		while (j + 512 <= backgroundSafeMemory)
			j += 512;
		ToteschaSettings.MaxMemory = j;


	}
}
