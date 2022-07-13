using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class Cell : MonoBehaviourPun
{
    #region Members
    [HideInInspector] public int boardPos = 0;
    [HideInInspector] public RectTransform rectTransform = null;
    public static bool mouseDownFinished = true, easeFinished = true;
    public Piece currentPiece = null;
    #endregion

    #region Setup
    public void Setup(int boardPos)
    {
        this.boardPos = boardPos;
        rectTransform = GetComponent<RectTransform>();
    }
    #endregion

    #region SendPiecePosition
    private void SendPiecePosition(string action, string suffix)
    {
        GameObject clientObject = GameObject.Find("Client(Clone)");
        if (clientObject != null)
        {
            Client client = clientObject.GetComponent<Client>();
            client.Send(action + "|" + PieceManager.instance.selectedPiece.currentCell.boardPos + "|" + suffix);
        }
    }
    #endregion

    #region SendCellPosition
    private void SendCellPosition(string action, string suffix)
    {
        GameObject clientObject = GameObject.Find("Client(Clone)");
        if (clientObject != null)
        {
            Client client = clientObject.GetComponent<Client>();
            client.Send(action + "|" + boardPos + "|" + suffix);
        }
    }
    #endregion

    #region OnMouseDown
    public IEnumerator OnMouseDown()
    {
        yield return new WaitUntil(() => mouseDownFinished);
        mouseDownFinished = false;
        if(PieceManager.instance.selectedPiece != null)
        {
            int oldPos = PieceManager.instance.selectedPiece.currentCell.boardPos;
            bool contained = PieceManager.instance.selectedPiece.highlightedCells.Contains(this);
            if (!contained)
            {
                if (easeFinished)
                {
                    easeFinished = false;
                    Transform transform = PieceManager.instance.selectedPiece.transform;
                    transform.LeanMove(transform.position - 2 * Vector3.up, 10f * Time.deltaTime).setEaseInQuad()
                        .setOnComplete(() => easeFinished = true);
                }
                if (Preserve.instance.isLocalOnline)
                {
                    if (Client.isHost)
                        SendPiecePosition("Move", "HPD");
                    else
                        SendPiecePosition("Move", "NHPD");
                }
                else if (RoomListing.IsOnline)
                {
                    object[] data = new object[]
                    {
                        oldPos
                    };
                    PhotonNetwork.RaiseEvent(EventCodes.PIECE_DOWN_EASED, data, RaiseEventOptions.Default, SendOptions.SendReliable);
                }
            }
            else
            {
                // yield return new WaitUntil(() => Piece.easeFinished);
                PieceManager.instance.selectedPiece.targetCell = this;
                PieceManager.instance.selectedPiece.ClearCells();
                Color teamColor = PieceManager.instance.selectedPiece.teamColor;
                StartCoroutine(Board.instance.PerformAction(new Tuple<int, int>(PieceManager.instance.selectedPiece.currentCell.boardPos, boardPos)));
                if (Preserve.instance.isLocalOnline)
                {
                    if (Client.isHost)
                        SendCellPosition("Move", "HM");
                    else
                        SendCellPosition("Move", "NHM");
                }
                else if (RoomListing.IsOnline)
                {
                    object[] data = new object[]
                    {
                        oldPos,
                        boardPos
                    };
                    PhotonNetwork.RaiseEvent(EventCodes.PIECE_MOVE, data, RaiseEventOptions.Default, SendOptions.SendReliable);
                }
                if (PieceManager.instance.selectedPiece != null)
                    PieceManager.instance.selectedPiece.isRotated = false;
                PieceManager.instance.real = true;
                if (!Preserve.instance.againstAI)
                    yield return PieceManager.instance.SwitchSides(teamColor);
                else
                    yield return PieceManager.instance.SwitchSides(Color.white);
            }
            if (PieceManager.instance.selectedPiece != null)
            {
                PieceManager.instance.selectedPiece.ClearCells();
                PieceManager.instance.selectedPiece.targetCell = null;
                PieceManager.instance.selectedPiece = null;
            }
        }
        mouseDownFinished = true;
    }
    #endregion
    
    #region RemovePiece
    public void RemovePiece()
    {
        if (currentPiece != null)
            currentPiece.Kill();
    }
    #endregion
}
