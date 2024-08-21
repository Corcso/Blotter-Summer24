using Godot;
using System;

public partial class MainMenuManager : Node
{

	[Export] Control joinMenu;
    [Export] Control disconnectBox;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_open_join_menu_pressed()
	{ 
		joinMenu.Visible = true;
	}

	public void _on_close_disconnect_box_pressed()
	{ disconnectBox.Visible = false; }
}
