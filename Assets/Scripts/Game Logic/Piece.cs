using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class Piece : MonoBehaviourPun
{
    #region Members
    [HideInInspector] public Color teamColor = Color.clear;
    [HideInInspector] public Color32 spriteColor;
    [HideInInspector] public Cell originalCell = null;
    [HideInInspector] public PieceType pieceType;
    [HideInInspector] public Cell currentCell = null;
    public bool promoted = false;
    [HideInInspector] public Cell targetCell = null;
    [HideInInspector] public List<Cell> highlightedCells = new List<Cell>();
    public bool isRotated = false, isActivated = true, hasMoved = false;
    [HideInInspector] public float numberOfSkips;
    public static bool moveFinished = true, easeFinished = true;
    #endregion

    #region Setup
    public virtual void Setup(PieceType pieceType, Color teamColor, Color32 spriteColor)
    {
        this.teamColor = teamColor;
        this.spriteColor = spriteColor;
        gameObject.GetComponent<MeshRenderer>().material.color = spriteColor;
        if (pieceType == PieceType.QUEEN)
        {
            if (teamColor == Color.white)
            {
                PieceManager.instance.whiteQueens.Add(this);
                PieceManager.instance.whitePieces.Add(this);
            }
            else
            {
                PieceManager.instance.blackQueens.Add(this);
                PieceManager.instance.blackPieces.Add(this);
            }
        }
        else if (pieceType == PieceType.PAWN)
        {
            if (teamColor == Color.white)
            {
                PieceManager.instance.whitePawns.Add(this);
                PieceManager.instance.whitePieces.Add(this);
            }
            else if (teamColor == Color.black)
            {
                PieceManager.instance.blackPawns.Add(this);
                PieceManager.instance.blackPieces.Add(this);
            }
        }
        else if (pieceType == PieceType.KING)
        {
            if (teamColor == Color.white)
                PieceManager.instance.whitePieces.Add(this);
            else if (teamColor == Color.black)
                PieceManager.instance.blackPieces.Add(this);
            PieceManager.instance.kingPieces.Add(this);
        }
        PieceManager.instance.allPieces.Add(this);
    }
    #endregion

    #region Place
    public void Place(Cell cell)
    {
        currentCell = cell;
        originalCell = cell;
        currentCell.currentPiece = this;
        transform.position = cell.transform.position + (pieceType == PieceType.KING ? 2.5f * Vector3.up : 2 * Vector3.up);
        transform.rotation = cell.transform.rotation;
        gameObject.SetActive(true);
    }
    #endregion

    #region CheckPathing
    public virtual bool CheckPathing()
    {
        bool found = false;
        foreach (Tuple<int, int> action in Board.instance.AllowedActions)
        {
            if (action.Item1 == currentCell.boardPos)
                highlightedCells.Add(Board.instance.allCells[action.Item2]);
        }
        return found;
    }
    #endregion

    #region ShowCells
    protected void ShowCells()
    {
        foreach (Cell cell in highlightedCells)
        {
            cell.GetComponent<MeshRenderer>().enabled = true;
        }
    }
    #endregion

    #region ClearCells
    public void ClearCells()
    {
        foreach (Cell cell in highlightedCells)
        {
            cell.GetComponent<MeshRenderer>().enabled = false;
        }
        highlightedCells.Clear();
    }
    #endregion

    #region SendCellPosition
    private void SendCellPosition(string action, string suffix)
    {
        GameObject clientObject = GameObject.Find("Client(Clone)");
        if (clientObject != null)
        {
            Client client = clientObject.GetComponent<Client>();
            client.Send(action + "|" + PieceManager.instance.selectedPiece.currentCell.boardPos + "|" + suffix);
        }
    }
    #endregion

    #region OnMouseDown
    public void OnMouseDown()
    {
        if ((RoomListing.IsOnline && !photonView.IsMine) || !Board.instance.actionPerformed || !PieceManager.instance.promotionFinished)
        {
            return;
        }
        if (easeFinished)
        {
            easeFinished = false;
            if (PieceManager.instance.selectedPiece != this)
            {
                if (PieceManager.instance.selectedPiece != null)
                {
                    Transform transform = PieceManager.instance.selectedPiece.transform;
                    transform.LeanMove(transform.position - 2 * Vector3.up, Time.deltaTime * 10f).setEaseInQuad()
                        .setOnComplete(() => easeFinished = true);
                    if (Preserve.instance.isLocalOnline)
                    {
                        if (Client.isHost)
                        {
                            SendCellPosition("Move", "HPD");
                        }
                        else
                        {
                            SendCellPosition("Move", "NHPD");
                        }
                    }
                    else if (RoomListing.IsOnline)
                    {
                        int cellPosition = PieceManager.instance.selectedPiece.currentCell.boardPos;
                        object[] data = new object[]
                        {
                            cellPosition
                        };
                        PhotonNetwork.RaiseEvent(EventCodes.PIECE_DOWN_EASED, data, RaiseEventOptions.Default, SendOptions.SendReliable);
                    }
                    PieceManager.instance.selectedPiece.ClearCells();
                }
                PieceManager.instance.selectedPiece = this;
                transform.LeanMove(transform.position + 2 * Vector3.up, Time.deltaTime * 10f).setEaseOutQuad()
                    .setOnComplete(() => easeFinished = true);
                if (Preserve.instance.isLocalOnline)
                {
                    if (Client.isHost)
                    {
                        SendCellPosition("Move", "HPU");
                    }
                    else
                    {
                        SendCellPosition("Move", "NHPU");
                    }
                }
                else if (RoomListing.IsOnline)
                {
                    int cellPosition = currentCell.boardPos;
                    object[] data = new object[]
                    {
                        cellPosition
                    };
                    PhotonNetwork.RaiseEvent(EventCodes.PIECE_UP_EASED, data, RaiseEventOptions.Default, SendOptions.SendReliable);
                }
                PieceManager.instance.selectedPiece.ClearCells();
                CheckPathing();
                ShowCells();
            }
            else
            {
                transform.LeanMove(transform.position - 2 * Vector3.up, Time.deltaTime * 10f).setEaseInQuad()
                    .setOnComplete(() => easeFinished = true);
                if (Preserve.instance.isLocalOnline)
                {
                    if (Client.isHost)
                    {
                        SendCellPosition("Move", "HPD");
                    }
                    else
                    {
                        SendCellPosition("Move", "NHPD");
                    }
                }
                else if (RoomListing.IsOnline)
                {
                    int cellPosition = currentCell.boardPos;
                    object[] data = new object[]
                    {
                        cellPosition
                    };
                    PhotonNetwork.RaiseEvent(EventCodes.PIECE_DOWN_EASED, data, RaiseEventOptions.Default, SendOptions.SendReliable);
                }
                PieceManager.instance.selectedPiece.ClearCells();
                ClearCells();
                PieceManager.instance.selectedPiece = null;
            }
        }
    }
    #endregion

    #region Kill
    public IEnumerator Kill()
    {
        currentCell.currentPiece = null;
        PieceManager.instance.allPieces.Remove(this);
        PieceManager.instance.whiteQueens.Remove(this);
        PieceManager.instance.blackQueens.Remove(this);
        PieceManager.instance.promotedPawns.Remove(this);
        PieceManager.instance.whitePawns.Remove(this);
        PieceManager.instance.blackPawns.Remove(this);
        GameObject crackedObject = Instantiate(pieceType == PieceType.PAWN ? PieceManager.instance.cutPawnPrefab : PieceManager.instance.cutQueenPrefab);
        crackedObject.transform.position = transform.position;
        crackedObject.transform.position -= 1.04f * Vector3.up;
        int col = currentCell.boardPos / 15;
        if (col % 2 == 0)
        {
            crackedObject.transform.rotation = Quaternion.Euler(90 * Vector3.up);
        }
        Material rightPartMaterial = crackedObject.GetComponentsInChildren<MeshRenderer>()[0].material, leftPartMaterial = crackedObject.GetComponentsInChildren<MeshRenderer>()[1].material;
        Color32 actualColor = spriteColor;
        actualColor.a = 255;
        rightPartMaterial.color = leftPartMaterial.color = actualColor;
        gameObject.SetActive(false);
        Transform rightPartTransform = crackedObject.GetComponentsInChildren<Transform>()[2], leftPartTransform = crackedObject.GetComponentsInChildren<Transform>()[1];
        for (int i = 0; i < 8; i++)
        {
            rightPartTransform.position += -0.1f * Vector3.forward;
            leftPartTransform.position += 0.1f * Vector3.forward;
            Color32 newColor = rightPartMaterial.color;
            newColor.a -= 15;
            rightPartMaterial.color = leftPartMaterial.color = newColor;
            yield return new WaitForSecondsRealtime(0.04f);
        }
        Destroy(crackedObject);
    }
    #endregion

    #region Reset
    public void Reset()
    {
        StartCoroutine(Kill());
        ClearCells();
        Place(originalCell);
        isRotated = false;
    }
    #endregion

    #region Move
    public void Move(int cellIndex)
    {
        currentCell.currentPiece = null;
        currentCell = Board.instance.allCells[cellIndex];
        currentCell.currentPiece = this;
        if (easeFinished)
        {
            easeFinished = false;
            gameObject.LeanRotate(currentCell.transform.rotation.eulerAngles, 10f * Time.deltaTime).setEaseInQuad();
            transform.LeanMove(currentCell.transform.position + (pieceType == PieceType.KING ? 2.5f * Vector3.up : 2 * Vector3.up), 10f * Time.deltaTime).setEaseInQuad()
                .setOnComplete(() => easeFinished = true);
        }
    }
    #endregion
}
