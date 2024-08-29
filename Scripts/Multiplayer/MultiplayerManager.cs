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
    [Export] Control cantConnectBox;
    [Export] Control connectingBox;

    public Color playerPenColor;
    public Color playerCardColor;

    // Buttons for card and pen colour
    // Used to hide them when joined game and show when disconnected. 
    [Export] Control penColorButton;
    [Export] Control cardColorButton;

    // Sound effects for main menu
    [Export] AudioStreamPlayer errorSFX;

    // Boolean to store if we are currently in game, used by host to kick players which connect mid game.
    bool inGame = false;

    // Boolean and timer to check if we are connecting. 
    bool connecting = false;
    float timeConnecting = 0;

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
        // If we have been connecting for more than 10 seconds time out. 
        if (connecting) {
            timeConnecting += (float)delta;
            if (timeConnecting > 10.0f) ConnectionFailed();
        }
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
        // Show all colour and name inputs, these can be changed again
        penColorButton.Visible = true;
        cardColorButton.Visible = true;
        playerNameInput.Visible = true;
        // Show disconnected box
        disconnectBox.Visible = true;
        // Delete multiplayer peer
        GD.Print("ID CALLING DSETROY " + Multiplayer.GetUniqueId());
        ENetConnection eNetConnection = peer.Host;
        peer.Close();
        eNetConnection.Destroy();
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

        // Set us no longer in game 
        inGame = false;

        // Play error sound
        errorSFX.Play();
    }

    public void ConnectedToServer()
    {
        // We are no longer connecting
        connecting = false;
        // Hide connecting box
        connectingBox.Visible = false;
            

        // Hide all colour and name inputs, these cant be changed once joined
        penColorButton.Visible = false;
        cardColorButton.Visible = false;
        playerNameInput.Visible = false;

        GD.Print("Connected To Server");
        // Open the Lobby
        lobbyMenu.Visible = true;
        // Show waiting on host, hide play button, hide waiting for players (because you will be at least 2nd player)
        lobbyMenu.GetNode<Control>("./Waiting On Host").Visible = true;
        lobbyMenu.GetNode<Control>("./Play Button").Visible = false;
        lobbyMenu.GetNode<Control>("./Waiting For Players").Visible = false;

        // Send our info to the server/host
        RpcId(1, "SendPlayerInformation", (playerNameInput.Text == "") ? "Player" : playerNameInput.Text, Multiplayer.GetUniqueId(), playerPenColor, playerCardColor);
    }

    public void ConnectionFailed()
    {
        GD.Print("Connection Failed");
        // We are no longer connecting
        connecting = false;
        // Hide connecting box
        connectingBox.Visible = false;
        // Show cant connect box
        cantConnectBox.Visible = true;
        // Play error sound
        errorSFX.Play();
    }

    public void _on_host_button_pressed() {
        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(port, 4);
        if (error != Error.Ok) { 
            GD.Print(error.ToString());
            // Play error sound
            errorSFX.Play();
            return;
        }
        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Hosting Begin");
        // Send player information to ourself, no need for RPC as only user
        SendPlayerInformation((playerNameInput.Text == "") ? "Host" : playerNameInput.Text, 1, playerPenColor, playerCardColor);
        // Open the Lobby
        lobbyMenu.Visible = true;
        // Hide waiting on host, hide play button, show waiting for players (initially)
        lobbyMenu.GetNode<Control>("./Waiting For Players").Visible = true;
        lobbyMenu.GetNode<Control>("./Waiting On Host").Visible = false;
        lobbyMenu.GetNode<Control>("./Play Button").Visible = false;


        // Hide all colour and name inputs, these cant be changed once joined
        penColorButton.Visible = false;
        cardColorButton.Visible = false;
        playerNameInput.Visible = false;

    }
    public void _on_join_button_pressed() {
        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(address.Text, port);
        if (error != Error.Ok)
        {
            GD.Print(error.ToString());
            // Play error sound
            errorSFX.Play();
            return;
        }
        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

        Multiplayer.MultiplayerPeer = peer;
        GD.Print("joined");
        GD.Print(peer.GetConnectionStatus().ToString());
        


        // Set connecting and connection timer
        timeConnecting = 0;
        connecting = true;
        // Show connecting box
        connectingBox.Visible = true;
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
        // Set in game to true
        inGame = true;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SendPlayerInformation(string name, long id, Color penColor, Color cardColor) {
        // If we are the host and are in game, force disconnect this player and return
        if (Multiplayer.IsServer() && inGame) { 
            peer.DisconnectPeer((int)id, true);
            return;
        }
        PlayerInfo info = new PlayerInfo() { 
            name = name,
            id = id,
            penColor = penColor,
            cardColor = cardColor
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
                Rpc("SendPlayerInformation", playerPair.Value.name, playerPair.Key, playerPair.Value.penColor, playerPair.Value.cardColor);
            }

            // If host and this is 2nd+ player then show play button
            if (GameManager.playerIds.Count >= 2) {
                // Hide waiting on host, show play button, hide waiting for players
                lobbyMenu.GetNode<Control>("./Waiting For Players").Visible = false;
                lobbyMenu.GetNode<Control>("./Waiting On Host").Visible = false;
                lobbyMenu.GetNode<Control>("./Play Button").Visible = true;
            }
        }
    }

    public void _on_leave_lobby_pressed() {
        PeerDisconnected(Multiplayer.GetUniqueId());
    }
}
