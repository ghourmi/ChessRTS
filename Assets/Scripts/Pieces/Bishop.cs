using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
    public Bishop(Team team, int x, int y, BoardManager board)
        : base(PieceType.Bishop, team, x, y, board) { }

    // ---------------------------------------------------------
    // Bishop: een zet = alle tiles in dezelfde diagonale lijn
    // ---------------------------------------------------------
    public override bool IsValidStep(Tile from, Tile to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);

        // Diagonale beweging → 1 zet
        return dx == dy && dx > 0;
    }

    // ---------------------------------------------------------
    // Movement neighbors voor BFS
    // ---------------------------------------------------------
    public override List<Tile> GetMovementNeighbors(Tile from)
    {
        List<Tile> neighbours = new List<Tile>();
        int[] dx = { 1, 1, -1, -1 };
        int[] dy = { 1, -1, 1, -1 };

        for (int dir = 0; dir < 4; dir++)
        {
            int nx = from.x;
            int ny = from.y;

            while (true)
            {
                nx += dx[dir];
                ny += dy[dir];

                Tile t = board.GetTile(nx, ny);
                if (t == null) break;

                if (t.occupant != null)
                {
                    if (t.occupant.team != team)
                        neighbours.Add(t);
                    break;
                }

                neighbours.Add(t);
            }
        }

        return neighbours;
    }

    public override List<Tile> GetLegalMoves()
    {
        return GetMovementNeighbors(board.GetTile(x, y));
    }
}
