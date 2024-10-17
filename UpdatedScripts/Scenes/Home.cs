using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher.UpdatedScripts.Contracts;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Scenes
{
    public partial class Home : Control
    {
        private List<ModpackButton> modpackButtons = new List<ModpackButton>();
        private MainControl mainControl = null;
        private LauncherWindow launcherWindow = null;

        // Called when the node enters the scene tree for the first time.
        public override async void _Ready()
        {
            launcherWindow = GetNode<LauncherWindow>("/root/MainControl/LauncherWindow");
            mainControl = GetNode<MainControl>("/root/MainControl");
            var refreshButton = GetNode<ReloadButton>("VBoxContainer/ScrollContainer/ButtonContainer/reloadButton");
            refreshButton.OnRefreshPressed += TriggerReload;

            await GetServerDetailsAsync();
        }

        private async void TriggerReload() => await GetServerDetailsAsync(true);
        

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        private async Task GetServerDetailsAsync(bool forceReload = false)
        { 
           var results = await mainControl.GetServerDetailsAsync(forceReload);
            if (results.Item1 == null || (results.Item1.Modpacks?.Count ?? 0) <= 0 )
            {
                launcherWindow.UpdateStatusText(results.Item2);
                launcherWindow.DisableNodes(disableAllNodes: true, disableLoginNodes: false, disableModpackNodes: true);
                return;
            }

            FinishLoadingScreen();
            ReloadModpacks(results.Item1.Modpacks);
            var imageData = await mainControl.GetImageDataAsync(results.Item1.NewsImageUrl);
            UpdateNews(imageData.Data, results.Item1.News);
        }

        private void SetLoadingScreen(bool showNewsContainer = false)
        {
            ClearModpacks();
            GetNode<Label>("VBoxContainer/ModpackSelectLabel").Text = "Loading...";
            launcherWindow.UpdateStatusBar(-1);
            if (!showNewsContainer)
            {
                GetNode<Label>("VBoxContainer/NewsLabel").Text = string.Empty;
                GetNode<HBoxContainer>("VBoxContainer/ScrollContainer/ButtonContainer").Visible = false;
                GetNode<HBoxContainer>("VBoxContainer/HBoxContainer").Visible = false;
                GetNode<TextureRect>("Background").Visible = false;
            }
        }
        private void FinishLoadingScreen()
        {
            launcherWindow.UpdateStatusBar(0);
            launcherWindow.DisableNodes(disableAllNodes: false);
            GetNode<Label>("VBoxContainer/ModpackSelectLabel").Text = "SELECT YOUR MODPACK";
            GetNode<Label>("VBoxContainer/NewsLabel").Text = "LATEST NEWS";
            GetNode<HBoxContainer>("VBoxContainer/ScrollContainer/ButtonContainer").Visible = true;
            GetNode<HBoxContainer>("VBoxContainer/HBoxContainer").Visible = true;
            GetNode<TextureRect>("Background").Visible = true;
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
            mainControl.SetModpack(modpackName);
            launcherWindow.DisableNodes(disableAllNodes:false);
        }
        private ModpackInstalledState CheckIfModpackIsUpToDate(Modpack modpack)
        {
            var settings = mainControl.GetSettings();

            var installedModpacks = settings.InstalledModpacks;
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
        private void ReloadModpacks(List<Modpack>? modpacks)
        {
            var toteschaSettings = mainControl.GetSettings();
            if (toteschaSettings == null)
            {
                launcherWindow.UpdateStatusText("Settings could not be loaded. Please reload launcher or report the issue on the discord server.");
                launcherWindow.DisableNodes(disableAllNodes: true);
                return;
            }

            if (modpacks == null || modpacks.Count == 0)
                return;
            var node = GetNode<HBoxContainer>("VBoxContainer/ScrollContainer/ButtonContainer");
            var scene = GD.Load<PackedScene>("res://modpack_button.tscn");

            ClearModpacks();

            if (string.IsNullOrEmpty(toteschaSettings.LastSelectedModpack) ||
                !modpacks.Select(x => x.Name).Contains(toteschaSettings.LastSelectedModpack))
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
    }
}
