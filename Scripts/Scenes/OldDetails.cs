using Godot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher;

public partial class OldDetails : Control
{
	private OldLauncherWindow launcherWindow = null;
	private Label modpackName, modLoader, mcVersion, modpackVersion, numMods, lastUpdated;
	private PackedScene scene;
	private List<FileDetail> files = new List<FileDetail>();
	private LineEdit search;

	// Called when the node enters the scene tree for the first time.
	public override async void _Ready()
	{
		scene = GD.Load<PackedScene>("res://file_detail.tscn");
		launcherWindow = GetNode<OldLauncherWindow>("/root/LauncherWindow");
		modpackName = GetNode<Label>("VBoxContainer/DetailsMarginContainer/DetailsFlowContainer/ModName/Value");
		modLoader = GetNode<Label>("VBoxContainer/DetailsMarginContainer/DetailsFlowContainer/ModLoader/Value");
		mcVersion = GetNode<Label>("VBoxContainer/DetailsMarginContainer/DetailsFlowContainer/MinecraftVersion/Value");
		modpackVersion = GetNode<Label>("VBoxContainer/DetailsMarginContainer/DetailsFlowContainer/ModpackVersion/Value");
		numMods = GetNode<Label>("VBoxContainer/DetailsMarginContainer/DetailsFlowContainer/ModCount/Value");
		lastUpdated = GetNode<Label>("VBoxContainer/DetailsMarginContainer/DetailsFlowContainer/LastUpdated/Value");
		search = GetNode<LineEdit>("VBoxContainer/FileSearch");
		search.TextChanged += OnSearchTextChanged;
		
		FillDetailsOfModpack();
		await UpdateFileList();
	}

	private void OnSearchTextChanged(string newText)
	{
		if (files.Count == 0)
			return;

		if (string.IsNullOrEmpty(newText))
			foreach (var file in files)
				file.Visible = true;
		else
			foreach (var file in files)
				if (file.Name.ToLower().Contains(newText.ToLower()))
					file.Visible = true;
				else
					file.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void ClearDetails()
	{
		modpackName.Text = "Select a modpack first!";
		modLoader.Text = "";
		mcVersion.Text = "";
		modpackVersion.Text = "";
		numMods.Text = "";
		lastUpdated.Text = "";
	}
	private void FillDetailsOfModpack()
	{
		ClearDetails();
		if (launcherWindow.SelectedModpack == null)
			return;
		
		modpackName.Text = launcherWindow.SelectedModpack.Name;
		modLoader.Text = launcherWindow.SelectedModpack.ModLoader;
		mcVersion.Text = launcherWindow.SelectedModpack.MineceaftVersion;
		modpackVersion.Text = launcherWindow.SelectedModpack.ModpackVersion;
		numMods.Text = launcherWindow.SelectedModpack.Files?.Count.ToString() ?? "0";
		lastUpdated.Text = launcherWindow.SelectedModpack.LastUpdated.ToShortDateString();
	}

	private async Task UpdateFileList()
	{
		//Create a "vanilla" mod file if files are null
		if (launcherWindow.SelectedModpack == null)
			return;
		else if (launcherWindow.SelectedModpack.Files == null || launcherWindow.SelectedModpack.Files.Count == 0)
			await AddNewModDetailAsync("It's just Minecraft!",
							 launcherWindow.SelectedModpack.MineceaftVersion,
							 "It's vanilla! No mods! Wow, isn't that exciting?!",
							 "",
							 "https://www.minecraft.net");

		else
		{
			ClearModDetails();
			foreach (var file in launcherWindow.SelectedModpack.Files)
				await AddNewModDetailAsync(file.Name, file.Version, file.Description, file.ImageURL, file.URL);			
		}

	}

	private async Task AddNewModDetailAsync(string name, string version, string description, string imageUrl, string link)
	{
		if (!IsInstanceValid(this))
			return;
			
		var fileListNode = GetNode<VBoxContainer>("VBoxContainer/ModListScroll/ModListContainer");
		var webHelper = GetNode<WebHelper>("/root/LauncherWindow/WebHelper");
		var fileDetail = (FileDetail)scene.Instantiate();
		var imageData = await webHelper.GetImageDataAsync(imageUrl);
		fileDetail.SetTextAndImage(name, version, description, link, imageData);
		fileDetail.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
		fileDetail.SizeFlagsVertical = SizeFlags.ShrinkCenter;
		if (IsInstanceValid(fileListNode))
		{
			files.Add(fileDetail);
			fileListNode.AddChild(fileDetail);
		}
	}

	private void ClearModDetails()
	{
		var node = GetNode<VBoxContainer>("VBoxContainer/ModListScroll/ModListContainer");
		if (files.Count > 0)
		{
			foreach (var modDetail in files)
				node.RemoveChild(modDetail);
			
			files.Clear();
		}
	}

}
