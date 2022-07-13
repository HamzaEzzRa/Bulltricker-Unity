using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Members
    public Board board;
    public PieceManager pieceManager;
    public Cell[,] allCells;
    #endregion

    #region Start
    void Start()
    {
        board.Setup();
        pieceManager.Setup();
    }
    #endregion
}