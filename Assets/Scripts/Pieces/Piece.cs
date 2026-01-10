using System.Collections.Generic;

public enum PieceType
{
    Pawn,
    Rook,
    Bishop,
    Knight,
    King
}

public abstract class Piece
{
    public PieceType type;    // ✅ voeg type toe
    public int x, y;
    public Team team;
    protected BoardManager board;

    public PieceView view;

    public Piece(PieceType type, Team team, int x, int y, BoardManager board)
    {
        this.type = type;
        this.team = team;
        this.x = x;
        this.y = y;
        this.board = board;
    }

    public abstract List<Tile> GetLegalMoves();
}
