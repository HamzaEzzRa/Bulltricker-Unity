[System.Serializable]
public enum PieceType
{
    EMPTY,
    PAWN,
    QUEEN,
    KING
}

[System.Serializable]
public enum PieceColor
{
    WHITE,
    BLACK
}

[System.Serializable]
public class GameData
{
    public PieceData[] pieceData = new PieceData[48];
    public bool isBlackTurn;
    public int numberOfWhitePieces, numberOfWhitePawns;
    public int numberOfBlackPieces, numberOfBlackPawns;
}

[System.Serializable]
public class PieceData
{
    // Possibly needs an id for referencing on load ?
    public PieceColor color;
    public PieceType type;
    public int cellPosition;
}
