using Godot;
using System;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher;

public partial class FileDetail : Control
{
	private Label nameLabel, versionLabel, descriptionLabel;
	private LinkButton modUrl;
	private TextureRect image;
	public string Name { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SetTextAndImage(string name, string version, string description, string link, ToteschaHttpResponse<ImageTexture> imageData)
	{
		Name = name;

		nameLabel = GetNode<Label>("HBoxContainer/VBoxContainer/NameLabel");
		versionLabel = GetNode<Label>("HBoxContainer/VBoxContainer/VersionLabel");
		descriptionLabel = GetNode<Label>("HBoxContainer/VBoxContainer/DescriptionLabel");
		modUrl = GetNode<LinkButton>("HBoxContainer/VBoxContainer/LinkButton");
		image = GetNode<TextureRect>("HBoxContainer/MarginContainer/TextureRect");
		nameLabel.Text = name;
		versionLabel.Text = version;	
		descriptionLabel.Text = description;
		modUrl.Text = link;
		modUrl.Uri = link;

		if (string.IsNullOrEmpty(imageData.Error))
			image.Texture = imageData.Data;
	}
}
