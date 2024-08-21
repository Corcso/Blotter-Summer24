using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

public partial class BingoCard : Node2D
{

	// 2D array of the numbers on the card. 
	// 0 Indicates a blank as there is no 0 ball.
	public int[,] numbers;

	// The ID of the player who owns this bingo card. 
	public long playerId;

	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void PopulateNumbers() { 
		for (int i = 0; i < numbers.GetLength(0); i++) {
			for (int j = 0; j < numbers.GetLength(1); j++) {
				int row = Mathf.FloorToInt(((i * 9) + j) / 27);
				int rowIdx = ((i * 9) + j) % 27;
                if (numbers[i, j] == 0) GetNode<Node>("./Row"+ row.ToString()).GetChild<Label>(rowIdx).Text = " ";
                else GetNode<Node>("./Row" + row.ToString()).GetChild<Label>(rowIdx).Text = numbers[i, j].ToString();
            }
		}
	}

	public void UnpackNumbers(int[] numbersFlat)
	{
		numbers = new int[18, 9];
		for (int i = 0; i < 18 * 9; i++) {
			numbers[i / 9, i % 9] = numbersFlat[i];
		}
	}
}
