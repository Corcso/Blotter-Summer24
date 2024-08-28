using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BingoManager : Node
{
	// Enum for game state
	public enum GameState { GENERATING_CARDS, CARD_SLIDE_IN, BALL_ROLL, DRAWING_BALL, AFTER_BALL_SYNC, BINGO, NEW_GAME_TYPE, GAME_END };

	// Current game state
	GameState currentGameState;
	GameState previousGameState;
	bool stateChanged;

	// Array 3 long for the winners
	long[] winnerIds;

	// Used for the animations
	double timeInState;

	// RNG for ball selection
	RandomNumberGenerator rng = new RandomNumberGenerator();

	// Sync count recieved 
	// This is a count which is increased from each user of if they have hit the end of the ball draw
	// This is so all ball draws happen at the same time.
	int syncCount; 

	// == Bingo Ball Stuff ==
	[Export] Node2D bingoBall;
	Node2D bingoBallRotation; // Use this special node for rotation so the shine is kept stationary
	Vector2 START_BALL_POSITION = new Vector2(250, -500);
	Vector2 HANG_BALL_POSITION = new Vector2(0, -150);
	Vector2 END_BALL_POSITION = new Vector2(0, 500);
	int ballToCall = 0;

    // Player id to their bingo card.
    public Godot.Collections.Dictionary<long, BingoCard> playersCards;

	// Bingo Card Scene
	[Export] PackedScene bingoCardScene;

	// Enum for round states
	public enum BingoGameType { ROW, FULL_HOUSE, THREE_FULL_HOSUE };

	// Current type of game
	BingoGameType currentGameType;

    // Array of numbers called this round
    List<int> calledBalls;

	// == Card generation stage variables
	Node2D generatingCardsPopup;

    // == Bingo win specific animatory vars ==
    long BW_winningPlayerId;
    Vector2 BW_moveCardPosition;
	Node2D BW_bingoBoomPopup;
	Vector2 BW_BallPositionWhenBingoPressed;
    Label BW_playerNameBoomText;

	// == New Cards Animation variables
	Node2D playerCardHolder;
	Node2D otherPlayerCardHolder;
    BingoButton bingoButton;
	FontFile kalinaBold;

	// == New Game Type Animation variables
	Node2D newGameTypePopupBox;

	// == Game Over Animation variables
	Node2D gameOverPopupBox;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Set the initial game type
		currentGameType = BingoGameType.ROW;

		// Setup the called balls array
		calledBalls = new List<int>();
		for (int i = 1; i < 91;  i++) { calledBalls.Add(i); }

        // Setup winners array 
        winnerIds = new long[3];

		// Current sync count is 0

        // Setup the players cards
        playersCards = new Godot.Collections.Dictionary<long, BingoCard>();

        // Set default game state
        currentGameState = GameState.GENERATING_CARDS;
		previousGameState = GameState.GENERATING_CARDS;
        stateChanged = true;

        // Set ball related variables
        bingoBallRotation = bingoBall.GetNode<Node2D>("./Inner Rotate");

        // Setup card generation popup
        generatingCardsPopup = GetParent().GetNode<Node2D>("./Generating Cards Popup");

        // Setup bingo win nodes
        BW_bingoBoomPopup = GetNode<Node2D>("../Bingo Boom Text");
        BW_playerNameBoomText = GetNode<Label>("../Player Name Bingo Boom Text");

        // Setup card holders
        otherPlayerCardHolder = GetParent().GetNode<Node2D>("./Other Players Cards");
		playerCardHolder = GetParent().GetNode<Node2D>("./Player Bingo Card");
        bingoButton = GetParent().GetNode<BingoButton>("./Bingo Button");

        // Setup new game type popup
        newGameTypePopupBox = GetParent().GetNode<Node2D>("./New Game Type Popup");

        // Setup game over popup
        gameOverPopupBox = GetParent().GetNode<Node2D>("./Game Over Popup");

        // Load font for player names
        kalinaBold = ResourceLoader.Load<FontFile>("res://Fonts/Kalina/Kalnia-Bold.ttf");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        // Constants for animation
        // Card position constants
        Vector2 MY_WIN_START_CARD_POSITION = new Vector2(-316, 0);
        Vector2 MY_WIN_HOLD_CARD_POSITION = new Vector2(-82, 0);
        Vector2 OTHER_WIN_HOLD_CARD_POSITION = new Vector2(0, -1960);
        Vector2 MY_LOSS_HOLD_CARD_POSITION = new Vector2(-750, 0);
        // Text position constants
        Vector2 BINGO_BOOM_START_POSITION = new Vector2(61, 565);
        Vector2 BINGO_BOOM_HOLD_POSITION = new Vector2(-308, 39);
        Vector2 BINGO_BOOM_END_POSITION = new Vector2(-811, -679);
        Vector2 PLAYER_NAME_BINGO_BOOM_START_POSITION = new Vector2(-430, -922);
        Vector2 PLAYER_NAME_BINGO_BOOM_HOLD_POSITION = new Vector2(-51, -381);
        Vector2 PLAYER_NAME_BINGO_BOOM_END_POSITION = new Vector2(464, 354);
        // Switch over game state
        switch (currentGameState)
		{
			case GameState.GENERATING_CARDS:
				if (stateChanged) {
					stateChanged = false;
                    playerCardHolder.Position = new Vector2(-316, -650);
                    otherPlayerCardHolder.Position = new Vector2(700, 0);
					bingoButton.Position = new Vector2(-128, 380);
					generatingCardsPopup.Visible = true;
                }
				if (Multiplayer.IsServer()) SetupNewBingoCards();
                break;
			case GameState.CARD_SLIDE_IN:
                playerCardHolder.Position = (Vector2)Tween.InterpolateValue(new Vector2(-316, -650), new Vector2(0, 650), timeInState, 0.75, Tween.TransitionType.Cubic, Tween.EaseType.Out);
                playerCardHolder.RotationDegrees = (float)Tween.InterpolateValue(-35, 35, timeInState, 0.75, Tween.TransitionType.Cubic, Tween.EaseType.Out);
                otherPlayerCardHolder.Position = (Vector2)Tween.InterpolateValue(new Vector2(700, 0), new Vector2(-200, 0), timeInState, 0.75, Tween.TransitionType.Cubic, Tween.EaseType.Out);
                bingoButton.Position = (Vector2)Tween.InterpolateValue(new Vector2(-128, 380), new Vector2(0, -200), timeInState, 0.75, Tween.TransitionType.Cubic, Tween.EaseType.Out);
				if (timeInState >= 0.75) {
					// Sync up first before starting
					ChangeState(GameState.AFTER_BALL_SYNC);
                    break;
                }
                break;
			// == BALL ROLLING ==
            case GameState.BALL_ROLL:
                if (stateChanged)
                {
					stateChanged = false;
					if (Multiplayer.IsServer())
					{
						GD.Print("Rolling Ball");
						// Generate a new ball making sure it hasnt been chosen already
						do
						{
							ballToCall = rng.RandiRange(1, 900);
						} while (calledBalls.Contains(ballToCall));
						Rpc("SetCurrentBall", ballToCall);
					}
					// Play bingo roll sound effect
					bingoBall.GetNode<AudioStreamPlayer>("./Ball Roll").Play();
					// Set ball colour (client side)
                    int ballColourChoice = rng.RandiRange(0, 5);
                    Color ballColour = new Color(1, 0, 0); // Initialse as red but this will be overwritten
					switch (ballColourChoice) {
						case 0: ballColour = new Color(1, 0, 0); break; // Red
                        case 1: ballColour = new Color(0, 0.76f, 0); break; // Green
                        case 2: ballColour = new Color(0, 0.7f, 0.7f); break; // Cyan/Teal
                        case 3: ballColour = new Color(0.85f, 0.7f, 0); break; // Yellow
                        case 4: ballColour = new Color(0.66f, 0, 0.85f); break; // Purple
                        case 5: ballColour = new Color(0.22f, 0.36f, 0.75f); break; // Blue
                    }
					bingoBallRotation.GetNode<Sprite2D>("./Ball Background").Modulate = ballColour;
					bingoBallRotation.GetNode<Sprite2D>("./Ball Ring").Modulate = ballColour;

					
					
                }
                if (timeInState >= 2)
                {
					ChangeState(GameState.DRAWING_BALL);
                    break;
                }
                break;
			// == BALL DRAWING ==
            case GameState.DRAWING_BALL:
				// If we just got into drawing ball state set the ball to call into the called balls array
				if (stateChanged)
				{
					stateChanged = false;
                    // Add the ball to the called balls array
                    // This means now bingos with this ball can be called
                    //calledBalls.Append(ballToCall);

                    // Play bingo ball slide sound effect
                    bingoBall.GetNode<AudioStreamPlayer>("./Ball Slide In").Play();
                }
				// Animate the ball into position
                bingoBall.Position = START_BALL_POSITION.Lerp(HANG_BALL_POSITION, Mathf.Clamp((float)timeInState, 0, 1));
				bingoBall.Scale = Vector2.One * Mathf.Lerp(0, 1.0f, Mathf.Clamp((float)timeInState, 0, 1));
				bingoBallRotation.Rotation = Mathf.Sin(2.0f * (float)timeInState) * 0.4f;
				// Let the ball hang then fall on the third second
				if (timeInState >= 3) {
					// Use a t^2 curve
                    bingoBall.Position = HANG_BALL_POSITION.Lerp(END_BALL_POSITION, Mathf.Clamp(((float)timeInState - 3.0f) * ((float)timeInState - 3.0f), 0, 1));
				}
				// If its been 4 seconds roll another ball.
                if (timeInState >= 4)
                {
					ChangeState(GameState.AFTER_BALL_SYNC);
                    break;
                }
                break;
			case GameState.AFTER_BALL_SYNC:
				// When we hit the after ball sync send our RPC to other players to say we are here
                if (stateChanged)
                {
                    stateChanged = false;
					Rpc("SyncAfterBallDraw");
                }
				// Wait until sync has been recieved from all players
				if (syncCount >= GameManager.playerIds.Count) {
                    ChangeState(GameState.BALL_ROLL);
                    break;
                }
                break;
			case GameState.BINGO:
				if (stateChanged) { 
					stateChanged = false;
					if (BW_winningPlayerId != Multiplayer.GetUniqueId()) { 
						BW_moveCardPosition = playersCards[BW_winningPlayerId].Position;
					}
					BW_BallPositionWhenBingoPressed = bingoBall.Position;
					BW_playerNameBoomText.Text = GameManager.players[BW_winningPlayerId].name;
					BW_playerNameBoomText.LabelSettings.OutlineColor = GameManager.players[BW_winningPlayerId].cardColor;
                }
                if (timeInState <= 0.5f)
				{
					

                    otherPlayerCardHolder.Position = (Vector2)Tween.InterpolateValue(new Vector2(500, 0), new Vector2(200, 0), timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                    if (BW_winningPlayerId != Multiplayer.GetUniqueId())
					{
                        playerCardHolder.Position = (Vector2)Tween.InterpolateValue(MY_WIN_START_CARD_POSITION, MY_LOSS_HOLD_CARD_POSITION - MY_WIN_START_CARD_POSITION, timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                        playersCards[BW_winningPlayerId].Position = (Vector2)Tween.InterpolateValue(BW_moveCardPosition, OTHER_WIN_HOLD_CARD_POSITION - BW_moveCardPosition, timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
						playersCards[BW_winningPlayerId].RotationDegrees = (float)Tween.InterpolateValue(0.0f, 55.0f, timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
						playersCards[BW_winningPlayerId].Scale = Vector2.One * (float)Tween.InterpolateValue(1f, 2f, timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
					}
					else { 
                        playerCardHolder.Position = (Vector2)Tween.InterpolateValue(MY_WIN_START_CARD_POSITION, MY_WIN_HOLD_CARD_POSITION - MY_WIN_START_CARD_POSITION, timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                        playerCardHolder.RotationDegrees = (float)Tween.InterpolateValue(0.0f, -35.0f, timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                        playerCardHolder.Scale = Vector2.One * (float)Tween.InterpolateValue(1f, 0.2f, timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                    }
					if(previousGameState == GameState.DRAWING_BALL)
					{
						bingoBall.Position = BW_BallPositionWhenBingoPressed.Lerp(END_BALL_POSITION, Mathf.Clamp((float)timeInState * 2 * ((float)timeInState * 2), 0, 1));
                    }
                }
				else if (timeInState <= 1.0f)
				{
                    BW_bingoBoomPopup.Position = (Vector2)Tween.InterpolateValue(BINGO_BOOM_START_POSITION, BINGO_BOOM_HOLD_POSITION - BINGO_BOOM_START_POSITION, timeInState - 0.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.Out);
                    BW_playerNameBoomText.Position = (Vector2)Tween.InterpolateValue(PLAYER_NAME_BINGO_BOOM_START_POSITION, PLAYER_NAME_BINGO_BOOM_HOLD_POSITION - PLAYER_NAME_BINGO_BOOM_START_POSITION, timeInState - 0.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.Out);
                }
                else if (timeInState <= 2.5f && timeInState > 2.0f)
                {
                    BW_bingoBoomPopup.Position = (Vector2)Tween.InterpolateValue(BINGO_BOOM_HOLD_POSITION, BINGO_BOOM_END_POSITION - BINGO_BOOM_HOLD_POSITION, timeInState - 2.0f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.In);
                    BW_playerNameBoomText.Position = (Vector2)Tween.InterpolateValue(PLAYER_NAME_BINGO_BOOM_HOLD_POSITION, PLAYER_NAME_BINGO_BOOM_END_POSITION - PLAYER_NAME_BINGO_BOOM_HOLD_POSITION, timeInState - 2.0f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.In);
                }
                else if (timeInState <= 3f && timeInState > 2.5f)
				{
                    //BW_bingoBoomPopup.Position = (Vector2)Tween.InterpolateValue(BINGO_BOOM_HOLD_POSITION, BINGO_BOOM_END_POSITION - BINGO_BOOM_HOLD_POSITION, timeInState - 2.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.In);

                    otherPlayerCardHolder.Position = (Vector2)Tween.InterpolateValue(new Vector2(700, 0), new Vector2(-200, 0), timeInState - 2.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                    if (BW_winningPlayerId != Multiplayer.GetUniqueId())
					{
                        playerCardHolder.Position = (Vector2)Tween.InterpolateValue(MY_LOSS_HOLD_CARD_POSITION, MY_WIN_START_CARD_POSITION - MY_LOSS_HOLD_CARD_POSITION, timeInState - 2.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                        playersCards[BW_winningPlayerId].Position = (Vector2)Tween.InterpolateValue(OTHER_WIN_HOLD_CARD_POSITION, -OTHER_WIN_HOLD_CARD_POSITION + BW_moveCardPosition, timeInState - 2.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                        playersCards[BW_winningPlayerId].RotationDegrees = (float)Tween.InterpolateValue(55.0f, -55.0f, timeInState - 2.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                        playersCards[BW_winningPlayerId].Scale = Vector2.One * (float)Tween.InterpolateValue(3f, -2f, timeInState - 2.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                    }
                    else
                    {
                        playerCardHolder.Position = (Vector2)Tween.InterpolateValue(MY_WIN_HOLD_CARD_POSITION, MY_WIN_START_CARD_POSITION - MY_WIN_HOLD_CARD_POSITION, timeInState - 2.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                        playerCardHolder.RotationDegrees = (float)Tween.InterpolateValue(-35.0f, 35.0f, timeInState - 2.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                        playerCardHolder.Scale = Vector2.One * (float)Tween.InterpolateValue(1.2f, -0.2f, timeInState - 2.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                    }
                }
				else if (timeInState > 3.0f) {
					//if(previousGameState == GameState.DRAWING_BALL) ChangeState(GameState.DRAWING_BALL);
					//else ChangeState(GameState.BALL_ROLL);
					if (currentGameType == BingoGameType.THREE_FULL_HOSUE) ChangeState(GameState.GAME_END);
					else ChangeState(GameState.NEW_GAME_TYPE);
					break;
                }
                break;
			case GameState.NEW_GAME_TYPE:
                Vector2 POPUP_START_POSITION = new Vector2(0, -500);
                Vector2 POPUP_HOLD_POSITION = new Vector2(0, 0);
                if (stateChanged)
                {
                    stateChanged = false;
					if (currentGameType == BingoGameType.ROW)
					{
						currentGameType = BingoGameType.FULL_HOUSE;
						newGameTypePopupBox.GetChild<Label>(0).Text = "Now Playing For\nFull House!";
					}
					else
					{
						currentGameType = BingoGameType.THREE_FULL_HOSUE;
						newGameTypePopupBox.GetChild<Label>(0).Text = "Now Playing For\n 3 Full Houses!";
					}
					newGameTypePopupBox.Show();
                }
				if (timeInState <= 0.7f)
				{
					newGameTypePopupBox.Position = (Vector2)Tween.InterpolateValue(POPUP_START_POSITION, POPUP_HOLD_POSITION - POPUP_START_POSITION, timeInState, 0.7, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                }
				else if (timeInState > 1.4f && timeInState <= 2.1f)
				{
                    newGameTypePopupBox.Position = (Vector2)Tween.InterpolateValue(POPUP_HOLD_POSITION, POPUP_START_POSITION - POPUP_HOLD_POSITION, timeInState - 1.4f, 0.7, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                }
				else if (timeInState > 2.1f) { 
					ChangeState(GameState.BALL_ROLL);
					newGameTypePopupBox.Hide();
				}
                break;
			case GameState.GAME_END:
                POPUP_START_POSITION = new Vector2(0, -650);
                POPUP_HOLD_POSITION = new Vector2(0, 0);
                if (stateChanged)
                {
                    stateChanged = false;
					// Set winners names
					gameOverPopupBox.GetNode<Label>("./Row Winner Name").Text = GameManager.players[winnerIds[0]].name;
					gameOverPopupBox.GetNode<Label>("./Row Winner Name").LabelSettings.OutlineColor = GameManager.players[winnerIds[0]].cardColor;
					gameOverPopupBox.GetNode<Label>("./FH Winner Name").Text = GameManager.players[winnerIds[1]].name;
                    gameOverPopupBox.GetNode<Label>("./FH Winner Name").LabelSettings.OutlineColor = GameManager.players[winnerIds[1]].cardColor;
                    gameOverPopupBox.GetNode<Label>("./TFH Winner Name").Text = GameManager.players[winnerIds[2]].name;
                    gameOverPopupBox.GetNode<Label>("./TFH Winner Name").LabelSettings.OutlineColor = GameManager.players[winnerIds[2]].cardColor;

                    // Make the box visible
                    gameOverPopupBox.Show();

					// Make the waiting on host visible if not host, or host buttons visible if are. 
					if (Multiplayer.IsServer())
					{
						gameOverPopupBox.GetNode<Control>("./Waiting For Host").Hide();
						gameOverPopupBox.GetNode<Control>("./Play Again Button").Show();
						gameOverPopupBox.GetNode<Control>("./Exit Game Button").Show();
					}
					else {
                        gameOverPopupBox.GetNode<Control>("./Waiting For Host").Show();
                        gameOverPopupBox.GetNode<Control>("./Play Again Button").Hide();
                        gameOverPopupBox.GetNode<Control>("./Exit Game Button").Hide();
                    }
                }
                if (timeInState <= 0.7f)
                {
					// Move the popup box in
                    gameOverPopupBox.Position = (Vector2)Tween.InterpolateValue(POPUP_START_POSITION, POPUP_HOLD_POSITION - POPUP_START_POSITION, timeInState, 0.7, Tween.TransitionType.Cubic, Tween.EaseType.InOut);

                    // Move the bingo cards out the way
                    playerCardHolder.Position = (Vector2)Tween.InterpolateValue(MY_WIN_START_CARD_POSITION, MY_LOSS_HOLD_CARD_POSITION - MY_WIN_START_CARD_POSITION, timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                    otherPlayerCardHolder.Position = (Vector2)Tween.InterpolateValue(new Vector2(500, 0), new Vector2(200, 0), timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);

					// Hide the bingo button 
					bingoButton.HideBingoButton();
                }
                break;
		}
		timeInState += delta;
	}

	private void ChangeState(GameState state) {
		previousGameState = currentGameState;
        currentGameState = state;
        timeInState = 0;
        stateChanged = true;
    }

    // Recieved from other players (and self) when they reach the end of ball draw
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void SyncAfterBallDraw() {
		syncCount++;
	}

	public bool CheckBingo(int playerId) { 
		// Switch over each game type for different bingo checks. 
		switch (currentGameType)
		{
			case BingoGameType.ROW:
				return CheckRowBingo(playerId);
			case BingoGameType.FULL_HOUSE:
				return CheckFHBingo(playerId);
			case BingoGameType.THREE_FULL_HOSUE:
				return Check3FHBingo(playerId);
		}
		return false;
	}

	private bool CheckThisRowBingo(int playerId, int rowIdx) {
        // Loop over each number in that row
        // All rows should have 5 numbers so a row of 0s should not be an issue.
        for (int j = 0; j < playersCards[playerId].numbers.GetLength(1); j++)
        {
            // If this number is a 0 ignore it as it is a blank space
            // If any numbers in that row were not called
            // Mark the row as not a bingo and stop looping this row
            if (playersCards[playerId].numbers[rowIdx, j] != 0 && !calledBalls.Contains(playersCards[playerId].numbers[rowIdx, j]))
            {
				return false;
            }
        }
        // If we have made it to the end of the loop all number matched and this row is a bingo
        return true;
    }

	private bool CheckRowBingo(int playerId) {
		// Loop over each row
		for (int i = 0; i < playersCards[playerId].numbers.GetLength(0); i++)
		{
			// Check this row is a bingo using the check this bingo row function 
			// If we have a bingo on this row no need to look further return a bingo
            if (CheckThisRowBingo(playerId, i)) return true;
        }
		// If no row bingos found return false
		return false;
	}

    private bool CheckFHBingo(int playerId)
    {
        // Loop over each 3rd row
        for (int i = 0; i < playersCards[playerId].numbers.GetLength(0); i += 3)
        {
            // Check the 3 rows in this box, if all 3 are bingos then we have a full house
            if (CheckThisRowBingo(playerId, i) && CheckThisRowBingo(playerId, i + 1) && CheckThisRowBingo(playerId, i + 2)) return true;
        }
        // If no row bingos found return false
        return false;
    }

    private bool Check3FHBingo(int playerId)
    {
		// Start a count for the number of full houses
		int fullHouseCount = 0;
        // Loop over each 3rd row
        for (int i = 0; i < playersCards[playerId].numbers.GetLength(0); i += 3)
        {
            // Check the 3 rows in this box, if all 3 are bingos then we have a full house
			// Add 1 to the full house count if we have a full house
            if (CheckThisRowBingo(playerId, i) && CheckThisRowBingo(playerId, i + 1) && CheckThisRowBingo(playerId, i + 2)) fullHouseCount++;
        }
        // If we have 3 full houses or more we have a bingo
        return fullHouseCount >= 3;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void SetCurrentBall(int ball) {
		ballToCall = ball;
        // Set the balls text to that ball
        bingoBall.GetNode<Label>("./Inner Rotate/Number").Text = ballToCall.ToString();
        // Reset ball position and scale
        bingoBall.Position = START_BALL_POSITION;
        bingoBall.Scale = Vector2.Zero;
    }

	private int onPlayerIndexForGeneration = 0;

	private void SetupNewBingoCards() {
		// Create a new card for them and generate its numbers
        BingoCardGenerator generator = new BingoCardGenerator();
		GD.Print("Attempting generation for player id " + GameManager.playerIds[onPlayerIndexForGeneration].ToString());
		if (generator.G6GenerateCardTwoStageReDo()) {
            // Send the numbers to all players to create a card
            Rpc("RegisterNewCard", GameManager.playerIds[onPlayerIndexForGeneration], generator.numbers);
			onPlayerIndexForGeneration++;
        }
		if (onPlayerIndexForGeneration >= GameManager.playerIds.Count)
		{
			Rpc("BeginGameWithCards");
		}
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RegisterNewCard(long playerId, int[] numbers) {

		BingoCard card = bingoCardScene.Instantiate<BingoCard>();
		card.GetNode<Node2D>("./Background").Modulate = GameManager.players[playerId].cardColor;
		card.UnpackNumbers(numbers);
		card.PopulateNumbers();
		// If this is my card spawn it in my card's location
		if (playerId == Multiplayer.GetUniqueId())
		{
			// Add card to world
			playerCardHolder.CallDeferred(Node.MethodName.AddChild, card);
            // Register my pen to this card
            GetParent().GetNode<Pen>("./My Pen").myBingoCard = card;	
        }
		// If this isnt my card add to other players section
		else { 
			// Add card to world
			otherPlayerCardHolder.CallDeferred(Node.MethodName.AddChild, card);
			// Get the position of this player
			int playerIndex = GameManager.GetPlayerIndexExcludingMe(Multiplayer.GetUniqueId(), playerId);
			// Create the player name label
			Label playerName = new Label();
			playerName.Text = GameManager.players[playerId].name;
			playerName.LabelSettings = new LabelSettings();
			playerName.LabelSettings.Font = kalinaBold; 
			playerName.LabelSettings.FontSize = 64; 
			playerName.LabelSettings.OutlineColor = GameManager.players[playerId].cardColor;
			playerName.LabelSettings.OutlineSize = 8;
			playerName.RotationDegrees = 90;
            otherPlayerCardHolder.CallDeferred(Node.MethodName.AddChild, playerName);
            switch (playerIndex)
			{
				case 0:
					card.Position = new Vector2(500, 0);
					playerName.Position = new Vector2(702, -255);
					break;
				case 1:
					card.Position = new Vector2(0, 0);
                    playerName.Position = new Vector2(202, -255);
                    break;
				case 2:
                    card.Position = new Vector2(-500, 0);
                    playerName.Position = new Vector2(-298, -255);
                    break;

            }
        }
		// Register card with bingo manager
		playersCards[playerId] = card;
	}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void BeginGameWithCards() {
		// Set default game state
		ChangeState(GameState.CARD_SLIDE_IN);
        generatingCardsPopup.Visible = false;
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void BingoWin(long playerId) {
		ChangeState(GameState.BINGO);

		BW_winningPlayerId = playerId;

		// Hide the bingo button if I am not the winner too. 
		if (playerId != Multiplayer.GetUniqueId()) bingoButton.HideBingoButton(3);

		// Add that player to the winners list
		winnerIds[(int)currentGameType] = playerId;

		// Play win SFX
		bingoButton.GetNode<AudioStreamPlayer>("./Bingo Win").Play();
    }

	public void _on_bingo_button_pressed() {
		if (CheckBingo(Multiplayer.GetUniqueId()))
		{
			Rpc("BingoWin", Multiplayer.GetUniqueId());
		}
		else {
			GD.Print("NO BINGO");
			// Play no bingo sound
			bingoButton.GetNode<AudioStreamPlayer>("./Bingo Not Success").Play();
		}
	}

}
