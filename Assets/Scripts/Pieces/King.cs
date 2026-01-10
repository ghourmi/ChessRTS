using System.Collections.Generic;

public class King : Piece
{
    public King(Team team, int x, int y, BoardManager board)
        : base(PieceType.King, team, x, y, board) { }

    public override List<Tile> GetLegalMoves()
    {
        List<Tile> moves = new List<Tile>();
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        for (int i = 0; i < 8; i++)
        {
            Tile t = board.GetTile(x + dx[i], y + dy[i]);
            if (t != null && (t.occupant == null || t.occupant.team != team))
                moves.Add(t);
        }

        return moves;
    }
}
