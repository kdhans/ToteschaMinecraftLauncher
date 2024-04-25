using Godot;
using System;
using System.IO;
using ToteschaMinecraftLauncher;

public partial class Settings : Control
{
	private ToteschaSettings toteschaSettings;
	private HSlider ramSlider;
	private LineEdit directoryField;
	private LineEdit serverField;
	private BaseButton downloadServerFiles, clearUnusedPacks, forceDownload, downloadSettings;
	private LauncherWindow launcherWindow;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		downloadServerFiles = GetNode<BaseButton>("VBoxContainer/AdditionalSettingsBox/HFlowContainer/DownloadServerOnlySettingButton");
		clearUnusedPacks = GetNode<BaseButton>("VBoxContainer/AdditionalSettingsBox/HFlowContainer/DeleteExpiredModsSettingButton");
		forceDownload = GetNode<BaseButton>("VBoxContainer/AdditionalSettingsBox/HFlowContainer/RedownloadSettingsButton");
		downloadSettings = GetNode<BaseButton>("VBoxContainer/AdditionalSettingsBox/HFlowContainer/DownloadPreferencesSettingsButton");
		ramSlider = GetNode<HSlider>("VBoxContainer/RamBox/HBoxContainer/RamSlider");
		
		launcherWindow = GetNode<LauncherWindow>("/root/LauncherWindow");
		
		directoryField = GetNode<LineEdit>("VBoxContainer/DirectoryBoxes/FileLocationBox/DirectoryContainer/DirectoryTextField");
		serverField = GetNode<LineEdit>("VBoxContainer/DirectoryBoxes/ServerBox/ServerTextField");
		GetSettingsValues();
		directoryField.TextChanged += OnFilePathChanged;
		directoryField.FocusExited += SetSettingsValues;
		serverField.FocusExited += SetSettingsValues;
		serverField.TextChanged += OnServerLocationChanged;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _ExitTree()
	{
		SetSettingsValues();
		base._ExitTree();
	}

	private void GetSettingsValues()
	{
		toteschaSettings = launcherWindow.ToteschaSettings;

		directoryField.Text = toteschaSettings.MinecraftInstallationPath;
		serverField.Text = toteschaSettings.ServerURL;
		downloadSettings.ButtonPressed = toteschaSettings.DownloadSettingsFile;
		forceDownload.ButtonPressed = toteschaSettings.ForceDownload;
		downloadServerFiles.ButtonPressed = toteschaSettings.DownloadServerFiles;
		clearUnusedPacks.ButtonPressed = toteschaSettings.CleanUpOldPacks;
	}

	private void SetSettingsValues()
	{
		toteschaSettings.MinecraftInstallationPath = directoryField.Text;
		toteschaSettings.ServerURL = serverField.Text;
		toteschaSettings.DownloadSettingsFile = downloadSettings.ButtonPressed;
		toteschaSettings.ForceDownload = forceDownload.ButtonPressed;
		toteschaSettings.DownloadServerFiles = downloadServerFiles.ButtonPressed;
		toteschaSettings.CleanUpOldPacks = clearUnusedPacks.ButtonPressed;

		launcherWindow.ToteschaSettings = toteschaSettings;
	}

	private void OnFilePathChanged(string newText)
	{
		if (string.IsNullOrEmpty(newText))
			return;
		toteschaSettings.MinecraftInstallationPath = newText;
	}
	private void OnServerLocationChanged(string newText)
	{
		if (string.IsNullOrEmpty(newText))
			return;
		toteschaSettings.ServerURL = newText;
	}
}
