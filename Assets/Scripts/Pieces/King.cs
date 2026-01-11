using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public King(Team team, int x, int y, BoardManager board)
        : base(PieceType.King, team, x, y, board) { }

    // ---------------------------------------------------------
    // King: elke tile is 1 zet (altijd 1 vak bewegen)
    // ---------------------------------------------------------
    public override bool IsValidStep(Tile from, Tile to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);

        // Koning beweegt exact 1 tile in elke richting
        return dx <= 1 && dy <= 1 && !(dx == 0 && dy == 0);
    }

    // ---------------------------------------------------------
    // Movement neighbors voor BFS
    // ---------------------------------------------------------
    public override List<Tile> GetMovementNeighbors(Tile from)
    {
        List<Tile> neighbours = new List<Tile>();
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        for (int i = 0; i < 8; i++)
        {
            Tile t = board.GetTile(from.x + dx[i], from.y + dy[i]);
            if (t != null && (t.occupant == null || t.occupant.team != team))
                neighbours.Add(t);
        }

        return neighbours;
    }

    public override List<Tile> GetLegalMoves()
    {
        return GetMovementNeighbors(board.GetTile(x, y));
    }
}
