using System.Collections.Generic;

public class Rook : Piece
{
    public Rook(Team team, int x, int y, BoardManager board) 
        : base(PieceType.Rook, team, x, y, board) { }

    public override List<Tile> GetLegalMoves()
    {
        List<Tile> moves = new List<Tile>();
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        int maxSteps = 7; // maximaal aantal stappen rook mag vooruit

        for (int dir = 0; dir < 4; dir++)
        {
            int nx = x;
            int ny = y;

            for (int step = 0; step < maxSteps; step++)
            {
                nx += dx[dir];
                ny += dy[dir];

                Tile t = board.GetTile(nx, ny);
                if (t == null) break;

                if (t.occupant == null)
                {
                    moves.Add(t);
                }
                else
                {
                    if (t.occupant.team != team)
                        moves.Add(t); // kan capture
                    break;
                }
            }
        }

        return moves;
    }

}
