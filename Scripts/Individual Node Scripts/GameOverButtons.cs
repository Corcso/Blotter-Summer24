using Godot;
using System;

public partial class GameOverButtons : Node2D
{
	BingoManager bingoManager;

	bool exitPressed;
	float timeSincePressedExit;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		bingoManager = GetNode<BingoManager>("../BingoManager");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (exitPressed) {
			timeSincePressedExit += (float)delta;
			if (timeSincePressedExit > 1) {
				GetParent().Name = "Deleting Now";
                GetParent().QueueFree();
                GetTree().Root.GetNode<Node>("./Game Scene2").Name = "Game Scene";
            }
		}
	}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void StartNewGame() {
		

        GetParent<Node2D>().Hide();
       

        // If we are playing again Delete this scene and recreate the game scene
        Node2D scene = ResourceLoader.Load<PackedScene>("res://Scenes/Game Scenes/Main Play Scene.tscn").Instantiate<Node2D>();
        GetTree().Root.AddChild(scene);
        scene.Name = "Game Scene";

		// Rerun ready on bingo manager, this resets evetything for a new game
		//bingoManager._Ready();

		exitPressed = true;
    }

	public void _on_play_again_button_pressed() {
		Rpc("StartNewGame");
	}

	public void _on_exit_game_button_pressed() {
		// If we exit, just disconnect a player. The disconnect signal will handle the rest. 
		Multiplayer.MultiplayerPeer.DisconnectPeer((int)GameManager.playerIds[1]);
	}
}
