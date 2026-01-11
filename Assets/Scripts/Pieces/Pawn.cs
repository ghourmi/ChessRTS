using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public Pawn(Team team, int x, int y, BoardManager board)
        : base(PieceType.Pawn, team, x, y, board) { }

    // ---------------------------------------------------------
    // Pawn: elke tile in het BFS-pad is 1 zet
    // (pion beweegt nooit meerdere tiles in één zet)
    // ---------------------------------------------------------
    public override bool IsValidStep(Tile from, Tile to)
    {
        return true; 
    }

    // ---------------------------------------------------------
    // Movement neighbors voor BFS
    // ---------------------------------------------------------
    public override List<Tile> GetMovementNeighbors(Tile from)
    {
        List<Tile> neighbours = new List<Tile>();
        int dir = team == Team.White ? 1 : -1;

        // 1 vooruit
        Tile fwd = board.GetTile(from.x, from.y + dir);
        if (fwd != null && fwd.occupant == null)
            neighbours.Add(fwd);

        // 2 vooruit vanaf startpositie
        if ((team == Team.White && from.y == 1) || (team == Team.Black && from.y == 6))
        {
            Tile fwd2 = board.GetTile(from.x, from.y + 2 * dir);
            if (fwd2 != null && fwd2.occupant == null)
                neighbours.Add(fwd2);
        }

        // Captures
        Tile capL = board.GetTile(from.x - 1, from.y + dir);
        Tile capR = board.GetTile(from.x + 1, from.y + dir);

        if (capL != null && capL.occupant != null && capL.occupant.team != team)
            neighbours.Add(capL);
        if (capR != null && capR.occupant != null && capR.occupant.team != team)
            neighbours.Add(capR);

        return neighbours;
    }

    public override List<Tile> GetLegalMoves()
    {
        return GetMovementNeighbors(board.GetTile(x, y));
    }
}
