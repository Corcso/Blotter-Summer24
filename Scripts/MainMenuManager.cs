using Godot;
using System;
using System.Transactions;

public partial class MainMenuManager : Node
{
	MultiplayerManager multiplayerManager;
	[Export] Control joinMenu;
    [Export] Control disconnectBox;

	int currentPenColourChoice = 0;
	int currentCardColorChoice = 0;

    [Export] Control penColorButton; 
    [Export] Control cardColorButton; 

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        // Set multiplayer manager reference
		multiplayerManager = GetNode<MultiplayerManager>("../Multiplayer Manager");

        // Set default pen and card colours
        Color penColor = new Color(0, 0.76f, 0);
        currentPenColourChoice = 1;
        penColorButton.GetNode<Node2D>("./Background").Modulate = penColor;
        penColorButton.GetNode<Node2D>("./Foreground").Modulate = penColor;
        multiplayerManager.playerPenColor = penColor;

        currentCardColorChoice = 5;
        Color cardColor = new Color(0.22f, 0.36f, 0.75f);
        cardColorButton.GetNode<Node2D>("./Background").Modulate = cardColor;
        multiplayerManager.playerCardColor = cardColor;
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

	public void _on_change_pen_color_button_pressed() {
		currentPenColourChoice = (currentPenColourChoice + 1) % 6;
		Color penColor = new Color();
        switch (currentPenColourChoice)
        {
            case 0: penColor = new Color(1, 0, 0); break; // Red
            case 1: penColor = new Color(0, 0.76f, 0); break; // Green
            case 2: penColor = new Color(0, 0.7f, 0.7f); break; // Cyan/Teal
            case 3: penColor = new Color(0.85f, 0.7f, 0); break; // Yellow
            case 4: penColor = new Color(0.66f, 0, 0.85f); break; // Purple
            case 5: penColor = new Color(0.22f, 0.36f, 0.75f); break; // Blue
        }
        penColorButton.GetNode<Node2D>("./Background").Modulate = penColor;
        penColorButton.GetNode<Node2D>("./Foreground").Modulate = penColor;
        multiplayerManager.playerPenColor = penColor;
    }

	public void _on_change_card_color_button_pressed() {
        currentCardColorChoice = (currentCardColorChoice + 1) % 6;
        Color cardColor = new Color();
        switch (currentCardColorChoice)
        {
            case 0: cardColor = new Color(1, 0, 0); break; // Red
            case 1: cardColor = new Color(0, 0.76f, 0); break; // Green
            case 2: cardColor = new Color(0, 0.7f, 0.7f); break; // Cyan/Teal
            case 3: cardColor = new Color(0.85f, 0.7f, 0); break; // Yellow
            case 4: cardColor = new Color(0.66f, 0, 0.85f); break; // Purple
            case 5: cardColor = new Color(0.22f, 0.36f, 0.75f); break; // Blue
        }
        cardColorButton.GetNode<Node2D>("./Background").Modulate = cardColor;
        multiplayerManager.playerCardColor = cardColor;
    }

    public void _on_close_join_box_pressed() {
        joinMenu.Visible = false;
    }
}
