using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{
    public Knight(Team team, int x, int y, BoardManager board)
        : base(PieceType.Knight, team, x, y, board) { }

    // ---------------------------------------------------------
    // Knight: echte sprong detecteren (1 zet)
    // ---------------------------------------------------------
    public override bool IsValidStep(Tile from, Tile to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        return (dx == 1 && dy == 2) || (dx == 2 && dy == 1);
    }

    // ---------------------------------------------------------
    // Knight movement neighbors (voor BFS)
    // ---------------------------------------------------------
    public override List<Tile> GetMovementNeighbors(Tile from)
    {
        List<Tile> neighbors = new List<Tile>();

        int[] dx = { 1, 2, 2, 1, -1, -2, -2, -1 };
        int[] dy = { 2, 1, -1, -2, 2, 1, -1, -2 };

        for (int i = 0; i < 8; i++)
        {
            int nx = from.x + dx[i];
            int ny = from.y + dy[i];

            Tile t = board.GetTile(nx, ny);
            if (t == null) continue;
            if (t.occupant == null || t.occupant.team != team)
                neighbors.Add(t);
        }

        return neighbors;
    }

    public override List<Tile> GetLegalMoves()
    {
        return GetMovementNeighbors(board.GetTile(x, y));
    }
}
