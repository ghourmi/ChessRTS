using System.Collections.Generic;
using UnityEngine;

public class Rook : Piece
{
    public Rook(Team team, int x, int y, BoardManager board)
        : base(PieceType.Rook, team, x, y, board) { }

    // ---------------------------------------------------------
    // Rook: een zet = alle tiles in dezelfde rechte lijn
    // ---------------------------------------------------------
    public override bool IsValidStep(Tile from, Tile to)
    {
        // Zelfde rij of zelfde kolom = 1 zet
        return (from.x == to.x) || (from.y == to.y);
    }

    // ---------------------------------------------------------
    // Movement neighbors voor BFS
    // ---------------------------------------------------------
    public override List<Tile> GetMovementNeighbors(Tile from)
    {
        List<Tile> neighbors = new List<Tile>();

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

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
                        neighbors.Add(t);
                    break;
                }

                neighbors.Add(t);
            }
        }

        return neighbors;
    }

    public override List<Tile> GetLegalMoves()
    {
        return GetMovementNeighbors(board.GetTile(x, y));
    }
}
