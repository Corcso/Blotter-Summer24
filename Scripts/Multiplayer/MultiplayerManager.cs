using Godot;
using System;
using System.Collections.Generic;

public partial class MultiplayerManager : Node
{
    [Export] LineEdit address;
    int port = 41761;

    private ENetMultiplayerPeer peer;

    [Export] GridContainer playersListGrid;
    [Export] Control lobbyMenu;
    [Export] LineEdit playerNameInput;
    [Export] Control disconnectBox;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        Multiplayer.PeerConnected += (id) => PeerConnected(id);
        Multiplayer.PeerDisconnected += (id) => PeerDisconnected(id);
        Multiplayer.ConnectedToServer += () => ConnectedToServer();
        Multiplayer.ConnectionFailed += () => ConnectionFailed();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void PeerConnected(long id)
    {
        GD.Print("Peer Connected " + id.ToString());
    }

    public void PeerDisconnected(long id)
    {
        GD.Print("Peer Disconnected " + id.ToString());
        //if (!Multiplayer.IsServer()) Multiplayer.MultiplayerPeer.DisconnectPeer(1);
        // On disconnect end the game in progress (if there is one)
        Node2D gameScene = GetTree().Root.GetNodeOrNull<Node2D>("./Game Scene");
        if(gameScene != null) { 
            gameScene.QueueFree();
            GetParent<Control>().Show();
        }
        // Show disconnected box
        disconnectBox.Visible = true;
        // Delete multiplayer peer
        GD.Print("ID CALLING DSETROY " + Multiplayer.GetUniqueId());
        peer.Host.Destroy();
        Multiplayer.MultiplayerPeer = null;
        peer = null;
        // Close the lobby if it is open
        lobbyMenu.Visible = false;
        // Also delete all player names
        foreach (Node n in lobbyMenu.GetNode<Node>("./Grid").GetChildren()) { 
            n.QueueFree();
        }

        // Clear the game manager
        GameManager.playerIds.Clear();
        GameManager.players.Clear();
    }

    public void ConnectedToServer()
    {
        GD.Print("Connected To Server");
        
        RpcId(1, "SendPlayerInformation", (playerNameInput.Text == "") ? "Player" : playerNameInput.Text, Multiplayer.GetUniqueId());
    }

    public void ConnectionFailed()
    {
        GD.Print("Connection Failed");
    }

    public void _on_host_button_pressed() {
        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(port, 4);
        if (error != Error.Ok) { 
            GD.Print(error.ToString());
            return;
        }
        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Hosting Begin");
        // Send player information to ourself, no need for RPC as only user
        SendPlayerInformation((playerNameInput.Text == "") ? "Host" : playerNameInput.Text, 1);
        // Open the Lobby
        lobbyMenu.Visible = true;
        // Hide waiting on host, show play button
        lobbyMenu.GetNode<Control>("./Waiting On Host").Visible = false;
        lobbyMenu.GetNode<Control>("./Play Button").Visible = true;
    }
    public void _on_join_button_pressed() {
        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(address.Text, port);
        if (error != Error.Ok)
        {
            GD.Print(error.ToString());
            return;
        }
        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

        Multiplayer.MultiplayerPeer = peer;
        GD.Print("joined");
        // Open the Lobby
        lobbyMenu.Visible = true;
        // Show waiting on host, hide play button
        lobbyMenu.GetNode<Control>("./Waiting On Host").Visible = true;
        lobbyMenu.GetNode<Control>("./Play Button").Visible = false;
    }

    public void _on_play_button_pressed() {
        Rpc("StartGame");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void StartGame() {
        Node2D scene = ResourceLoader.Load<PackedScene>("res://Scenes/Game Scenes/Main Play Scene.tscn").Instantiate<Node2D>();
        GetTree().Root.AddChild(scene);
        scene.Name = "Game Scene";
        this.GetParent<Control>().Hide();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SendPlayerInformation(string name, long id) { 
        PlayerInfo info = new PlayerInfo() { 
            name = name,
            id = id 
        };
        if (!GameManager.players.ContainsKey(id)) {
            GameManager.players[id] = info;
        }
        if (!GameManager.playerIds.Contains(id))
        {
            GameManager.playerIds.Add(id);
        }
        foreach (Node n in playersListGrid.GetChildren()) n.QueueFree();
        foreach (KeyValuePair<long, PlayerInfo> playerPair in GameManager.players)
        {
            Label playerName = new Label();
            playersListGrid.AddChild(playerName);
            playerName.Text = playerPair.Value.name;
            playerName.AddThemeFontSizeOverride("font_size", 48);
            playerName.HorizontalAlignment = HorizontalAlignment.Center;
            playerName.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        }
        if (Multiplayer.IsServer()) { 
            foreach (KeyValuePair<long, PlayerInfo> playerPair in GameManager.players)
            {
                Rpc("SendPlayerInformation", playerPair.Value.name, playerPair.Key);
            }
        }
    }
}
