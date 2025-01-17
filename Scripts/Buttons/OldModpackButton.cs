using Godot;
using System;
using System.Linq.Expressions;

public partial class OldModpackButton : TextureButton
{
	public ModpackInstalledState InstalledState { get; set; }
	[Signal]
	public delegate void OnModpackButtonClickedEventHandler(string modpackName);

	private bool isToggledOn = false;
	private bool isFocused = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetIconForInstalledState();
		this.Toggled += OnButtonToggled;
		this.MouseEntered += OnMouseEntered;
		this.MouseExited += OnMouseExited;
	}

	public void ToggleButton(bool toggle)
	{
		ButtonPressed = toggle;
		OnButtonToggled(toggle);
	}
	private void OnMouseExited()
	{
		isFocused = false;
		if (isToggledOn)
			return;
		SetThemeOverride(false);
	}

	private void OnMouseEntered()
	{
		isFocused = true;
		SetThemeOverride(true);
	}

	private void OnButtonToggled(bool toggledOn)
	{
		if (toggledOn)
		{
			var title = GetNode<Label>("ButtonHContainer/ButtonVContainer/ModpackNameLabel");
			EmitSignal(SignalName.OnModpackButtonClicked, title.Text);
		}

		isToggledOn = toggledOn;
		if (!isFocused)
			SetThemeOverride(toggledOn);

	}

	private void SetThemeOverride(bool backgroundIsGreen)
	{
		var title = GetNode<Label>("ButtonHContainer/ButtonVContainer/ModpackNameLabel");
		var subtitle = GetNode<Label>("ButtonHContainer/ButtonVContainer/ModpackVersionLabel");
		SetIconForInstalledState();

		if (backgroundIsGreen)
		{
			title.RemoveThemeColorOverride("font_color");
			title.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
			subtitle.RemoveThemeColorOverride("font_color");
			subtitle.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
		}
		else
		{
			title.RemoveThemeColorOverride("font_color");
			title.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));
			subtitle.RemoveThemeColorOverride("font_color");
			subtitle.AddThemeColorOverride("font_color", new Color(0, 0, 0, 1));
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public void SetText(string titleText, string subtitleText)
	{
		var title = GetNode<Label>("ButtonHContainer/ButtonVContainer/ModpackNameLabel");
		var subtitle = GetNode<Label>("ButtonHContainer/ButtonVContainer/ModpackVersionLabel");

		title.Text = titleText;
		subtitle.Text = subtitleText;
	}

	public void SetIconForInstalledState()
	{
		var darkIcon = !isFocused && !isToggledOn;
		var title = GetNode<Label>("ButtonHContainer/ButtonVContainer/ModpackNameLabel");

		var iconBasePath = (darkIcon) ? "res://Icons/Dark/" : "res://Icons/Light/";
		string imagePath = null;
		var texture = GetNode<TextureRect>("ButtonHContainer/IconMargin/ButtonIcon");
		texture.Texture = null;
		switch (InstalledState)
		{
			case ModpackInstalledState.NeedsUpdate:
				imagePath = iconBasePath + "update.png";
				break;
			case ModpackInstalledState.UpToDate:
				imagePath = iconBasePath + "check.png";
				break;
			default:
				imagePath = iconBasePath + "download.png";
				break;
		}
		if (imagePath != null)
			texture.Texture = GD.Load<CompressedTexture2D>(imagePath);
	}
}

public enum OldModpackInstalledState
{
	Download,
	NeedsUpdate,
	UpToDate
}
