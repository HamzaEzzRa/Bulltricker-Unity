using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum Team
{
    WHITE,
    BLACK,
    NUMBER_OF_TEAMS
}

public class PieceManager : MonoBehaviourPun
{
    #region Members
    public static PieceManager instance;
    public float setupProgress;
    public bool isSetupDone;
    public GameObject pawnPrefab, queenPrefab, kingPrefab, cutPawnPrefab, cutQueenPrefab;
    public Transform cameraRigTransform = null;
    public CameraController cameraController = null;
    [HideInInspector] public List<Piece> allPieces = null;
    [HideInInspector] public List<Piece> whitePieces = null;
    [HideInInspector] public List<Piece> blackPieces = null;
    [HideInInspector] public List<Piece> whiteQueens = null, blackQueens = null;
    [HideInInspector] public List<Piece> whitePawns = new List<Piece>(), blackPawns = new List<Piece>();
    [HideInInspector] public List<Piece> promotedPawns = new List<Piece>();
    [HideInInspector] public List<Piece> kingPieces = new List<Piece>();
    [HideInInspector] public Piece selectedPiece = null;
    [HideInInspector] public bool[] isKingAlive;
    [HideInInspector] public Stack<Piece> undoPieceStack = new Stack<Piece>();
    [HideInInspector] public Stack<Cell> undoCellStack = new Stack<Cell>();
    [HideInInspector] public UI UIElement;
    [HideInInspector] public bool cameraRotating = false, isBlackTurn = false, isGameOver = false;
    [HideInInspector] public bool switchFinished = true, canReset = false, real = true, promotionFinished = true;
    private Dictionary<int, Photon.Realtime.Player> playersLibrary = new Dictionary<int, Photon.Realtime.Player>();
    private MCTS mcts = null;
    private MCTSBoard currentBoard = null;
    private const int AILevel = 3;
    private string savePath;

    private readonly Dictionary<int, PieceType> typeValue = new Dictionary<int, PieceType> {
        { 11, PieceType.PAWN },
        { 12, PieceType.QUEEN },
        { 13, PieceType.KING }
    };
    #endregion

    #region Awake
    private void Awake()
    {
        instance = this;
        if (RoomListing.IsOnline)
            gameObject.AddComponent<AudioListener>();
    }
    #endregion

    #region OnApplicationPause
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            if (!RoomListing.IsOnline && !Preserve.instance.isLocalOnline && isSetupDone && !isGameOver)
            {
                SerializationManager.Save(SaveData.Instance);
            }
        }
    }
    #endregion

    #region UpdateSave
    public void UpdateSave()
    {
        if (RoomListing.IsOnline || Preserve.instance.isLocalOnline || isGameOver)
        {
            return;
        }
        for (int i = 0; i < 24; i++)
        {
            PieceData newWhiteData, newBlackData;
            if (i < whitePieces.Count && whitePieces[i] != null && whitePieces[i].isActiveAndEnabled)
            {
                newWhiteData = new PieceData
                {
                    color = PieceColor.WHITE,
                    type = whitePieces[i].pieceType,
                    cellPosition = whitePieces[i].currentCell.boardPos
                };
            }
            else
            {
                newWhiteData = new PieceData
                {
                    type = PieceType.EMPTY
                };
            }
            SaveData.Instance.gameData.pieceData[i] = newWhiteData;
            if (i < blackPieces.Count && blackPieces[i] != null && blackPieces[i].isActiveAndEnabled)
            {
                newBlackData = new PieceData
                {
                    color = PieceColor.BLACK,
                    type = blackPieces[i].pieceType,
                    cellPosition = blackPieces[i].currentCell.boardPos
                };
            }
            else
            {
                newBlackData = new PieceData
                {
                    type = PieceType.EMPTY
                };
            }
            SaveData.Instance.gameData.pieceData[i + 24] = newBlackData;
        }
        SaveData.Instance.gameData.numberOfWhitePieces = Board.instance.numberOfWhitePieces; SaveData.Instance.gameData.numberOfWhitePawns = Board.instance.numberOfWhitePawns;
        SaveData.Instance.gameData.numberOfBlackPieces = Board.instance.numberOfBlackPieces; SaveData.Instance.gameData.numberOfBlackPawns = Board.instance.numberOfBlackPawns;
        SaveData.Instance.gameData.isBlackTurn = isBlackTurn;
    }
    #endregion

    #region Setup
    public void Setup()
    {
        if (RoomListing.IsOnline)
        {
            playersLibrary = PhotonNetwork.CurrentRoom.Players;
        }
        cameraRigTransform = GameObject.Find("Camera Rig").GetComponent<Transform>();
        cameraController = cameraRigTransform.gameObject.GetComponent<CameraController>();
        UIElement = GameObject.Find("UI").GetComponent<UI>();
        UIElement.Start();
        isKingAlive = new bool[(int)Team.NUMBER_OF_TEAMS];
        isKingAlive[(int)Team.WHITE] = true;
        isKingAlive[(int)Team.BLACK] = true;
        savePath = Application.persistentDataPath + "/Saves/" + (Preserve.instance.againstAI ? "AI" : "QP") + ".save";
        if (File.Exists(savePath) && !RoomListing.IsOnline && !Preserve.instance.isLocalOnline)
        {
            SaveData.Instance = (SaveData)SerializationManager.Load(savePath);
            for (int i = 0; i < 48; i++)
            {
                PieceData newData = SaveData.Instance.gameData.pieceData[i];
                Piece newPiece = null;
                if (newData != null && newData.type != PieceType.EMPTY)
                {
                    PieceType pieceType = newData.type;
                    newPiece = CreatePiece(pieceType);
                }
                if (newPiece != null)
                {
                    Color teamColor = newData.color == PieceColor.WHITE ? Color.white : Color.black;
                    Color32 spriteColor = newData.color == PieceColor.WHITE ? new Color32(255, 255, 255, 220) : new Color32(140, 140, 140, 220);
                    newPiece.Setup(newPiece.pieceType, teamColor, spriteColor);
                    newPiece.Place(Board.instance.allCells[newData.cellPosition]);
                }
                setupProgress = i / 48f;
            }
            PlayerTeam currentPlayer = !SaveData.Instance.gameData.isBlackTurn ? PlayerTeam.WHITE : PlayerTeam.BLACK;
            Board.instance.ReconstructValues(currentPlayer);
            Color colorToSwitch = SaveData.Instance.gameData.isBlackTurn ? Color.white : Color.black;
            if (colorToSwitch == Color.white)
            {
                cameraRigTransform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                cameraController.originalRotation = cameraRigTransform.rotation.eulerAngles;
            }
            Board.instance.numberOfWhitePieces = SaveData.Instance.gameData.numberOfWhitePieces; Board.instance.numberOfWhitePawns = SaveData.Instance.gameData.numberOfWhitePawns;
            Board.instance.numberOfBlackPieces = SaveData.Instance.gameData.numberOfBlackPieces; Board.instance.numberOfBlackPawns = SaveData.Instance.gameData.numberOfBlackPawns;
            Board.instance.LogValues("Loaded Board", "SavesLog");
            StartCoroutine(SwitchSides(colorToSwitch));
        }
        else
        {
            CreatePieces();
            Board.instance.numberOfWhitePieces = Board.instance.numberOfBlackPieces = 24;
            Board.instance.numberOfWhitePawns = Board.instance.numberOfBlackPawns = 15;
            StartCoroutine(SwitchSides(Color.black));
            if ((Preserve.instance.isLocalOnline && !Client.isHost) || (RoomListing.IsOnline && !PhotonNetwork.IsMasterClient))
            {
                cameraRigTransform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                cameraController.originalRotation = cameraRigTransform.rotation.eulerAngles;
            }
        }
        StartCoroutine(FlagSetupDone());
        Debug.Log("White Pieces Count: " + whitePieces.Count);
        Debug.Log("Black Pieces Count: " + blackPieces.Count);
    }

    private IEnumerator FlagSetupDone()
    {
        yield return new WaitForSeconds(1f);
        isSetupDone = true;
    }
    #endregion

    #region CreatePieces
    private void CreatePieces()
    {
        float spawnProgress;
        int[] boardValues = Board.instance.boardValues;
        int k = 0;
        for (int i = 0; i < boardValues.Length; i++)
        {
            if (boardValues[i] != 0 && boardValues[i] != -2 && boardValues[i] != -1)
            {
                PieceType pieceType = typeValue[Math.Abs(boardValues[i])];
                Piece newPiece = CreatePiece(pieceType);
                Color teamColor = boardValues[i] / 10 == 1 ? Color.white : Color.black;
                Color32 spriteColor = teamColor == Color.white ? new Color32(255, 255, 255, 220) : new Color32(140, 140, 140, 220);
                newPiece.Setup(pieceType, teamColor, spriteColor);
                if (RoomListing.IsOnline)
                {
                    PhotonView view = newPiece.gameObject.AddComponent<PhotonView>();
                    view.ViewID = k + 1;
                    view.Synchronization = ViewSynchronization.ReliableDeltaCompressed;
                    if (teamColor == Color.white)
                        view.TransferOwnership(playersLibrary[1]);
                    else
                        view.TransferOwnership(playersLibrary[2]);
                }
                newPiece.Place(Board.instance.allCells[i]);
                spawnProgress = i + 1 / boardValues.Length;
                if (teamColor == Color.white)
                    spawnProgress += 0.5f;
                setupProgress = spawnProgress;
                k++;
            }
        }
    }
    #endregion

    #region SetInteractive
    public void SetInteractive(List<Piece> pieceList, bool value)
    {
        foreach (Piece piece in pieceList)
        {
            if (piece != null)
            {
                piece.GetComponent<Collider>().enabled = value;
            }
        }
    }
    #endregion

    #region SwitchSides
    public IEnumerator SwitchSides(Color color)
    {
        yield return new WaitUntil(() => Piece.moveFinished && promotionFinished && switchFinished && Board.instance.actionPerformed);
        switchFinished = false;
        Board.instance.CheckState();
        if (isGameOver)
        {
            SetInteractive(whitePieces, false);
            SetInteractive(blackPieces, false);
            if (File.Exists(savePath))
                File.Delete(savePath);
        }
        else
        {
            isBlackTurn = color == Color.white ? true : false;
            SetInteractive(whitePieces, !isBlackTurn);
            if (!Preserve.instance.againstAI)
            {
                SetInteractive(blackPieces, isBlackTurn);
                if (Preserve.instance.isCameraRotatable && !Preserve.instance.isLocalOnline && !RoomListing.IsOnline && !isGameOver)
                {
                    yield return new WaitForSecondsRealtime(0.3f);
                    yield return new WaitUntil(() => !cameraRotating && promotionFinished);
                    if (canReset)
                    {
                        cameraRotating = true;
                        for (int i = 0; i < 45; i++)
                        {
                            yield return new WaitForSecondsRealtime(0.001f);
                            cameraRigTransform.RotateAround(Board.instance.transform.position - new Vector3(31, 0, 0), Vector3.up, 4.0f);
                        }
                        cameraController.originalRotation = cameraRigTransform.rotation.eulerAngles;
                        cameraRotating = false;
                    }
                }
                else if (Preserve.instance.isLocalOnline)
                {
                    if (Client.isHost)
                    {
                        SetInteractive(blackPieces, false);
                    }
                    else
                    {
                        SetInteractive(whitePieces, false);
                    }
                }
                else if (RoomListing.IsOnline)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        SetInteractive(blackPieces, false);
                    }
                    else
                    {
                        SetInteractive(whitePieces, false);
                    }
                }
            }
            else
            {
                SetInteractive(blackPieces, isBlackTurn);
                if (isBlackTurn && canReset && !Preserve.instance.isLocalOnline && !RoomListing.IsOnline)
                {
                    StartCoroutine(MoveAI());
                }
            }
        }
        UpdateSave();
        switchFinished = true;
    }
    #endregion

    #region Unkill
    public void Unkill(Cell cell, Piece piece)
    {
        piece.currentCell.currentPiece = null;
        cell.currentPiece = piece;
        piece.currentCell = cell;
        piece.transform.position = cell.transform.position;
        piece.transform.rotation = cell.transform.rotation;
        int row = cell.boardPos / 15;
        int col = cell.boardPos % 15;
        if (row % 2 == 0)
        {
            if (col % 2 != 0)
            {
                piece.transform.position += 2 * Vector3.up;
            }
        }
        else
        {
            if (col % 2 == 0)
            {
                piece.transform.position += 2 * Vector3.up;
            }
            else
            {
                piece.transform.position += 4 * Vector3.up;
            }
        }
        if (!piece.isActiveAndEnabled)
        {
            if (piece.pieceType == PieceType.PAWN)
            {
                if (piece.teamColor == Color.white)
                {
                    whitePawns.Add(piece);
                }
                else
                {
                    blackPawns.Add(piece);
                }
            }
            else if (piece.pieceType == PieceType.QUEEN)
            {
                if (piece.promoted)
                {
                    promotedPawns.Add(piece);
                }
                if (piece.teamColor == Color.white)
                {
                    whiteQueens.Add(piece);
                }
                else
                {
                    blackQueens.Add(piece);
                }
            }
            if (piece.teamColor == Color.white)
            {
                whitePieces.Add(piece);
            }
            else
            {
                blackPieces.Add(piece);
            }
            allPieces.Add(piece);
        }
        piece.gameObject.SetActive(true);
    }
    #endregion

    #region MoveAI
    public IEnumerator MoveAI()
    {
        yield return new WaitUntil(() => Piece.moveFinished && switchFinished);
        if (!isGameOver)
        {
            if (mcts == null)
                mcts = new MCTS(5);
            if (currentBoard == null)
                currentBoard = new MCTSBoard(Board.instance.boardValues, Board.instance.TotalMoves, 1);
            currentBoard.boardValues = Board.instance.boardValues;
            MCTSBoard nextBoard = mcts.FindNextMove(currentBoard, 1);
            int startingCellIndex = 0, targetCellIndex = 0;
            currentBoard.LogValues("Current Board", "MCTSBoardsLog");
            nextBoard.LogValues("Next Board", "MCTSBoardsLog");
            for (int i = 0; i < nextBoard.boardValues.Length; i++)
            {
                if (nextBoard.boardValues[i] != currentBoard.boardValues[i])
                {
                    if ((nextBoard.boardValues[i] == 0 || nextBoard.boardValues[i] == -2) && currentBoard.boardValues[i] / 10 == -1)
                        startingCellIndex = i;
                    if ((currentBoard.boardValues[i] == 0 || currentBoard.boardValues[i] == -2) && nextBoard.boardValues[i] / 10 == -1)
                        targetCellIndex = i;
                }
            }
            Debug.Log("Starting Cell: " + startingCellIndex);
            Debug.Log("Target Cell: " + targetCellIndex);
            Tuple<int, int> action = new Tuple<int, int>(startingCellIndex, targetCellIndex);
            yield return Board.instance.PerformAction(action);
            yield return SwitchSides(Color.black);
        }
    }
    #endregion

    #region CreatePiece
    private Piece CreatePiece(PieceType pieceType)
    {
        GameObject newPieceObject = null;
        if (pieceType == PieceType.PAWN)
        {
            newPieceObject = Instantiate(pawnPrefab, Board.instance.pieceHolderTransform);
        }
        else if (pieceType == PieceType.QUEEN)
        {
            newPieceObject = Instantiate(queenPrefab, Board.instance.pieceHolderTransform);
        }
        else if (pieceType == PieceType.KING)
        {
            newPieceObject = Instantiate(kingPrefab, Board.instance.pieceHolderTransform);
        }
        if (newPieceObject != null)
        {
            Piece newPiece = newPieceObject.AddComponent<Piece>();
            newPiece.pieceType = pieceType;
            return newPiece;
        }
        return null;
    }
    #endregion
    
    #region PromotePawn
    public IEnumerator PromotePawn(Piece pawn, Cell cell, Color teamColor, Color32 spriteColor)
    {
        yield return new WaitUntil(() => promotionFinished);
        promotionFinished = false;
        pawn.Kill();
        pawn.gameObject.SetActive(false);
        if (teamColor == Color.white)
        {
            Board.instance.numberOfWhitePawns--;
        }
        else
        {
            Board.instance.numberOfBlackPawns--;
        }
        Piece promotedPawn = CreatePiece(PieceType.QUEEN);
        if (RoomListing.IsOnline)
        {
            PhotonView view = promotedPawn.gameObject.AddComponent<PhotonView>();
            view.ViewID = 49 + promotedPawns.Count;
            PhotonTransformView transformView = promotedPawn.gameObject.AddComponent<PhotonTransformView>();
            view.ObservedComponents = new List<Component>
            {
                transformView
            };
            view.Synchronization = ViewSynchronization.ReliableDeltaCompressed;
            if (teamColor == Color.white)
            {
                view.TransferOwnership(playersLibrary[1]);
            }
            else
            {
                view.TransferOwnership(playersLibrary[2]);
            }
        }
        promotedPawn.Setup(PieceType.QUEEN, teamColor, spriteColor);
        promotedPawn.promoted = true;
        promotedPawn.Place(cell);
        promotedPawns.Add(promotedPawn);
        promotedPawn.GetComponent<Collider>().enabled = false;
        promotionFinished = true;
    }
    #endregion

    #region ResetPieces
    public void ResetPieces()
    {
        foreach (Piece piece in whitePieces)
        {
            piece.Kill();
        }
        foreach (Piece piece in blackPieces)
        {
            piece.Kill();
        }
        promotedPawns.Clear();
        whitePieces.Clear();
        blackPieces.Clear();
        whiteQueens.Clear();
        blackQueens.Clear();
        whitePawns.Clear();
        blackPawns.Clear();
        Setup();
        canReset = false;
        Debug.Log("Successful Reset\n");
    }
    #endregion

    #region OnEnable
    private void OnEnable()
    {
        if (RoomListing.IsOnline)
        {
            PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
        }
    }
    #endregion

    #region OnDestroy
    private void OnDestroy()
    {
        try
        {
            PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
            if (!RoomListing.IsOnline && !Preserve.instance.isLocalOnline && isSetupDone && !isGameOver)
            {
                SerializationManager.Save(SaveData.Instance);
            }
        }
        catch (Exception exception)
        {
            Debug.Log(exception);
        }
    }
    #endregion

    #region ServerEventListener
    private void NetworkingClient_EventReceived(ExitGames.Client.Photon.EventData obj)
    {
        if (RoomListing.IsOnline)
        {
            if (obj.Code == EventCodes.PIECE_MOVE)
            {
                object[] data = (object[])obj.CustomData;
                int currentPos = (int)data[0];
                int nextPos = (int)data[1];
                Tuple<int, int> action = new Tuple<int, int>(currentPos, nextPos);
                StartCoroutine(PerformActionAndSwitch(action));
            }
            else if (obj.Code == EventCodes.PIECE_UP)
            {
                object[] data = (object[])obj.CustomData;
                int currentPos = (int)data[0];
                Board.instance.allCells[currentPos].currentPiece.transform.position += 2 * Vector3.up;
            }
            else if (obj.Code == EventCodes.PIECE_DOWN)
            {
                object[] data = (object[])obj.CustomData;
                int currentPos = (int)data[0];
                Board.instance.allCells[currentPos].currentPiece.transform.position -= 2 * Vector3.up;
            }
            else if (obj.Code == EventCodes.PIECE_UP_EASED)
            {
                object[] data = (object[])obj.CustomData;
                int currentPos = (int)data[0];
                Transform transform = Board.instance.allCells[currentPos].currentPiece.transform;
                transform.LeanMove(transform.position + 2 * Vector3.up, Time.deltaTime * 10f).setEaseOutQuad();
            }
            else if (obj.Code == EventCodes.PIECE_DOWN_EASED)
            {
                object[] data = (object[])obj.CustomData;
                int currentPos = (int)data[0];
                Transform transform = Board.instance.allCells[currentPos].currentPiece.transform;
                transform.LeanMove(transform.position - 2 * Vector3.up, Time.deltaTime * 10f).setEaseInQuad();
            }
        }
    }

    private IEnumerator PerformActionAndSwitch(Tuple<int, int> action)
    {
        Color colorToSwitch = Board.instance.boardValues[action.Item1] / 10 == 1 ? Color.white : Color.black;
        yield return Board.instance.PerformAction(action);
        yield return SwitchSides(colorToSwitch);
    }
    #endregion
}
