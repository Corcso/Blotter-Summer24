using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BingoCardGenerator : GodotObject
{
    // Random number generator used for card generation
    RandomNumberGenerator rng;

    public int[] numbers;

    //private void G1GenerateBingoCard()
    //{
    //    GD.Print("starting generation");

    //    int attempts = 100000;
    //    while (attempts >= 100000)
    //    {
    //        attempts = 0;
    //        List<int> chosenNumbers = new List<int>();
    //        for (int i = 0; i < 15; i++)
    //        {
    //            int numbersThisRow = 0;
    //            while (numbersThisRow != 6)
    //            {
    //                numbersThisRow = 0;
    //                for (int j = 0; j < 9; j++)
    //                {
    //                    if (rng.RandiRange(0, 1) == 0)
    //                    {
    //                        numbers[i, j] = 0;
    //                        //GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = "  ";
    //                    }
    //                    else
    //                    {
    //                        int thisCell = 0;
    //                        do
    //                        {
    //                            attempts++;
    //                            switch (j)
    //                            {
    //                                case 0: thisCell = rng.RandiRange(1, 9); break;
    //                                case 1: thisCell = rng.RandiRange(10, 19); break;
    //                                case 2: thisCell = rng.RandiRange(20, 29); break;
    //                                case 3: thisCell = rng.RandiRange(30, 39); break;
    //                                case 4: thisCell = rng.RandiRange(40, 49); break;
    //                                case 5: thisCell = rng.RandiRange(50, 59); break;
    //                                case 6: thisCell = rng.RandiRange(60, 69); break;
    //                                case 7: thisCell = rng.RandiRange(70, 79); break;
    //                                case 8: thisCell = rng.RandiRange(80, 90); break;
    //                            }
    //                        } while (chosenNumbers.Contains(thisCell) && attempts < 100000);
    //                        if (attempts >= 100000) { break; }
    //                        chosenNumbers.Add(thisCell);
    //                        numbers[i, j] = thisCell;
    //                        //GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = thisCell.ToString();
    //                        numbersThisRow++;
    //                    }
    //                }
    //                if (attempts >= 100000) { break; }
    //                //GD.Print("row " + i.ToString() + " gen with " + numbersThisRow.ToString());
    //            }
    //            if (attempts >= 100000) { break; }
    //            GD.Print("row " + i.ToString() + " has 6");
    //        }
    //    }
    //}

    //private void G2GenerateBingoCardCol()
    //{
    //    bool cardValid = false;
    //    while (!cardValid)
    //    {
    //        cardValid = true;
    //        List<int> chosenNumbers = new List<int>();
    //        for (int j = 0; j < 9; j++)
    //        {
    //            int numbersThisRow = 0;
    //            int numbersToBeThisCol = (j == 0) ? 9 : ((j == 8) ? 11 : 10);
    //            while (numbersThisRow != numbersToBeThisCol)
    //            {
    //                numbersThisRow = 0;
    //                for (int i = 0; i < 15; i++)
    //                {
    //                    if (rng.RandiRange(0, 1) == 0)
    //                    {
    //                        numbers[i, j] = 0;
    //                        //GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = "  ";
    //                    }
    //                    else
    //                    {
    //                        int thisCell = 0;
    //                        do
    //                        {
    //                            switch (j)
    //                            {
    //                                case 0: thisCell = rng.RandiRange(1, 9); break;
    //                                case 1: thisCell = rng.RandiRange(10, 19); break;
    //                                case 2: thisCell = rng.RandiRange(20, 29); break;
    //                                case 3: thisCell = rng.RandiRange(30, 39); break;
    //                                case 4: thisCell = rng.RandiRange(40, 49); break;
    //                                case 5: thisCell = rng.RandiRange(50, 59); break;
    //                                case 6: thisCell = rng.RandiRange(60, 69); break;
    //                                case 7: thisCell = rng.RandiRange(70, 79); break;
    //                                case 8: thisCell = rng.RandiRange(80, 90); break;
    //                            }
    //                        } while (chosenNumbers.Contains(thisCell));
    //                        chosenNumbers.Add(thisCell);
    //                        numbers[i, j] = thisCell;
    //                       // GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = thisCell.ToString();
    //                        numbersThisRow++;
    //                    }
    //                }
    //                //GD.Print("row " + i.ToString() + " gen with " + numbersThisRow.ToString());
    //            }
    //        }
    //    }
    //}

    //private void G3GenerateCardTwoStage()
    //{
    //    List<int> chosenNumbers = new List<int>();
    //    int[] colRequirements = new int[] { 9, 10, 10, 10, 10, 10, 10, 10, 11 };
    //    for (int i = 0; i < 15; i++)
    //    {
    //        int numbersThisRow = 0;
    //        while (numbersThisRow != 6)
    //        {
    //            numbersThisRow = 0;
    //            List<int> colsPopulated = new List<int>();
    //            for (int j = 0; j < 9; j++)
    //            {
    //                if (rng.RandiRange(0, 1) == 1 && colRequirements[j] > 0)
    //                {
    //                    numbers[i, j] = 99;
    //                    numbersThisRow++;
    //                    colsPopulated.Add(j);
    //                }
    //                else
    //                {
    //                    numbers[i, j] = 0;
    //                }
    //            }
    //            foreach (int col in colsPopulated) colRequirements[col]--;
    //        }
    //    }
    //    for (int i = 0; i < 15; i++)
    //    {
    //        for (int j = 0; j < 9; j++)
    //        {
    //            if (numbers[i, j] == 99)
    //            {
    //                int thisCell = 0;
    //                do
    //                {
    //                    switch (j)
    //                    {
    //                        case 0: thisCell = rng.RandiRange(1, 9); break;
    //                        case 1: thisCell = rng.RandiRange(10, 19); break;
    //                        case 2: thisCell = rng.RandiRange(20, 29); break;
    //                        case 3: thisCell = rng.RandiRange(30, 39); break;
    //                        case 4: thisCell = rng.RandiRange(40, 49); break;
    //                        case 5: thisCell = rng.RandiRange(50, 59); break;
    //                        case 6: thisCell = rng.RandiRange(60, 69); break;
    //                        case 7: thisCell = rng.RandiRange(70, 79); break;
    //                        case 8: thisCell = rng.RandiRange(80, 90); break;
    //                    }
    //                } while (chosenNumbers.Contains(thisCell));
    //                chosenNumbers.Add(thisCell);
    //                numbers[i, j] = thisCell;
    //                //GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = thisCell.ToString();
    //            }
    //            else
    //            {
    //                //GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = "  ";
    //            }
    //        }
    //    }
    //}

    ///// <summary>
    ///// 4th Generation of bingo card generation algorithm
    ///// Can theoretically hang forever, generates a valid card structure first then fills in the numbers
    ///// Will keep randomly generating cards until 1 is valid. 
    ///// </summary>
    //public void G4GenerateCardTwoStageReDo()
    //{
    //    rng = new RandomNumberGenerator();

    //    List<int> chosenNumbers = new List<int>();
    //    int[] colRequirements = new int[] { 9, 10, 10, 10, 10, 10, 10, 10, 11 };
    //    bool validTable = false;
    //    while (!validTable)
    //    {
    //        numbers = new int[15, 9];
    //        validTable = true;
    //        for (int i = 0; i < 15; i++)
    //        {
    //            int numbersThisRow = 0;
    //            while (numbersThisRow != 6)
    //            {
    //                numbersThisRow = 0;
    //                for (int j = 0; j < 9; j++)
    //                {
    //                    if (rng.RandiRange(0, 1) == 1)
    //                    {
    //                        numbers[i, j] = 99;
    //                        numbersThisRow++;
    //                    }
    //                    else
    //                    {
    //                        numbers[i, j] = 0;
    //                    }
    //                }
    //            }
    //        }
    //        for (int i = 0; i < 9; i++)
    //        {
    //            int numbersThisCol = 0;
    //            for (int j = 0; j < 15; j++)
    //            {
    //                if (numbers[j, i] == 99)
    //                {
    //                    numbersThisCol++;
    //                }
    //            }
    //            if (numbersThisCol != colRequirements[i])
    //            {
    //                validTable = false;
    //                break;
    //            }
    //        }
    //    }

    //    for (int i = 0; i < 15; i++)
    //    {
    //        for (int j = 0; j < 9; j++)
    //        {
    //            if (numbers[i, j] == 99)
    //            {
    //                int thisCell = 0;
    //                do
    //                {
    //                    switch (j)
    //                    {
    //                        case 0: thisCell = rng.RandiRange(1, 9); break;
    //                        case 1: thisCell = rng.RandiRange(10, 19); break;
    //                        case 2: thisCell = rng.RandiRange(20, 29); break;
    //                        case 3: thisCell = rng.RandiRange(30, 39); break;
    //                        case 4: thisCell = rng.RandiRange(40, 49); break;
    //                        case 5: thisCell = rng.RandiRange(50, 59); break;
    //                        case 6: thisCell = rng.RandiRange(60, 69); break;
    //                        case 7: thisCell = rng.RandiRange(70, 79); break;
    //                        case 8: thisCell = rng.RandiRange(80, 90); break;
    //                    }
    //                } while (chosenNumbers.Contains(thisCell));
    //                chosenNumbers.Add(thisCell);
    //                numbers[i, j] = thisCell;
    //                //GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = thisCell.ToString();
    //            }
    //            else
    //            {
    //                //GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = "  ";
    //            }
    //        }
    //    }
    //}

    ///// <summary>
    ///// 5th Generation of bingo card generation algorithm, frame based solution, returns true on success
    ///// Can theoretically hang forever, generates a valid card structure first then fills in the numbers
    ///// Will keep randomly generating cards until 1 is valid. 
    ///// </summary>
    //public bool G5GenerateCardTwoStageReDo()
    //{
    //    rng = new RandomNumberGenerator();

    //    List<int> chosenNumbers = new List<int>();
    //    int[] colRequirements = new int[] { 9, 10, 10, 10, 10, 10, 10, 10, 11 };
    //    List<int> colRequirementsDecrease = new List<int> { 9, 10, 10, 10, 10, 10, 10, 10, 11 };
    //    bool validTable = true;
    //    numbers = new int[15, 9];
    //    validTable = true;
    //    for (int i = 0; i < 15; i++)
    //    {
    //        int numbersThisRow = 0;
    //        while (numbersThisRow != 6)
    //        {
    //            if (colRequirementsDecrease.FindAll((int i) => { return i == 0; }).Count > 3) return false;

    //            numbersThisRow = 0;
    //            for (int j = 0; j < 9; j++)
    //            {
    //                if (colRequirementsDecrease[j] > 0 && rng.Randf() <= 4.0f / 9.0f + 1.0f / (colRequirementsDecrease[j] + 2))
    //                {
    //                    numbers[i, j] = 99;
    //                    numbersThisRow++;
    //                    colRequirementsDecrease[j]--;
    //                    if (colRequirementsDecrease[j] < 0) colRequirementsDecrease[j] = 0;
    //                }
    //                else
    //                {
    //                    numbers[i, j] = 0;
    //                }
    //            }
    //        }
    //    }
    //    for (int i = 0; i < 9; i++)
    //    {
    //        int numbersThisCol = 0;
    //        for (int j = 0; j < 15; j++)
    //        {
    //            if (numbers[j, i] == 99)
    //            {
    //                numbersThisCol++;
    //            }
    //        }
    //        if (numbersThisCol != colRequirements[i])
    //        {
    //            validTable = false;
    //            GD.Print("Table failed column " + i.ToString() + " had " + numbersThisCol.ToString() + " numbers!");
    //            break;
    //        }
    //    }

    //    if (validTable)
    //    {
    //        for (int i = 0; i < 15; i++)
    //        {
    //            for (int j = 0; j < 9; j++)
    //            {
    //                if (numbers[i, j] == 99)
    //                {
    //                    int thisCell = 0;
    //                    do
    //                    {
    //                        switch (j)
    //                        {
    //                            case 0: thisCell = rng.RandiRange(1, 9); break;
    //                            case 1: thisCell = rng.RandiRange(10, 19); break;
    //                            case 2: thisCell = rng.RandiRange(20, 29); break;
    //                            case 3: thisCell = rng.RandiRange(30, 39); break;
    //                            case 4: thisCell = rng.RandiRange(40, 49); break;
    //                            case 5: thisCell = rng.RandiRange(50, 59); break;
    //                            case 6: thisCell = rng.RandiRange(60, 69); break;
    //                            case 7: thisCell = rng.RandiRange(70, 79); break;
    //                            case 8: thisCell = rng.RandiRange(80, 90); break;
    //                        }
    //                    } while (chosenNumbers.Contains(thisCell));
    //                    chosenNumbers.Add(thisCell);
    //                    numbers[i, j] = thisCell;
    //                    //GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = thisCell.ToString();
    //                }
    //                else
    //                {
    //                    //GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = "  ";
    //                }
    //            }
    //        }
    //        return true;
    //    }
    //    else return false;
    //}

    public bool G6GenerateCardTwoStageReDo()
    {
        rng = new RandomNumberGenerator();

        List<int> chosenNumbers = new List<int>();
        int[] colRequirements = new int[] { 9, 10, 10, 10, 10, 10, 10, 10, 11 };
        bool validTable = false;
        int attempts = 0;
        while (!validTable && attempts < 5000)
        {
            attempts++;
            numbers = new int[18 * 9];
            validTable = true;
            for (int i = 0; i < 18; i++)
            {
                int numbersThisRow = 0;
                while (numbersThisRow != 5)
                {
                    numbersThisRow = 0;
                    for (int j = 0; j < 9; j++)
                    {
                        if (rng.RandiRange(0, 1) == 1)
                        {
                            numbers[i * 9 + j] = 99;
                            numbersThisRow++;
                        }
                        else
                        {
                            numbers[i * 9 + j] = 0;
                        }
                    }
                }
            }
            for (int i = 0; i < 9; i++)
            {
                int numbersThisCol = 0;
                for (int j = 0; j < 18; j++)
                {
                    if (numbers[j * 9 + i] == 99)
                    {
                        numbersThisCol++;
                    }
                }
                if (numbersThisCol != colRequirements[i])
                {
                    validTable = false;
                    break;
                }
            }
        }
        if (!validTable) return false;
        for (int i = 0; i < 18; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (numbers[i * 9 + j] == 99)
                {
                    int thisCell = 0;
                    do
                    {
                        switch (j)
                        {
                            case 0: thisCell = rng.RandiRange(1, 9); break;
                            case 1: thisCell = rng.RandiRange(10, 19); break;
                            case 2: thisCell = rng.RandiRange(20, 29); break;
                            case 3: thisCell = rng.RandiRange(30, 39); break;
                            case 4: thisCell = rng.RandiRange(40, 49); break;
                            case 5: thisCell = rng.RandiRange(50, 59); break;
                            case 6: thisCell = rng.RandiRange(60, 69); break;
                            case 7: thisCell = rng.RandiRange(70, 79); break;
                            case 8: thisCell = rng.RandiRange(80, 90); break;
                        }
                    } while (chosenNumbers.Contains(thisCell));
                    chosenNumbers.Add(thisCell);
                    numbers[i * 9 + j] = thisCell;
                    //GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = thisCell.ToString();
                }
                else
                {
                    //GetNode<Node>("./Numbers").GetChild<Label>((i * 9) + j).Text = "  ";
                }
            }
        }
        return true;
    }
}
