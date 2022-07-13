using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum MCTSCellState
{
    OUT_OF_BOUNDS,
    EMPTY,
    FRIENDLY,
    ENEMY
}

public enum MCTSGameState
{
    ON_GOING,
    WHITE_WIN,
    BLACK_WIN,
    DRAW
}

public class MCTSBoard
{
    #region Members
    public int[] boardValues;
    public int totalMoves = 0;
    public int boardSize;
    public int currentPlayer;

    public const int DEFAULT_BOARD_SIZE = 15*15;
    #endregion

    #region Constructors
    public MCTSBoard()
    {
        boardSize = DEFAULT_BOARD_SIZE;
        boardValues = new int[]
        {
            -1,   0,     -1,   0,     -1,   0,     -1,   0,    -1,    0,    -1,    0,   -1,    0,   -1,
            -12,  -2,    -12,  -2,    -12,  -2,    -12,  -13,  -12,   -2,   -12,   -2,  -12,   -2,  -12,
            -1,   -11,   -1,   -11,   -1,   -11,   -1,   -11,  -1,    -11,  -1,    -11, -1,    -11, -1,
            -11,  -2,    -11,  -2,    -11,  -2,    -11,  -2,   -11,   -2,   -11,   -2,  -11,   -2,  -11,
            -1,   0,     -1,   0,     -1,   0,     -1,   0,    -1,    0,    -1,    0,   -1,    0,   -1,
            0,    -2,    0,    -2,    0,    -2,    0,    -2,   0,     -2,   0,     -2,  0,     -2,  0,
            -1,   0,     -1,   0,     -1,   0,     -1,   0,    -1,    0,    -1,    0,   -1,    0,   -1,
            0,    -2,    0,    -2,    0,    -2,    0,    -2,   0,     -2,   0,     -2,  0,     -2,  0,
            -1,   0,     -1,   0,     -1,   0,     -1,   0,    -1,    0,    -1,    0,   -1,    0,   -1,
            0,    -2,    0,    -2,    0,    -2,    0,    -2,   0,     -2,   0,     -2,  0,     -2,  0,
            -1,   0,     -1,   0,     -1,   0,     -1,   0,    -1,    0,    -1,    0,   -1,    0,   -1,
            11,   -2,    11,   -2,    11,   -2,    11,   -2,   11,    -2,   11,    -2,  11,    -2,  11,
            -1,   11,    -1,   11,    -1,   11,    -1,   11,   -1,    11,   -1,    11,  -1,    11,  -1,
            12,   -2,    12,   -2,    12,   -2,    12,   13,   12,    -2,   12,    -2,  12,    -2,  12,
            -1,   0,     -1,   0,     -1,   0,     -1,   0,    -1,    0,    -1,    0,   -1,    0,   -1
        };
        currentPlayer = 1;
    }

    public MCTSBoard(int[] boardValues, int currentPlayer)
    {
        this.boardValues = boardValues;
        boardSize = boardValues.Length;
        this.currentPlayer = currentPlayer;
    }

    public MCTSBoard(int[] boardValues, int totalMoves, int currentPlayer)
    {
        this.boardValues = boardValues;
        boardSize = this.boardValues.Length;
        this.totalMoves = totalMoves;
        this.currentPlayer = currentPlayer;
    }

    public MCTSBoard(MCTSBoard board)
    {
        boardSize = board.boardValues.Length;
        boardValues = new int[boardSize];
        for (int i = 0; i < boardValues.Length; i++)
        {
            boardValues[i] = board.boardValues[i];
        }
        currentPlayer = board.currentPlayer;
    }
    #endregion

    #region ValidateCell
    private MCTSCellState ValidateCell(int currentCell, int cellToCheck)
    {
        if (cellToCheck < 0 || cellToCheck >= boardSize)
            return MCTSCellState.OUT_OF_BOUNDS;
        if ((cellToCheck - currentCell) % 15 != 0 && (cellToCheck < (int)(currentCell / 15f) * 15 || cellToCheck >= (int)(currentCell / 15f + 1) * 15))
            return MCTSCellState.OUT_OF_BOUNDS;
        if (boardValues[cellToCheck] == 0 || boardValues[cellToCheck] == -1 || boardValues[cellToCheck] == -2)
            return MCTSCellState.EMPTY;
        if (boardValues[cellToCheck] / 10 == boardValues[currentCell] / 10)
            return MCTSCellState.FRIENDLY;
        else
            return MCTSCellState.ENEMY;
    }
    #endregion

    #region GetAllowedActions
    public List<Tuple<int, int>> GetAllowedActions()
    {
        List<Tuple<int, int>> allowed = new List<Tuple<int, int>>();
        bool mandatoryPawnMove = false, mandatoryQueenMove = false;
        for (int i = 0; i < boardSize; i++)
        {
            if (boardValues[i] / 10 == currentPlayer)
            {
                if (Math.Abs(boardValues[i]) == 11 && !mandatoryQueenMove)
                {
                    // Pawn Logic Here
                    int colorMult = boardValues[i] / 10; // Piece Color Identification
                    if (!mandatoryPawnMove)
                    {
                        if (ValidateCell(i - 15 * colorMult, i - 14 * colorMult) == MCTSCellState.EMPTY)
                            allowed.Add(new Tuple<int, int>(i, i - 14 * colorMult));
                        if (ValidateCell(i - 15 * colorMult, i - 16 * colorMult) == MCTSCellState.EMPTY)
                            allowed.Add(new Tuple<int, int>(i, i - 16 * colorMult));
                        if (ValidateCell(i, i - 30 * colorMult) == MCTSCellState.EMPTY && ValidateCell(i, i - 15 * colorMult) == MCTSCellState.EMPTY)
                            allowed.Add(new Tuple<int, int>(i, i - 30 * colorMult));
                        // Pawn Double Starting Step
                        if ((colorMult == 1 && i / 15 == 12) || (colorMult == -1 && i / 15 == 2) && ValidateCell(i, i - 60 * colorMult) == MCTSCellState.EMPTY && ValidateCell(i, i - 45 * colorMult) == MCTSCellState.EMPTY)
                            allowed.Add(new Tuple<int, int>(i, i - 60 * colorMult));
                    }
                    if ((i / 15) % 2 == 0) // Non Rotated Pawn Identification
                    {
                        Tuple<int, int> possibleMove = null;
                        for (int j = 2; j < 15; j += 4)
                        {
                            if (ValidateCell(i, i - (j - 1) * 15 * colorMult) == MCTSCellState.EMPTY && ValidateCell(i, i - j * 15 * colorMult) == MCTSCellState.ENEMY && ValidateCell(i, i - (j + 1) * 15 * colorMult) == MCTSCellState.EMPTY && ValidateCell(i, i - (j + 2) * 15 * colorMult) == MCTSCellState.EMPTY)
                            {
                                possibleMove = new Tuple<int, int>(i, i - (j + 2) * 15 * colorMult);
                                if (!mandatoryPawnMove)
                                    allowed = new List<Tuple<int, int>>();
                                mandatoryPawnMove = true;
                            }
                            else
                            {
                                if (possibleMove != null)
                                    allowed.Add(possibleMove);
                                break;
                            }
                        }
                    }
                }
                else if (Math.Abs(boardValues[i]) == 12)
                {
                    // Queen Logic Here
                    List<Tuple<int, int>> tmp = new List<Tuple<int, int>>();
                    bool checkingUp, checkingDown, checkingLeft, checkingRight;
                    checkingUp = checkingDown = checkingLeft = checkingRight = true;
                    bool mandatoryUp, mandatoryDown, mandatoryLeft, mandatoryRight;
                    mandatoryUp = mandatoryDown = mandatoryLeft = mandatoryRight = false;
                    if (!mandatoryQueenMove && !mandatoryPawnMove)
                    {
                        if (ValidateCell(i - 15, i - 14) == MCTSCellState.EMPTY)
                            tmp.Add(new Tuple<int, int>(i, i - 14));
                        if (ValidateCell(i - 15, i - 16) == MCTSCellState.EMPTY)
                            tmp.Add(new Tuple<int, int>(i, i - 16));
                        if (ValidateCell(i + 15, i + 14) == MCTSCellState.EMPTY)
                            tmp.Add(new Tuple<int, int>(i, i + 14));
                        if (ValidateCell(i + 15, i + 16) == MCTSCellState.EMPTY)
                            tmp.Add(new Tuple<int, int>(i, i + 16));
                    }
                    for (int j = 2; j < 15; j += 2)
                    {
                        if (!mandatoryLeft && !mandatoryRight)
                        {
                            if (checkingUp)
                            {
                                // Rotated Queen Identification
                                if ((i / 15) % 2 == 1)
                                {
                                    if (!mandatoryQueenMove && !mandatoryPawnMove)
                                    {
                                        if (ValidateCell(i, i - 15 * j) == MCTSCellState.EMPTY)
                                            tmp.Add(new Tuple<int, int>(i, i - 15 * j));
                                        else
                                            checkingUp = false;
                                    }
                                }
                                else // Non Rotated Queen Identification
                                {
                                    if (ValidateCell(i, i - (j - 1) * 15) == MCTSCellState.EMPTY)
                                    {
                                        if (ValidateCell(i, i - j * 15) == MCTSCellState.EMPTY && (!mandatoryQueenMove || mandatoryUp))
                                            tmp.Add(new Tuple<int, int>(i, i - 15 * j));
                                        else if (ValidateCell(i, i - j * 15) == MCTSCellState.ENEMY && ValidateCell(i, i - (j + 1) * 15) == MCTSCellState.EMPTY && ValidateCell(i, i - (j + 2) * 15) == MCTSCellState.EMPTY)
                                        {
                                            tmp = new List<Tuple<int, int>>();
                                            if (!mandatoryQueenMove)
                                                allowed = new List<Tuple<int, int>>();
                                            mandatoryQueenMove = mandatoryUp = true;
                                        }
                                        else
                                            checkingUp = false;
                                    }
                                    else
                                        checkingUp = false;
                                }
                            }
                            if (checkingDown)
                            {
                                // Rotated Queen Identification
                                if ((i / 15) % 2 == 1)
                                {
                                    if (!mandatoryQueenMove && !mandatoryPawnMove)
                                    {
                                        if (ValidateCell(i, i + 15 * j) == MCTSCellState.EMPTY)
                                            tmp.Add(new Tuple<int, int>(i, i + 15 * j));
                                        else
                                            checkingDown = false;
                                    }
                                }
                                else // Non Rotated Queen Identification
                                {
                                    if (ValidateCell(i, i + (j - 1) * 15) == MCTSCellState.EMPTY)
                                    {
                                        if (ValidateCell(i, i + j * 15) == MCTSCellState.EMPTY && (!mandatoryQueenMove || mandatoryDown))
                                            tmp.Add(new Tuple<int, int>(i, i + 15 * j));
                                        else if (ValidateCell(i, i + j * 15) == MCTSCellState.ENEMY && ValidateCell(i, i + (j + 1) * 15) == MCTSCellState.EMPTY && ValidateCell(i, i + (j + 2) * 15) == MCTSCellState.EMPTY)
                                        {
                                            tmp = new List<Tuple<int, int>>();
                                            if (!mandatoryQueenMove)
                                                allowed = new List<Tuple<int, int>>();
                                            mandatoryQueenMove = mandatoryDown = true;
                                        }
                                        else
                                            checkingDown = false;
                                    }
                                    else
                                        checkingDown = false;
                                }
                            }
                        }
                        if (!mandatoryUp && !mandatoryDown)
                        {
                            if (checkingLeft)
                            {
                                // Non Rotated Queen Identification
                                if ((i / 15) % 2 == 0)
                                {
                                    if (!mandatoryQueenMove && !mandatoryPawnMove)
                                    {
                                        if (ValidateCell(i, i - j) == MCTSCellState.EMPTY)
                                            tmp.Add(new Tuple<int, int>(i, i - j));
                                        else
                                            checkingLeft = false;
                                    }
                                }
                                else // Rotated Queen Identification
                                {
                                    if (ValidateCell(i, i - (j - 1)) == MCTSCellState.EMPTY)
                                    {
                                        if (ValidateCell(i, i - j) == MCTSCellState.EMPTY && (!mandatoryQueenMove || mandatoryLeft))
                                            tmp.Add(new Tuple<int, int>(i, i - j));
                                        else if (ValidateCell(i, i - j) == MCTSCellState.ENEMY && ValidateCell(i, i - (j + 1)) == MCTSCellState.EMPTY && ValidateCell(i, i - (j + 2)) == MCTSCellState.EMPTY)
                                        {
                                            tmp = new List<Tuple<int, int>>();
                                            if (!mandatoryQueenMove)
                                                allowed = new List<Tuple<int, int>>();
                                            mandatoryQueenMove = mandatoryLeft = true;
                                        }
                                        else
                                            checkingLeft = false;
                                    }
                                    else
                                        checkingLeft = false;
                                }
                            }
                            if (checkingRight)
                            {
                                // Non Rotated Queen Identification
                                if ((i / 15) % 2 == 0)
                                {
                                    if (!mandatoryQueenMove && !mandatoryPawnMove)
                                    {
                                        if (ValidateCell(i, i + j) == MCTSCellState.EMPTY)
                                            tmp.Add(new Tuple<int, int>(i, i + j));
                                        else
                                            checkingRight = false;
                                    }
                                }
                                else // Rotated Queen Identification
                                {
                                    if (ValidateCell(i, i + (j - 1)) == MCTSCellState.EMPTY)
                                    {
                                        if (ValidateCell(i, i + j) == MCTSCellState.EMPTY && (!mandatoryQueenMove || mandatoryRight))
                                            tmp.Add(new Tuple<int, int>(i, i + j));
                                        else if (ValidateCell(i, i + j) == MCTSCellState.ENEMY && ValidateCell(i, i + (j + 1)) == MCTSCellState.EMPTY && ValidateCell(i, i + (j + 2)) == MCTSCellState.EMPTY)
                                        {
                                            tmp = new List<Tuple<int, int>>();
                                            if (!mandatoryQueenMove)
                                                allowed = new List<Tuple<int, int>>();
                                            mandatoryQueenMove = mandatoryRight = true;
                                        }
                                        else
                                            checkingRight = false;
                                    }
                                    else
                                        checkingRight = false;
                                }
                            }
                        }
                    }
                    foreach (Tuple<int, int> action in tmp)
                        allowed.Add(action);
                }
                else if (Math.Abs(boardValues[i]) == 13)
                {
                    // King Logic Here
                    if (!mandatoryPawnMove && !mandatoryQueenMove)
                    {
                        if (ValidateCell(i, i + 30) == MCTSCellState.EMPTY && ValidateCell(i, i + 15) == MCTSCellState.EMPTY && (ValidateCell(i, i + 60) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i, i + 60) == MCTSCellState.EMPTY) && (ValidateCell(i + 60, i + 62) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i + 60, i + 62) == MCTSCellState.EMPTY) && (ValidateCell(i + 60, i + 58) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i + 60, i + 58) == MCTSCellState.EMPTY))
                            allowed.Add(new Tuple<int, int>(i, i + 30));
                        if (ValidateCell(i, i - 30) == MCTSCellState.EMPTY && ValidateCell(i, i - 15) == MCTSCellState.EMPTY && (ValidateCell(i, i - 60) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i, i - 60) == MCTSCellState.EMPTY) && (ValidateCell(i - 60, i - 58) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i - 60, i - 58) == MCTSCellState.EMPTY) && (ValidateCell(i - 60, i - 62) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i - 60, i - 62) == MCTSCellState.EMPTY))
                            allowed.Add(new Tuple<int, int>(i, i - 30));
                        if (ValidateCell(i, i + 2) == MCTSCellState.EMPTY && ValidateCell(i, i + 1) == MCTSCellState.EMPTY && (ValidateCell(i, i + 4) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i, i + 4) == MCTSCellState.EMPTY) && (ValidateCell(i + 15, i + 4 + 15) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i + 15, i + 4 + 15) == MCTSCellState.EMPTY) && (ValidateCell(i - 15, i + 4 - 15) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i - 15, i + 4 - 15) == MCTSCellState.EMPTY))
                            allowed.Add(new Tuple<int, int>(i, i + 2));
                        if (ValidateCell(i, i - 2) == MCTSCellState.EMPTY && ValidateCell(i, i - 1) == MCTSCellState.EMPTY && (ValidateCell(i, i - 4) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i, i - 4) == MCTSCellState.EMPTY) && (ValidateCell(i + 15, i - 4 + 15) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i + 15, i - 4 + 15) == MCTSCellState.EMPTY) && (ValidateCell(i - 15, i - 4 - 15) == MCTSCellState.OUT_OF_BOUNDS || ValidateCell(i - 15, i - 4 - 15) == MCTSCellState.EMPTY))
                            allowed.Add(new Tuple<int, int>(i, i - 2));
                    }
                }
            }
        }
        return allowed;
    }
    #endregion

    #region PerformAction
    public void PerformAction(Tuple<int, int> action)
    {
        totalMoves++;
        int oldValue = boardValues[action.Item1];
        // King Action
        if (Math.Abs(boardValues[action.Item1]) == 13)
            boardValues[action.Item1] = -2;
        // Pawn/Queen Action
        else
        {
            boardValues[action.Item1] = 0;
            int distance = action.Item2 - action.Item1;
            int direction = distance / Math.Abs(distance);
            int step = 0;
            if (distance % 15 == 0 || (Math.Abs(oldValue) == 12 && ValidateCell(action.Item1, action.Item2) != MCTSCellState.OUT_OF_BOUNDS))
            {
                // Pawn/Queen Vertical Movement
                if (distance % 15 == 0)
                    step = 30;
                // Queen Horizontal Movement
                else if (Math.Abs(oldValue) == 12 && ValidateCell(action.Item1, action.Item2) != MCTSCellState.OUT_OF_BOUNDS)
                    step = 2;
                int normalizedDistance = Math.Abs(distance);
                for (int i = step; i < normalizedDistance; i += step)
                    boardValues[action.Item1 + i * direction] = 0;
            }
        }
        boardValues[action.Item2] = oldValue;
        if (Math.Abs(boardValues[action.Item2]) == 11 && (action.Item2 / 15 == 0 || action.Item2 / 15 == 14))
        {
            int colorMult = oldValue / 10;
            boardValues[action.Item2] = 12 * colorMult;
        }
    }
    #endregion

    #region CheckState
    public GameState CheckState()
    {
        if (GetAllowedActions().Count == 0)
            return GameState.DRAW;
        for (int i = 0; i < boardSize; i++)
        {
            if (Math.Abs(boardValues[i]) == 13)
            {
                int surroundingPieces = 0;
                int opponentPieces = 0;
                if (ValidateCell(i, i - 15) != MCTSCellState.EMPTY)
                {
                    surroundingPieces += 1;
                    if (ValidateCell(i, i - 15) == MCTSCellState.ENEMY)
                        opponentPieces += 1;
                }
                if (ValidateCell(i, i + 15) != MCTSCellState.EMPTY)
                {
                    surroundingPieces += 1;
                    if (ValidateCell(i, i + 15) == MCTSCellState.ENEMY)
                        opponentPieces += 1;
                }
                if (ValidateCell(i, i - 1) != MCTSCellState.EMPTY)
                {
                    surroundingPieces += 1;
                    if (ValidateCell(i, i - 1) == MCTSCellState.ENEMY)
                        opponentPieces += 1;
                }
                if (ValidateCell(i, i + 1) != MCTSCellState.EMPTY)
                {
                    surroundingPieces += 1;
                    if (ValidateCell(i, i + 1) == MCTSCellState.ENEMY)
                        opponentPieces += 1;
                }
                if (surroundingPieces == 4 && opponentPieces > 0)
                {
                    if (boardValues[i] / 10 == -1)
                        return GameState.WHITE_WIN;
                    else
                        return GameState.BLACK_WIN;
                }
            }
        }
        return GameState.ONGOING;
    }
    #endregion

    #region LogValues
    public void LogValues(string boardName, string fileName)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Logs"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Logs");
        }
        string path = Application.persistentDataPath + "/Logs/" + fileName + ".txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine("-------------------- " + boardName + " --------------------");
        for (int i = 0; i < boardValues.Length; i++)
        {
            writer.Write(boardValues[i] + "  ");
            if ((i + 1) % 15 == 0)
                writer.Write("\n");
        }
        writer.Write("\n");
        writer.Close();
    }
    #endregion
}
