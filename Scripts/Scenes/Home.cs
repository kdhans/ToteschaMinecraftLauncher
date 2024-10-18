using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ToteschaMinecraftLauncher;
using System.Linq;

#nullable enable
public partial class Home : Control
{
	private List<ModpackButton> modpackButtons = new List<ModpackButton>();
	private LauncherWindow launcherWindow = null;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		launcherWindow = GetNode<LauncherWindow>("/root/LauncherWindow");
		var refreshButton = GetNode<ReloadButton>("VBoxContainer/ScrollContainer/ButtonContainer/reloadButton");
		refreshButton.OnRefreshPressed += TriggerReload;
		await WaitForData();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private async Task WaitForData()
	{
		if (launcherWindow.IsLoadingServerData)
			SetToLoadingScreen();
		while (launcherWindow.IsLoadingServerData)
			await Task.Delay(10);
		FinishLoadingScreen();
		ReloadModpacks(launcherWindow.ServerDetails?.Modpacks, launcherWindow.ToteschaSettings);
		UpdateNews(launcherWindow.LatestNewsPicture, launcherWindow.LatestNews);
	}

	private async void TriggerReload()
	{
		SetToLoadingScreen(true);
		await launcherWindow.LoadServerDetailsAsync();
		FinishLoadingScreen();
		ReloadModpacks(launcherWindow.ServerDetails?.Modpacks, launcherWindow.ToteschaSettings);
		UpdateNews(launcherWindow.LatestNewsPicture, launcherWindow.LatestNews);
	}
	private void SetToLoadingScreen(bool alreadyLoaded = false)
	{
		ClearModpacks();
		GetNode<Label>("VBoxContainer/ModpackSelectLabel").Text = "Loading...";
		if (!alreadyLoaded)
		{
			GetNode<Label>("VBoxContainer/NewsLabel").Text = string.Empty;
			GetNode<HBoxContainer>("VBoxContainer/ScrollContainer/ButtonContainer").Visible = false;
			GetNode<HBoxContainer>("VBoxContainer/HBoxContainer").Visible = false;
			GetNode<TextureRect>("Background").Visible = false;
		}
	}
	private void FinishLoadingScreen()
	{
		GetNode<Label>("VBoxContainer/ModpackSelectLabel").Text = "SELECT YOUR MODPACK";
		GetNode<Label>("VBoxContainer/NewsLabel").Text = "LATEST NEWS";
		GetNode<HBoxContainer>("VBoxContainer/ScrollContainer/ButtonContainer").Visible = true;
		GetNode<HBoxContainer>("VBoxContainer/HBoxContainer").Visible = true;
		GetNode<TextureRect>("Background").Visible = true;
	}

	private void ReloadModpacks(List<OldModpack>? modpacks, OldToteschaSettings toteschaSettings)
	{
		if (toteschaSettings == null)
			toteschaSettings = new OldToteschaSettings();

		if (modpacks == null || modpacks.Count == 0)
			return;
		var node = GetNode<HBoxContainer>("VBoxContainer/ScrollContainer/ButtonContainer");
		var scene = GD.Load<PackedScene>("res://modpack_button.tscn");

		ClearModpacks();
		
		if (string.IsNullOrEmpty(toteschaSettings.LastSelectedModpack) || 
			!modpacks.Select(x=> x.Name).Contains(toteschaSettings.LastSelectedModpack))
			toteschaSettings.LastSelectedModpack = modpacks[0].Name;

		foreach (var modpack in modpacks)
		{
			var installedState = CheckIfModpackIsUpToDate(modpack);
			ModpackButton modpackButton = (ModpackButton)scene.Instantiate();
			modpackButton.SetText(modpack.Name, $"{modpack.ModLoader}-{modpack.MineceaftVersion}");
			modpackButton.InstalledState = installedState;

			if (modpack.Name == toteschaSettings.LastSelectedModpack)
			{
				modpackButton.ToggleButton(true);
				SelectModpack(modpack.Name);
			}

			modpackButton.OnModpackButtonClicked += SelectModpack;
			modpackButtons.Add(modpackButton);
			node.AddChild(modpackButton);
		}
	}

	private void ClearModpacks()
	{
		var node = GetNode<HBoxContainer>("VBoxContainer/ScrollContainer/ButtonContainer");
		if (modpackButtons.Count > 0)
		{
			foreach (var button in modpackButtons)
			{
				button.OnModpackButtonClicked -= SelectModpack;
				node.RemoveChild(button);
			}
			modpackButtons.Clear();
		}
	}

	private void SelectModpack(string modpackName)
	{
		launcherWindow.SelectedModpack = launcherWindow.ServerDetails.Modpacks.Where(x=> x.Name == modpackName).First();
		launcherWindow.EnableButtons();
	}

	private void UpdateNews(ImageTexture? image, string? newsText)
	{
		var textureRect = GetNode<TextureRect>("VBoxContainer/HBoxContainer/NewsImage");
		var richTextBox = GetNode<RichTextLabel>("VBoxContainer/HBoxContainer/RichTextLabel");
		if (image != null)
			textureRect.Texture = image;
		if (richTextBox != null)
			richTextBox.Text = newsText;
	}

	private ModpackInstalledState CheckIfModpackIsUpToDate(OldModpack modpack)
	{
		var installedModpacks = launcherWindow.ToteschaSettings.InstalledModpacks;		
		if (installedModpacks == null || installedModpacks.Count == 0)
			return ModpackInstalledState.Download;
		
		var installedModpackNames = installedModpacks.Select(x => x.Name).ToList();
		if (!installedModpackNames.Contains(modpack.Name))
			return ModpackInstalledState.Download;

		var selectedModpack = installedModpacks.First(x => x.Name == modpack.Name);
		if (selectedModpack.ModpackVersion != modpack.ModpackVersion)
			return ModpackInstalledState.NeedsUpdate;

		return ModpackInstalledState.UpToDate;
	}
}
