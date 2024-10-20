﻿using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher.UpdatedScripts.Contracts;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Scenes
{
    public partial class Settings : Control
    {
        private ToteschaSettings toteschaSettings;
        private HSlider ramSlider;
        private LineEdit directoryField;
        private LineEdit serverField;
        private BaseButton downloadServerOnlyFiles, deleteExpiredModpacks, forceRedownload, closeLauncherAfterDownload;
        private MainControl mainControl;
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            mainControl = GetNode<MainControl>("/root/MainControl");

            closeLauncherAfterDownload = GetNode<BaseButton>("VBoxContainer/AdditionalSettingsBox/HFlowContainer/CloseLauncherSettingsButton");
            deleteExpiredModpacks = GetNode<BaseButton>("VBoxContainer/AdditionalSettingsBox/HFlowContainer/DeleteExpiredModsSettingButton");
            forceRedownload = GetNode<BaseButton>("VBoxContainer/AdditionalSettingsBox/HFlowContainer/ForceRedownloadButton");
            downloadServerOnlyFiles = GetNode<BaseButton>("VBoxContainer/AdditionalSettingsBox/HFlowContainer/DownloadServerOnlySettingButton");
            ramSlider = GetNode<HSlider>("VBoxContainer/RamBox/HBoxContainer/RamSlider");
            directoryField = GetNode<LineEdit>("VBoxContainer/DirectoryBoxes/FileLocationBox/DirectoryContainer/DirectoryTextField");
            serverField = GetNode<LineEdit>("VBoxContainer/DirectoryBoxes/ServerBox/ServerTextField");

            //Perform after fields have been assigned, and before events have been assigned so we're not wasting time
            GetSettingsValues();

            directoryField.TextChanged += OnFilePathChanged;
            directoryField.FocusExited += SetSettingsValues;
            serverField.FocusExited += SetSettingsValues;
            serverField.TextChanged += OnServerLocationChanged;
            downloadServerOnlyFiles.Toggled += SetButtonSettings;
            deleteExpiredModpacks.Toggled += SetButtonSettings;
            forceRedownload.Toggled += SetButtonSettings;
            closeLauncherAfterDownload.Toggled += SetButtonSettings;
        }

        private void SetButtonSettings(bool toggledOn) => SetSettingsValues();

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
            toteschaSettings = mainControl.GetSettings();

            directoryField.Text = toteschaSettings.MinecraftInstallationPath;
            serverField.Text = toteschaSettings.ServerURL;

            closeLauncherAfterDownload.ButtonPressed = toteschaSettings.CloseLaucherAfterDownload;
            forceRedownload.ButtonPressed = toteschaSettings.ForceDownload;
            downloadServerOnlyFiles.ButtonPressed = toteschaSettings.DownloadOnlyServerFiles;
            deleteExpiredModpacks.ButtonPressed = toteschaSettings.CleanUpOldPacks;
        }

        private void SetSettingsValues()
        {
            toteschaSettings.MinecraftInstallationPath = directoryField.Text;
            toteschaSettings.ServerURL = serverField.Text;

            toteschaSettings.CloseLaucherAfterDownload = closeLauncherAfterDownload.ButtonPressed;
            toteschaSettings.ForceDownload = forceRedownload.ButtonPressed;
            toteschaSettings.DownloadOnlyServerFiles = downloadServerOnlyFiles.ButtonPressed;
            toteschaSettings.CleanUpOldPacks = deleteExpiredModpacks.ButtonPressed;

            mainControl.UpdateSettings(toteschaSettings);
            GetNode<Label>("/root/MainControl/LauncherWindow/FooterContainer/LaunchButtonContainer/LaunchLabel").Text = (downloadServerOnlyFiles.ButtonPressed) ? "Download" : "Launch";
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
}
