using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher.UpdatedScripts.Contracts;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Scenes
{
    public partial class Details : Control
    {
        private MainControl mainControl = null;
        private Label modpackName, modLoader, mcVersion, modpackVersion, numMods, lastUpdated;
        private PackedScene scene;
        private List<FileDetail> files = new List<FileDetail>();
        private LineEdit search;

        // Called when the node enters the scene tree for the first time.
        public override async void _Ready()
        {
            scene = GD.Load<PackedScene>("res://file_detail.tscn");
            mainControl = GetNode<MainControl>("/root/MainControl");
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
            Modpack? selectedModpack = mainControl.GetSelectedModpack();
            if (selectedModpack == null)
                return;

            modpackName.Text = selectedModpack.Name;
            modLoader.Text = selectedModpack.ModLoader;
            mcVersion.Text = selectedModpack.MineceaftVersion;
            modpackVersion.Text = selectedModpack.ModpackVersion;
            numMods.Text = selectedModpack.Files?.Count.ToString() ?? "0";
            lastUpdated.Text = selectedModpack.LastUpdated.ToShortDateString();
        }

        private async Task UpdateFileList()
        {
            //Create a "vanilla" mod file if files are null
            Modpack? selectedModpack = mainControl.GetSelectedModpack();
            if (selectedModpack == null)
                return;
            else if (selectedModpack.Files == null || selectedModpack.Files.Count == 0)
                await AddNewModDetailAsync("It's just Minecraft!",
                                 selectedModpack.MineceaftVersion,
                                 "It's vanilla! No mods! Wow, isn't that exciting?!",
                                 "",
                                 "https://www.minecraft.net");

            else
            {
                ClearModDetails();
                foreach (var file in selectedModpack.Files)
                    await AddNewModDetailAsync(file.Name, file.Version, file.Description, file.ImageURL, file.URL);
            }

        }

        private async Task AddNewModDetailAsync(string name, string version, string description, string imageUrl, string link)
        {
            if (!IsInstanceValid(this))
                return;

            var fileListNode = GetNode<VBoxContainer>("VBoxContainer/ModListScroll/ModListContainer");
            var fileDetail = (FileDetail)scene.Instantiate();
            var imageData = await mainControl.GetImageDataAsync(imageUrl);
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
}
