using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BingoManager : Node
{
	// Enum for game state
	public enum GameState { GENERATING_CARDS, CARD_SLIDE_IN, BALL_ROLL, DRAWING_BALL, BINGO };

	// Current game state
	GameState currentGameState;
	GameState previousGameState;
	bool stateChanged;
	// Used for the animations
	double timeInState;

	// RNG for ball selection
	RandomNumberGenerator rng = new RandomNumberGenerator();

	// == Bingo Ball Stuff ==
	[Export] Node2D bingoBall;
	Node2D bingoBallRotation; // Use this special node for rotation so the shine is kept stationary
	Vector2 START_BALL_POSITION = new Vector2(250, -500);
	Vector2 HANG_BALL_POSITION = new Vector2(0, -150);
	Vector2 END_BALL_POSITION = new Vector2(0, 500);
	int ballToCall = 0;

    // Player id to their bingo card.
    Godot.Collections.Dictionary<long, BingoCard> playersCards;

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

	// == New Cards Animation variables
	Node2D playerCardHolder;
	Node2D otherPlayerCardHolder;
    BingoButton bingoButton;
	FontFile kalinaBold;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Set the initial game type
		currentGameType = BingoGameType.ROW;

		// Setup the called balls array
		calledBalls = new List<int>();
		for (int i = 1; i < 91;  i++) { calledBalls.Add(i); }

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

		// Setup card holders
		otherPlayerCardHolder = GetParent().GetNode<Node2D>("./Other Players Cards");
		playerCardHolder = GetParent().GetNode<Node2D>("./Player Bingo Card");
        bingoButton = GetParent().GetNode<BingoButton>("./Bingo Button");

		// Load font for player names
		kalinaBold = ResourceLoader.Load<FontFile>("res://Fonts/Kalina/Kalnia-Bold.ttf");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
					ChangeState(GameState.BALL_ROLL);
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
				}
				// Animate the ball into position
                bingoBall.Position = START_BALL_POSITION.Lerp(HANG_BALL_POSITION, Mathf.Clamp((float)timeInState, 0, 1));
				bingoBall.Scale = Vector2.One * Mathf.Lerp(0, 4.6f, Mathf.Clamp((float)timeInState, 0, 1));
				bingoBallRotation.Rotation = Mathf.Sin(2.0f * (float)timeInState) * 0.4f;
				// Let the ball hang then fall on the third second
				if (timeInState >= 3) {
					// Use a t^2 curve
                    bingoBall.Position = HANG_BALL_POSITION.Lerp(END_BALL_POSITION, Mathf.Clamp(((float)timeInState - 3.0f) * ((float)timeInState - 3.0f), 0, 1));
				}
				// If its been 4 seconds roll another ball.
                if (timeInState >= 4)
                {
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

                }
				Vector2 MY_WIN_START_CARD_POSITION = new Vector2(-316, 0);
				Vector2 MY_WIN_HOLD_CARD_POSITION = new Vector2(-82, 0);
                Vector2 OTHER_WIN_HOLD_CARD_POSITION = new Vector2(0, -1960);
				Vector2 MY_LOSS_HOLD_CARD_POSITION = new Vector2(-750, 0);
                if (timeInState <= 0.5f)
				{
					//BW_bingoBoomPopup.Rotation = Mathf.Lerp(0, 2 * Mathf.Pi, (float)timeInState * 2);
					BW_bingoBoomPopup.Rotation = (float)Tween.InterpolateValue(0.0f, 2 * Mathf.Pi, timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                    BW_bingoBoomPopup.Scale = Vector2.One * (float)Tween.InterpolateValue(0.0f, 1.0f, timeInState, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);

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
						bingoBall.Position = BW_BallPositionWhenBingoPressed.Lerp(END_BALL_POSITION, Mathf.Clamp(((float)timeInState * 2) * ((float)timeInState * 2), 0, 1));
                    }
                }
				else if (timeInState <= 2.5f)
				{
					BW_bingoBoomPopup.Rotation = Mathf.Sin((Mathf.Pi * (float)timeInState) - 1.5f) / 2.5f;
				}
				else if (timeInState <= 3f)
				{
                    BW_bingoBoomPopup.Rotation = (float)Tween.InterpolateValue(2 * Mathf.Pi, 4 * Mathf.Pi, timeInState - 2.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
                    BW_bingoBoomPopup.Scale = Vector2.One * (float)Tween.InterpolateValue(1.0f, -1.0f, timeInState - 2.5f, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);

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
				else {
					if(previousGameState == GameState.DRAWING_BALL) ChangeState(GameState.DRAWING_BALL);
					else ChangeState(GameState.BALL_ROLL);
					break;
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

	public bool CheckBingo(int playerId) { 
		// Switch over each game type for different bingo checks. 
		switch (currentGameType)
		{
			case BingoGameType.ROW:
				return CheckRowBingo(playerId);
			case BingoGameType.FULL_HOUSE:
				return false;
			case BingoGameType.THREE_FULL_HOSUE:
				return false;
		}
		return false;
	}

	private bool CheckRowBingo(int playerId) {
		// Loop over each row
		for (int i = 0; i < playersCards[playerId].numbers.GetLength(0); i++)
		{
			// Loop over each number in that row
			// Assume the row is a bingo
			bool rowBingo = true; 

			// All rows should have 5 numbers so a row of 0s should not be an issue.

            for (int j = 0; j < playersCards[playerId].numbers.GetLength(1); j++)
            {
				// If this number is a 0 ignore it as it is a blank space
				// If any numbers in that row were not called
				// Mark the row as not a bingo and stop looping this row
                if (playersCards[playerId].numbers[i, j] != 0 && !calledBalls.Contains(playersCards[playerId].numbers[i, j])) {
					rowBingo = false;
					break;
				}
            }
			// If we have a bingo on this row no need to look further return a bingo
            if (rowBingo) return true;
			
        }
		// If no row bingos found return false
		return false;
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
			playerName.LabelSettings.OutlineColor = new Color(255, 127, 0);
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
    }

	public void _on_bingo_button_pressed() {
		if (CheckBingo(Multiplayer.GetUniqueId()))
		{
			Rpc("BingoWin", Multiplayer.GetUniqueId());
		}
		else {
			GD.Print("NO BINGO");
		}
	}
}
