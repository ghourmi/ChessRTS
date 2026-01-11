using System.Collections.Generic;
using UnityEngine;

public enum PieceType
{
    Pawn,
    Rook,
    Bishop,
    Knight,
    King
}

public enum MoveMode
{
    Offence,
    Defence
}

public abstract class Piece
{
    public PieceType type;
    public int x, y;
    public Team team;
    public BoardManager board;
    public PieceView view;
    public MoveMode moveMode;

    public Piece(PieceType type, Team team, int x, int y, BoardManager board)
    {
        this.type = type;
        this.team = team;
        this.x = x;
        this.y = y;
        this.board = board;
        this.moveMode = MoveMode.Defence;
    }

    // ---------------------------------------------------------
    // 1) Bepaalt of een tile-overgang een echte zet is
    // ---------------------------------------------------------
    public virtual bool IsValidStep(Tile from, Tile to)
    {
        // Default: lineaire stukken → elke tile in dezelfde richting is 1 zet
        return true;
    }

    // Knight override
    public virtual bool IsKnight()
    {
        return type == PieceType.Knight;
    }

    // ---------------------------------------------------------
    // 2) Converteert BFS-pad naar echte zetten
    // ---------------------------------------------------------
    public List<Tile> ConvertPathToMoves(List<Tile> path)
    {
        List<Tile> moves = new List<Tile>();
        Tile current = board.GetTile(x, y);

        foreach (Tile t in path)
        {
            if (IsValidStep(current, t))
            {
                moves.Add(t);
                current = t;
            }
        }

        return moves;
    }

    // ---------------------------------------------------------
    // 3) Defence mode: 1 zet voor het doel stoppen
    // ---------------------------------------------------------
    public List<Tile> ApplyDefenceMode(List<Tile> moves)
    {
        if (moves.Count <= 1)
            return moves;

        // Schrappen van laatste zet
        moves.RemoveAt(moves.Count - 1);
        return moves;
    }

    // ---------------------------------------------------------
    // Abstracts
    // ---------------------------------------------------------
    public abstract List<Tile> GetMovementNeighbors(Tile from);
    public abstract List<Tile> GetLegalMoves();
}
