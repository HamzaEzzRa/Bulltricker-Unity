using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;

#region Enums
public enum CellState
{
    OUT_OF_BOUNDS,
    EMPTY,
    FRIENDLY,
    ENEMY
}

public enum GameState
{
    ONGOING,
    WHITE_WIN,
    BLACK_WIN,
    DRAW
}

public enum PlayerTeam
{
    BLACK = -1,
    WHITE = 1
}

public enum CellType
{
    NORMAL_EMPTY = 0,
    BLOCKED = -1,
    KING_EMPTY = -2,
    WHITE_PAWN = 11,
    WHITE_QUEEN = 12,
    WHITE_KING = 13,
    BLACK_PAWN = -11,
    BLACK_QUEEN = -12,
    BLACK_KING = -13
}
#endregion

public class Board : MonoBehaviour
{
    #region Members
    public static Board instance;
    public float setupProgress;
    public bool isSetupDone, undoFinished = true, actionPerformed = true;
    public Transform pieceHolderTransform;
    public GameObject cellPrefab;
    public const int DEFAULT_ROW_COUNT = 15, DEFAULT_COLUMN_COUNT = 15, COLOR_DIVIDER = 10;
    public const int DEFAULT_BOARD_SIZE = DEFAULT_ROW_COUNT * DEFAULT_COLUMN_COUNT;
    public int rowCount, columnCount;
    private int boardSize;
    public PlayerTeam CurrentPlayer { get; private set; }
    public int TotalMoves { get; private set; } = 0;
    [HideInInspector] public Cell[] allCells;
    [HideInInspector] public int[] boardValues;
    public int numberOfWhitePieces, numberOfWhitePawns, numberOfBlackPieces, numberOfBlackPawns;
    public List<Tuple<int, int>> AllowedActions { get; private set; } = null;
    #endregion

    #region Awake
    private void Awake()
    {
        instance = this;
    }
    #endregion

    #region Setup
    public void Setup()
    {
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
        rowCount = DEFAULT_ROW_COUNT;
        columnCount = DEFAULT_COLUMN_COUNT;
        boardSize = rowCount * columnCount;
        CurrentPlayer = PlayerTeam.WHITE;
        allCells = new Cell[boardSize];
        CreateCells();
        LogValues("Initial Board", "BoardsLog");
        AllowedActions = GetAllowedActions();
    }

    public void Setup(int[] boardValues, int rowCount, int columnCount, PlayerTeam CurrentPlayer, int TotalMoves)
    {
        this.rowCount = rowCount;
        this.columnCount = columnCount;
        boardSize = rowCount * columnCount;
        this.boardValues = new int[boardSize];
        for (int i = 0; i < boardSize; i++)
            this.boardValues[i] = boardValues[i];
        this.CurrentPlayer = CurrentPlayer;
        this.TotalMoves = TotalMoves;
        allCells = new Cell[boardSize];
        CreateCells();
        AllowedActions = GetAllowedActions();
    }

    public void Setup(Board board)
    {
        rowCount = board.rowCount;
        columnCount = board.columnCount;
        boardSize = board.rowCount * board.columnCount;
        boardValues = new int[boardSize];
        for (int i = 0; i < boardValues.Length; i++)
            boardValues[i] = board.boardValues[i];
        CurrentPlayer = board.CurrentPlayer;
        TotalMoves = board.TotalMoves;
        allCells = new Cell[boardSize];
        CreateCells();
        AllowedActions = GetAllowedActions();
    }
    #endregion

    #region CreateCells
    public void CreateCells()
    {
        for (int i = boardSize - 1; i >= 0; i--)
        {
            int row = i / columnCount;
            int col = i % columnCount;
            GameObject newCell = Instantiate(cellPrefab, pieceHolderTransform);
            RectTransform rectTransform = newCell.GetComponent<RectTransform>();
            rectTransform.position = new Vector3Int(-MapToEdge(col), 1, MapToEdge(row));
            if (row % 2 == 0)
            {
                if (col % 2 == 0)
                {
                    rectTransform.position += new Vector3Int(0, 1, 0);
                    rectTransform.localScale = new Vector3Int(2, 2, 1);
                }
                else
                {
                    rectTransform.localScale = new Vector3Int(2, 6, 1);
                    rectTransform.Rotate(90 * Vector3.forward);
                }
            }
            else
            {
                if (col % 2 == 0)
                {
                    rectTransform.localScale = new Vector3Int(2, 6, 1);
                }
                else
                {
                    rectTransform.position += new Vector3Int(0, 1, 0);
                    rectTransform.localScale = new Vector3Int(6, 6, 1);
                }
            }
            allCells[boardSize - 1 - i] = newCell.GetComponent<Cell>();
            allCells[boardSize - 1 - i].Setup(boardSize - 1 - i);
            setupProgress = (boardSize - i) / boardSize;
        }
        StartCoroutine(FlagSetupDone());
    }

    private IEnumerator FlagSetupDone()
    {
        yield return new WaitForSeconds(0.5f);
        isSetupDone = true;
    }
    #endregion

    #region ValidateCell
    public CellState ValidateCell(int currentCell, int cellToCheck)
    {
        if (cellToCheck < 0 || cellToCheck >= boardSize)
            return CellState.OUT_OF_BOUNDS;
        if ((cellToCheck - currentCell) % columnCount != 0 && (cellToCheck < (currentCell / columnCount) * columnCount || cellToCheck >= (currentCell / columnCount + 1) * columnCount))
            return CellState.OUT_OF_BOUNDS;
        if (boardValues[cellToCheck] == (int)CellType.NORMAL_EMPTY || boardValues[cellToCheck] == (int)CellType.BLOCKED || boardValues[cellToCheck] == (int)CellType.KING_EMPTY)
            return CellState.EMPTY;
        if (boardValues[cellToCheck] / COLOR_DIVIDER == boardValues[currentCell] / COLOR_DIVIDER)
            return CellState.FRIENDLY;
        else
            return CellState.ENEMY;
    }
    #endregion

    #region GetAllowedActions
    public List<Tuple<int, int>> GetAllowedActions()
    {
        List<Tuple<int, int>> allowed = new List<Tuple<int, int>>();
        bool mandatoryPawnMove = false, mandatoryQueenMove = false;
        for (int i = 0; i < boardSize; i++)
        {
            if (boardValues[i] / COLOR_DIVIDER == (int)CurrentPlayer)
            {
                if ((boardValues[i] == (int)CellType.WHITE_PAWN || boardValues[i] == (int)CellType.BLACK_PAWN) && !mandatoryQueenMove)
                {
                    // Pawn Logic Here
                    int colorMult = boardValues[i] / COLOR_DIVIDER; // Piece Color Identification
                    if (!mandatoryPawnMove)
                    {
                        if (ValidateCell(i - columnCount * colorMult, i - (columnCount - 1) * colorMult) == CellState.EMPTY)
                            allowed.Add(new Tuple<int, int>(i, i - (columnCount - 1) * colorMult));
                        if (ValidateCell(i - columnCount * colorMult, i - (columnCount + 1) * colorMult) == CellState.EMPTY)
                            allowed.Add(new Tuple<int, int>(i, i - (columnCount + 1) * colorMult));
                        if (ValidateCell(i, i - 2 * columnCount * colorMult) == CellState.EMPTY && ValidateCell(i, i - columnCount * colorMult) == CellState.EMPTY)
                        {
                            allowed.Add(new Tuple<int, int>(i, i - 2 * columnCount * colorMult));
                            // Pawn Double Starting Step
                            if (((colorMult == (int)PlayerTeam.WHITE && i / columnCount == rowCount - 3) || (colorMult == (int)PlayerTeam.BLACK && i / columnCount == 2)) && ValidateCell(i, i - 4 * columnCount * colorMult) == CellState.EMPTY && ValidateCell(i, i - 3 * columnCount * colorMult) == CellState.EMPTY)
                                allowed.Add(new Tuple<int, int>(i, i - 4 * columnCount * colorMult));
                        }
                    }
                    if ((i / columnCount) % 2 == 0) // Non Rotated Pawn Identification
                    {
                        Tuple<int, int> possibleMove = null;
                        for (int j = 2; j < rowCount; j += 4)
                        {
                            if (ValidateCell(i, i - (j - 1) * columnCount * colorMult) == CellState.EMPTY && ValidateCell(i, i - j * columnCount * colorMult) == CellState.ENEMY && ValidateCell(i, i - (j + 1) * columnCount * colorMult) == CellState.EMPTY && ValidateCell(i, i - (j + 2) * columnCount * colorMult) == CellState.EMPTY)
                            {
                                possibleMove = new Tuple<int, int>(i, i - (j + 2) * columnCount * colorMult);
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
                else if (boardValues[i] == (int)CellType.WHITE_QUEEN || boardValues[i] == (int)CellType.BLACK_QUEEN)
                {
                    // Queen Logic Here
                    List<Tuple<int, int>> tmp = new List<Tuple<int, int>>(), tmp_up = new List<Tuple<int, int>>(), tmp_down = new List<Tuple<int, int>>();
                    List<Tuple<int, int>> tmp_left = new List<Tuple<int, int>>(), tmp_right = new List<Tuple<int, int>>();
                    bool checkingUp, checkingDown, checkingLeft, checkingRight;
                    checkingUp = checkingDown = checkingLeft = checkingRight = true;
                    bool mandatoryUp, mandatoryDown, mandatoryLeft, mandatoryRight;
                    mandatoryUp = mandatoryDown = mandatoryLeft = mandatoryRight = false;
                    if (!mandatoryQueenMove && !mandatoryPawnMove)
                    {
                        if (ValidateCell(i - columnCount, i - (columnCount - 1)) == CellState.EMPTY)
                            tmp.Add(new Tuple<int, int>(i, i - (columnCount - 1)));
                        if (ValidateCell(i - columnCount, i - (columnCount + 1)) == CellState.EMPTY)
                            tmp.Add(new Tuple<int, int>(i, i - (columnCount + 1)));
                        if (ValidateCell(i + columnCount, i + columnCount - 1) == CellState.EMPTY)
                            tmp.Add(new Tuple<int, int>(i, i + columnCount - 1));
                        if (ValidateCell(i + columnCount, i + columnCount + 1) == CellState.EMPTY)
                            tmp.Add(new Tuple<int, int>(i, i + columnCount + 1));
                    }
                    int maxCount = rowCount >= columnCount ? rowCount : columnCount;
                    for (int j = 2; j < maxCount; j += 2)
                    {
                        if (!mandatoryLeft && !mandatoryRight && j < rowCount)
                        {
                            if (checkingUp)
                            {
                                // Rotated Queen Identification
                                if ((i / columnCount) % 2 == 1)
                                {
                                    if (!mandatoryQueenMove && !mandatoryPawnMove)
                                    {
                                        if (ValidateCell(i, i - columnCount * j) == CellState.EMPTY)
                                            tmp.Add(new Tuple<int, int>(i, i - columnCount * j));
                                        else
                                            checkingUp = false;
                                    }
                                }
                                else // Non Rotated Queen Identification
                                {
                                    if (ValidateCell(i, i - (j - 1) * columnCount) == CellState.EMPTY)
                                    {
                                        if (ValidateCell(i, i - j * columnCount) == CellState.EMPTY)
                                        {
                                            if (mandatoryUp)
                                                tmp_up.Add(new Tuple<int, int>(i, i - columnCount * j));
                                            else if (!mandatoryPawnMove && !mandatoryQueenMove)
                                                tmp.Add(new Tuple<int, int>(i, i - columnCount * j));
                                        }
                                        else if (ValidateCell(i, i - j * columnCount) == CellState.ENEMY && ValidateCell(i, i - (j + 1) * columnCount) == CellState.EMPTY && ValidateCell(i, i - (j + 2) * columnCount) == CellState.EMPTY)
                                        {
                                            tmp = new List<Tuple<int, int>>();
                                            tmp_up = new List<Tuple<int, int>>();
                                            if (!mandatoryQueenMove)
                                                allowed = new List<Tuple<int, int>>();
                                            mandatoryQueenMove = mandatoryUp = true;
                                        }
                                        else
                                        {
                                            checkingUp = false;
                                            foreach (Tuple<int, int> action in tmp_up)
                                                allowed.Add(action);
                                            tmp_up = new List<Tuple<int, int>>();
                                        }
                                    }
                                    else
                                    {
                                        checkingUp = false;
                                        foreach (Tuple<int, int> action in tmp_up)
                                            allowed.Add(action);
                                        tmp_up = new List<Tuple<int, int>>();
                                    }
                                }
                            }
                            if (checkingDown)
                            {
                                // Rotated Queen Identification
                                if ((i / columnCount) % 2 == 1)
                                {
                                    if (!mandatoryQueenMove && !mandatoryPawnMove)
                                    {
                                        if (ValidateCell(i, i + columnCount * j) == CellState.EMPTY)
                                            tmp.Add(new Tuple<int, int>(i, i + columnCount * j));
                                        else
                                            checkingDown = false;
                                    }
                                }
                                else // Non Rotated Queen Identification
                                {
                                    if (ValidateCell(i, i + (j - 1) * columnCount) == CellState.EMPTY)
                                    {
                                        if (ValidateCell(i, i + j * columnCount) == CellState.EMPTY)
                                        {
                                            if (mandatoryDown)
                                                tmp_down.Add(new Tuple<int, int>(i, i + columnCount * j));
                                            else if (!mandatoryPawnMove && !mandatoryQueenMove)
                                                tmp.Add(new Tuple<int, int>(i, i + columnCount * j));
                                        }
                                        else if (ValidateCell(i, i + j * columnCount) == CellState.ENEMY && ValidateCell(i, i + (j + 1) * columnCount) == CellState.EMPTY && ValidateCell(i, i + (j + 2) * columnCount) == CellState.EMPTY)
                                        {
                                            tmp = new List<Tuple<int, int>>();
                                            tmp_down = new List<Tuple<int, int>>();
                                            if (!mandatoryQueenMove)
                                                allowed = new List<Tuple<int, int>>();
                                            mandatoryQueenMove = mandatoryDown = true;
                                        }
                                        else
                                        {
                                            checkingDown = false;
                                            foreach (Tuple<int, int> action in tmp_down)
                                                allowed.Add(action);
                                            tmp_down = new List<Tuple<int, int>>();
                                        }
                                    }
                                    else
                                    {
                                        checkingDown = false;
                                        foreach (Tuple<int, int> action in tmp_down)
                                            allowed.Add(action);
                                        tmp_down = new List<Tuple<int, int>>();
                                    }
                                }
                            }
                        }
                        if (!mandatoryUp && !mandatoryDown)
                        {
                            if (checkingLeft)
                            {
                                // Non Rotated Queen Identification
                                if ((i / columnCount) % 2 == 0)
                                {
                                    if (!mandatoryQueenMove && !mandatoryPawnMove)
                                    {
                                        if (ValidateCell(i, i - j) == CellState.EMPTY)
                                            tmp.Add(new Tuple<int, int>(i, i - j));
                                        else
                                            checkingLeft = false;
                                    }
                                }
                                else // Rotated Queen Identification
                                {
                                    if (ValidateCell(i, i - (j - 1)) == CellState.EMPTY)
                                    {
                                        if (ValidateCell(i, i - j) == CellState.EMPTY)
                                        {
                                            if (mandatoryLeft)
                                                tmp_left.Add(new Tuple<int, int>(i, i - j));
                                            else if (!mandatoryPawnMove && !mandatoryQueenMove)
                                                tmp.Add(new Tuple<int, int>(i, i - j));
                                        }
                                        else if (ValidateCell(i, i - j) == CellState.ENEMY && ValidateCell(i, i - (j + 1)) == CellState.EMPTY && ValidateCell(i, i - (j + 2)) == CellState.EMPTY)
                                        {
                                            tmp = new List<Tuple<int, int>>();
                                            tmp_left = new List<Tuple<int, int>>();
                                            if (!mandatoryQueenMove)
                                                allowed = new List<Tuple<int, int>>();
                                            mandatoryQueenMove = mandatoryLeft = true;
                                        }
                                        else
                                        {
                                            checkingLeft = false;
                                            foreach (Tuple<int, int> action in tmp_left)
                                                allowed.Add(action);
                                            tmp_left = new List<Tuple<int, int>>();
                                        }
                                    }
                                    else
                                    {
                                        checkingLeft = false;
                                        foreach (Tuple<int, int> action in tmp_left)
                                            allowed.Add(action);
                                        tmp_left = new List<Tuple<int, int>>();
                                    }
                                }
                            }
                            if (checkingRight)
                            {
                                // Non Rotated Queen Identification
                                if ((i / columnCount) % 2 == 0)
                                {
                                    if (!mandatoryQueenMove && !mandatoryPawnMove)
                                    {
                                        if (ValidateCell(i, i + j) == CellState.EMPTY)
                                            tmp.Add(new Tuple<int, int>(i, i + j));
                                        else
                                            checkingRight = false;
                                    }
                                }
                                else // Rotated Queen Identification
                                {
                                    if (ValidateCell(i, i + j - 1) == CellState.EMPTY)
                                    {
                                        if (ValidateCell(i, i + j) == CellState.EMPTY)
                                        {
                                            if (mandatoryRight)
                                                tmp_right.Add(new Tuple<int, int>(i, i + j));
                                            else if (!mandatoryPawnMove && !mandatoryQueenMove)
                                                tmp.Add(new Tuple<int, int>(i, i + j));
                                        }
                                        else if (ValidateCell(i, i + j) == CellState.ENEMY && ValidateCell(i, i + j + 1) == CellState.EMPTY && ValidateCell(i, i + j + 2) == CellState.EMPTY)
                                        {
                                            tmp = new List<Tuple<int, int>>();
                                            tmp_right = new List<Tuple<int, int>>();
                                            if (!mandatoryQueenMove)
                                                allowed = new List<Tuple<int, int>>();
                                            mandatoryQueenMove = mandatoryRight = true;
                                        }
                                        else
                                        {
                                            checkingRight = false;
                                            foreach (Tuple<int, int> action in tmp_right)
                                                allowed.Add(action);
                                            tmp_right = new List<Tuple<int, int>>();
                                        }
                                    }
                                    else
                                    {
                                        checkingRight = false;
                                        foreach (Tuple<int, int> action in tmp_right)
                                            allowed.Add(action);
                                        tmp_right = new List<Tuple<int, int>>();
                                    }
                                }
                            }
                        }
                    }
                    foreach (Tuple<int, int> action in tmp)
                        allowed.Add(action);
                    foreach (Tuple<int, int> action in tmp_up)
                        allowed.Add(action);
                    foreach (Tuple<int, int> action in tmp_down)
                        allowed.Add(action);
                    foreach (Tuple<int, int> action in tmp_left)
                        allowed.Add(action);
                    foreach (Tuple<int, int> action in tmp_right)
                        allowed.Add(action);
                }
                else if (boardValues[i] == (int)CellType.WHITE_KING || boardValues[i] == (int)CellType.BLACK_KING)
                {
                    // King Logic Here
                    if (!mandatoryPawnMove && !mandatoryQueenMove)
                    {
                        if (ValidateCell(i, i + 2 * columnCount) == CellState.EMPTY && ValidateCell(i, i + columnCount) == CellState.EMPTY && (ValidateCell(i, i + 4 * columnCount) == CellState.OUT_OF_BOUNDS || ValidateCell(i, i + 4 * columnCount) == CellState.EMPTY) && (ValidateCell(i + 4 * columnCount, i + 4 * columnCount + 2) == CellState.OUT_OF_BOUNDS || ValidateCell(i + 4 * columnCount, i + 4 * columnCount + 2) == CellState.EMPTY) && (ValidateCell(i + 4 * columnCount, i + 4 * columnCount - 2) == CellState.OUT_OF_BOUNDS || ValidateCell(i + 4 * columnCount, i + 4 * columnCount - 2) == CellState.EMPTY))
                            allowed.Add(new Tuple<int, int>(i, i + 2 * columnCount));
                        if (ValidateCell(i, i - 2 * columnCount) == CellState.EMPTY && ValidateCell(i, i - columnCount) == CellState.EMPTY && (ValidateCell(i, i - 4 * columnCount) == CellState.OUT_OF_BOUNDS || ValidateCell(i, i - 4 * columnCount) == CellState.EMPTY) && (ValidateCell(i - 4 * columnCount, i - (4 * columnCount - 2)) == CellState.OUT_OF_BOUNDS || ValidateCell(i - 4 * columnCount, i - (4 * columnCount - 2)) == CellState.EMPTY) && (ValidateCell(i - 4 * columnCount, i - (2 * columnCount + 2)) == CellState.OUT_OF_BOUNDS || ValidateCell(i - 4 * columnCount, i - (4 * columnCount + 2)) == CellState.EMPTY))
                            allowed.Add(new Tuple<int, int>(i, i - 2 * columnCount));
                        if (ValidateCell(i, i + 2) == CellState.EMPTY && ValidateCell(i, i + 1) == CellState.EMPTY && (ValidateCell(i, i + 4) == CellState.OUT_OF_BOUNDS || ValidateCell(i, i + 4) == CellState.EMPTY) && (ValidateCell(i + 2 * columnCount, i + 4 + 2 * columnCount) == CellState.OUT_OF_BOUNDS || ValidateCell(i + 2 * columnCount, i + 4 + 2 * columnCount) == CellState.EMPTY) && (ValidateCell(i - 2 * columnCount, i + 4 - 2 * columnCount) == CellState.OUT_OF_BOUNDS || ValidateCell(i - 2 * columnCount, i + 4 - 2 * columnCount) == CellState.EMPTY))
                            allowed.Add(new Tuple<int, int>(i, i + 2));
                        if (ValidateCell(i, i - 2) == CellState.EMPTY && ValidateCell(i, i - 1) == CellState.EMPTY && (ValidateCell(i, i - 4) == CellState.OUT_OF_BOUNDS || ValidateCell(i, i - 4) == CellState.EMPTY) && (ValidateCell(i + 2 * columnCount, i - 4 + 2 * columnCount) == CellState.OUT_OF_BOUNDS || ValidateCell(i + 2 * columnCount, i - 4 + 2 * columnCount) == CellState.EMPTY) && (ValidateCell(i - 2 * columnCount, i - 4 - 2 * columnCount) == CellState.OUT_OF_BOUNDS || ValidateCell(i - 2 * columnCount, i - 4 - 2 * columnCount) == CellState.EMPTY))
                            allowed.Add(new Tuple<int, int>(i, i - 2));
                    }
                }
            }
        }
        return allowed;
    }
    #endregion

    #region PerformAction
    public IEnumerator PerformAction(Tuple<int, int> action)
    {
        actionPerformed = false;
        TotalMoves++;
        int oldValue = boardValues[action.Item1];
        Color teamColor = boardValues[action.Item1] / COLOR_DIVIDER == (int)PlayerTeam.WHITE ? Color.white : Color.black;
        Piece selectedPiece = allCells[action.Item1].currentPiece;
        PieceManager.instance.undoCellStack.Push(allCells[action.Item1]);
        PieceManager.instance.undoPieceStack.Push(selectedPiece);
        // King Action
        if (boardValues[action.Item1] == (int)CellType.WHITE_KING || boardValues[action.Item1] == (int)CellType.BLACK_KING)
            boardValues[action.Item1] = (int)CellType.KING_EMPTY;
        // Pawn/Queen Action
        else
        {
            boardValues[action.Item1] = (int)CellType.NORMAL_EMPTY;
            int distance = action.Item2 - action.Item1;
            PieceType pieceType = selectedPiece.pieceType;
            int direction = distance / Math.Abs(distance);
            int step = 2; // Queen Horizontal Movement
            if (distance % columnCount == 0 || ((oldValue == (int)CellType.WHITE_QUEEN || oldValue == (int)CellType.BLACK_QUEEN) && ValidateCell(action.Item1, action.Item2) != CellState.OUT_OF_BOUNDS))
            {
                if (distance % columnCount == 0) // Pawn/Queen Vertical Movement
                    step = 2 * columnCount;
                int normalizedDistance = Math.Abs(distance);
                for (int i = step; i < normalizedDistance; i += step)
                {
                    if (boardValues[action.Item1 + i * direction] != (int)CellType.NORMAL_EMPTY)
                    {
                        PieceManager.instance.undoCellStack.Push(allCells[action.Item1 + i * direction]);
                        PieceManager.instance.undoPieceStack.Push(allCells[action.Item1 + i * direction].currentPiece);
                        boardValues[action.Item1 + i * direction] = (int)CellType.NORMAL_EMPTY;
                        selectedPiece.Move(action.Item1 + (i + step) * direction);
                        yield return allCells[action.Item1 + i * direction].currentPiece.Kill();
                        if (teamColor == Color.white)
                        {
                            numberOfBlackPieces--;
                            if (pieceType == PieceType.PAWN)
                                numberOfBlackPawns--;
                        }
                        else if (teamColor == Color.black)
                        {
                            numberOfWhitePieces--;
                            if (pieceType == PieceType.PAWN)
                                numberOfWhitePawns--;
                        }
                        yield return new WaitForSeconds(0.75f);
                    }
                }
            }
        }
        boardValues[action.Item2] = oldValue;
        selectedPiece.Move(action.Item2);
        if ((boardValues[action.Item2] == (int)CellType.WHITE_PAWN && action.Item2 / columnCount == 0) || (boardValues[action.Item2] == (int)CellType.BLACK_PAWN && action.Item2 / columnCount == rowCount - 1))
        {
            int colorMult = oldValue / COLOR_DIVIDER;
            Color32 spriteColor = colorMult == (int)PlayerTeam.WHITE ? new Color32(255, 255, 255, 220) : new Color32(140, 140, 140, 220);
            boardValues[action.Item2] = (int)CellType.WHITE_QUEEN * colorMult;
            yield return PieceManager.instance.PromotePawn(selectedPiece, allCells[action.Item2], teamColor, spriteColor);
        }
        CurrentPlayer = CurrentPlayer == PlayerTeam.WHITE ? PlayerTeam.BLACK : PlayerTeam.WHITE;
        PieceManager.instance.canReset = true;
        if (!Preserve.instance.againstAI || CurrentPlayer == PlayerTeam.WHITE)
            AllowedActions = GetAllowedActions();
        actionPerformed = true;
        LogValues("Board N° " + TotalMoves, "BoardsLog");
    }
    #endregion

    #region ReconstructValues
    public void ReconstructValues(PlayerTeam currentPlayer)
    {
        int length = allCells.Length;
        boardValues = new int[length];
        for (int i = 0; i < length; i++)
        {
            int row = i / columnCount;
            int col = i % columnCount;
            Piece piece = allCells[i].currentPiece;
            if (piece == null)
            {
                if (row % 2 == 0)
                {
                    if (col % 2 == 0)
                        boardValues[i] = -1;
                    else
                        boardValues[i] = 0;
                }
                else
                {
                    if (col % 2 == 0)
                        boardValues[i] = 0;
                    else
                        boardValues[i] = -2;
                }
            }
            else
            {
                int colorMult = piece.teamColor == Color.white ? 1 : -1;
                if (piece.pieceType == PieceType.PAWN)
                    boardValues[i] = (int)CellType.WHITE_PAWN * colorMult;
                else if (piece.pieceType == PieceType.QUEEN)
                    boardValues[i] = (int)CellType.WHITE_QUEEN * colorMult;
                else if (piece.pieceType == PieceType.KING)
                    boardValues[i] = (int)CellType.WHITE_KING * colorMult;
            }
        }
        CurrentPlayer = currentPlayer;
        AllowedActions = GetAllowedActions();
    }
    #endregion

    #region CheckState
    public GameState CheckState()
    {
        for (int i = 0; i < boardSize; i++)
        {
            if (boardValues[i] == (int)CellType.WHITE_KING || boardValues[i] == (int)CellType.BLACK_KING)
            {
                int surroundingPieces = 0;
                int opponentPieces = 0;
                if (ValidateCell(i, i - columnCount) != CellState.EMPTY)
                {
                    surroundingPieces += 1;
                    if (ValidateCell(i, i - columnCount) == CellState.ENEMY)
                        opponentPieces += 1;
                }
                if (ValidateCell(i, i + columnCount) != CellState.EMPTY)
                {
                    surroundingPieces += 1;
                    if (ValidateCell(i, i + columnCount) == CellState.ENEMY)
                        opponentPieces += 1;
                }
                if (ValidateCell(i, i - 1) != CellState.EMPTY)
                {
                    surroundingPieces += 1;
                    if (ValidateCell(i, i - 1) == CellState.ENEMY)
                        opponentPieces += 1;
                }
                if (ValidateCell(i, i + 1) != CellState.EMPTY)
                {
                    surroundingPieces += 1;
                    if (ValidateCell(i, i + 1) == CellState.ENEMY)
                        opponentPieces += 1;
                }
                if (surroundingPieces == 4 && opponentPieces > 0)
                {
                    PieceManager.instance.isGameOver = true;
                    if (boardValues[i] / COLOR_DIVIDER == (int)PlayerTeam.BLACK)
                    {
                        PieceManager.instance.isKingAlive[1] = false;
                        return GameState.WHITE_WIN;
                    }
                    else if (boardValues[i] / COLOR_DIVIDER == (int)PlayerTeam.WHITE)
                    {
                        PieceManager.instance.isKingAlive[0] = false;
                        return GameState.BLACK_WIN;
                    }
                }
            }
        }
        if (AllowedActions.Count == 0 || (numberOfWhitePieces < 5 && numberOfBlackPieces < 5))
        {
            PieceManager.instance.isGameOver = true;
            return GameState.DRAW;
        }
        return GameState.ONGOING;
    }
    #endregion

    #region MapToEdge
    private int MapToEdge(int x)
    {
        return x * 4 - 28;
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

    #region WaitToUndo
    public IEnumerator WaitToUndo()
    {
        yield return new WaitUntil(() => undoFinished && actionPerformed);
        undoFinished = false;
        if (PieceManager.instance.isKingAlive[0] && PieceManager.instance.isKingAlive[1])
        {
            if (PieceManager.instance.isGameOver)
            {
                PieceManager.instance.isGameOver = false;
                PieceManager.instance.isKingAlive[0] = PieceManager.instance.isKingAlive[1] = true;
            }
            if (PieceManager.instance.selectedPiece != null)
            {
                PieceManager.instance.selectedPiece.ClearCells();
                PieceManager.instance.selectedPiece.transform.position -= 2 * Vector3.up;
                PieceManager.instance.selectedPiece = null;
            }
            if (PieceManager.instance.undoPieceStack.Count > 0 && PieceManager.instance.undoCellStack.Count > 0)
            {
                Piece randomOtherPiece = null;
                Color previousColor = PieceManager.instance.isBlackTurn ? Color.white : Color.black;
                for (int i = PieceManager.instance.undoPieceStack.Count - 1; i >= 0; i--)
                {
                    Piece actualPiece = PieceManager.instance.undoPieceStack.Pop();
                    if (actualPiece.teamColor != previousColor)
                    {
                        randomOtherPiece = actualPiece;
                        Cell actualCell = PieceManager.instance.undoCellStack.Pop();
                        PieceManager.instance.Unkill(actualCell, actualPiece);
                        if (previousColor == Color.black)
                        {
                            numberOfWhitePieces++;
                            if (actualPiece.pieceType == PieceType.PAWN)
                            {
                                numberOfWhitePawns++;
                            }
                        }
                        else
                        {
                            numberOfBlackPieces++;
                            if (actualPiece.pieceType == PieceType.PAWN)
                            {
                                numberOfBlackPawns++;
                            }
                        }
                    }
                    else
                    {
                        if (!actualPiece.isActiveAndEnabled && PieceManager.instance.promotedPawns.Count > 0)
                        {
                            Piece destroyable = PieceManager.instance.promotedPawns[PieceManager.instance.promotedPawns.Count - 1];
                            destroyable.Kill();
                            Destroy(destroyable.gameObject);
                            if (actualPiece.teamColor == Color.white)
                            {
                                numberOfWhitePawns++;
                            }
                            else
                            {
                                numberOfBlackPawns++;
                            }
                        }
                        Cell actualCell = PieceManager.instance.undoCellStack.Pop();
                        PieceManager.instance.Unkill(actualCell, actualPiece);
                        if (previousColor == Color.white)
                        {
                            PieceManager.instance.real = false;
                            yield return PieceManager.instance.SwitchSides(Color.black);
                        }
                        else
                        {
                            PieceManager.instance.real = false;
                            yield return PieceManager.instance.SwitchSides(Color.white);
                        }
                        break;
                    }
                }
                TotalMoves--;
                ReconstructValues(CurrentPlayer == PlayerTeam.WHITE ? PlayerTeam.BLACK : PlayerTeam.WHITE);
            }
        }
        undoFinished = true;
    }
    #endregion
}
