using Godot;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

public partial class GameManager : Node
{
	public static Dictionary<long, PlayerInfo> players = new Dictionary<long, PlayerInfo>();
	public static List<long> playerIds = new List<long>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Returns the index of the player id itToFind discounting myPlayerId
	/// E.g if the players array is [1, 2, 3, 4] and you are player 2 and want the index of player 3 you will get 1 back as 3 is the 2nd player which isnt you. 
	/// </summary>
	/// <param name="myPlayerId">Your player ID</param>
	/// <param name="idToFind">Player ID to find</param>
	/// <returns>Index of player ID discounting you, -1 = not found. </returns>
	static public int GetPlayerIndexExcludingMe(long myPlayerId, long idToFind) {
		int index = 0;
		foreach(long id in playerIds)
		{
			if(id == idToFind) return index;
			if (id != myPlayerId) index++;
		}
		return -1;
	}
}
