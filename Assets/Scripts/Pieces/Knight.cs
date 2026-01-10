using System.Collections.Generic;

public class Knight : Piece
{
    public Knight(Team team, int x, int y, BoardManager board)
        : base(PieceType.Knight, team, x, y, board) { }

    public override List<Tile> GetLegalMoves()
    {
        List<Tile> moves = new List<Tile>();
        int[] dx = { 2, 1, -1, -2, -2, -1, 1, 2 };
        int[] dy = { 1, 2, 2, 1, -1, -2, -2, -1 };

        for (int i = 0; i < 8; i++)
        {
            Tile t = board.GetTile(x + dx[i], y + dy[i]);
            if (t != null && (t.occupant == null || t.occupant.team != team))
                moves.Add(t);
        }

        return moves;
    }
}
