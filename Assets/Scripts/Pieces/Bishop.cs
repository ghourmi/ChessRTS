using System.Collections.Generic;

public class Bishop : Piece
{
    public Bishop(Team team, int x, int y, BoardManager board)
        : base(PieceType.Bishop, team, x, y, board) { }

    public override List<Tile> GetLegalMoves()
    {
        List<Tile> moves = new List<Tile>();
        int[] dx = { 1, 1, -1, -1 };
        int[] dy = { 1, -1, 1, -1 };

        for (int dir = 0; dir < 4; dir++)
        {
            int nx = x;
            int ny = y;
            while (true)
            {
                nx += dx[dir];
                ny += dy[dir];
                Tile t = board.GetTile(nx, ny);
                if (t == null) break;
                if (t.occupant == null)
                    moves.Add(t);
                else
                {
                    if (t.occupant.team != team)
                        moves.Add(t);
                    break;
                }
            }
        }

        return moves;
    }
}
