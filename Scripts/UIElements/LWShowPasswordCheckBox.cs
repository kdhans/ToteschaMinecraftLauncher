using Godot;
using System;

public partial class LWShowPasswordCheckBox : CheckBox
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Toggled += OnCheckboxToggled;
	}

	private void OnCheckboxToggled(bool toggledOn)
	{
		var passwordBox = GetNode<LineEdit>("/root/LauncherWindow/LoginWindow/VBoxContainer/PasswordGroup/LWPasswordTextBox");
		if (toggledOn)
			passwordBox.Secret = false;
		else
			passwordBox.Secret = true;
		
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
