using System.Collections.Generic;

public class Pawn : Piece
{
    public Pawn(Team team, int x, int y, BoardManager board)
        : base(PieceType.Pawn, team, x, y, board) { }

    public override List<Tile> GetLegalMoves()
    {
        List<Tile> moves = new List<Tile>();
        int dir = team == Team.White ? 1 : -1;

        // 1 stap vooruit
        Tile forward = board.GetTile(x, y + dir);
        if (forward != null && forward.occupant == null)
            moves.Add(forward);

        // Diagonaal captures
        int[] dx = { -1, 1 };
        foreach (int d in dx)
        {
            Tile t = board.GetTile(x + d, y + dir);
            if (t != null && t.occupant != null && t.occupant.team != team)
                moves.Add(t);
        }

        return moves;
    }
}
