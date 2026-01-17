using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Board Settings")]
    public int width = 8;
    public int height = 8;
    public float tileSize = 1f;
    public GameObject tilePrefab;

    [Header("Piece Settings")]
    public GameObject pieceViewPrefab;

    [HideInInspector]
    public Tile[,] board;

    public List<Piece> pieces = new List<Piece>();

    private Piece selectedPiece;
    private List<GameObject> highlightObjects = new List<GameObject>();

    public Piece CurrentPiece { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("[Board] Start");
        BoardGenerator generator = new BoardGenerator(width, height, tileSize, tilePrefab);
        board = generator.GenerateBoard();
        generator.SpawnVisualBoard(board, this.transform);
        CenterCamera();
        SetupTestPieces();
    }

    void CenterCamera()
    {
        Debug.Log("[Board] CenterCamera");
        Camera.main.transform.position = new Vector3(
            (width - 1) * tileSize / 2f,
            (height - 1) * tileSize / 2f,
            -10
        );
    }

    // ---------------------------------------------------------
    // SELECTIE
    // ---------------------------------------------------------

    public void SelectPieceType(PieceType type)
    {
        CurrentPiece = pieces.Find(p => p.type == type && p.team == Team.White);

        if (CurrentPiece == null)
        {
            Debug.LogWarning("[SelectPieceType] No piece of type " + type + " found!");
            return;
        }

        selectedPiece = CurrentPiece;
        HighlightLegalMoves(CurrentPiece);

        Debug.Log($"[SelectPieceType] Selected: {type} | Mode: {CurrentPiece.moveMode} at ({CurrentPiece.x},{CurrentPiece.y})");
    }

    public void SelectPiece(Piece piece)
    {
        Debug.Log($"[SelectPiece] Clicked on {piece.type} {piece.team} at ({piece.x},{piece.y})");

        // CASE 1 — Je hebt een defender geselecteerd en klikt op een friendly piece
        if (CurrentPiece != null &&
            CurrentPiece.moveMode == MoveMode.Defence &&
            piece.team == Team.White &&               // alleen jouw team
            piece != CurrentPiece)                    // niet zichzelf
        {
            CurrentPiece.defendTarget = piece;
            Debug.Log($"[SelectPiece] {CurrentPiece.type} is now defending {piece.type}");
            return; // NIET selecteren, NIET highlighten
        }

        // CASE 2 — Normale selectie
        if (piece.defendTarget != null)
            Debug.Log($"[SelectPiece] Clearing old defendTarget of {piece.type}");

        piece.defendTarget = null;

        selectedPiece = piece;
        CurrentPiece = piece;
        Debug.Log($"[SelectPiece] Selected {piece.type} at ({piece.x},{piece.y}) | Mode: {piece.moveMode}");
        HighlightLegalMoves(piece);
    }

    // ---------------------------------------------------------
    // HIGHLIGHTING
    // ---------------------------------------------------------

    public void   HighlightLegalMoves(Piece piece)
    {
        ClearHighlights();

        List<Tile> moves = piece.GetLegalMoves();
        Debug.Log($"[HighlightLegalMoves] {piece.type} has {moves.Count} legal moves (mode: {piece.moveMode})");

        foreach (Tile t in moves)
        {
            GameObject highlight = new GameObject($"Highlight_{t.x}_{t.y}");
            highlight.transform.position = new Vector3(t.x * tileSize, t.y * tileSize, -0.5f);

            SpriteRenderer sr = highlight.AddComponent<SpriteRenderer>();
            sr.color = new Color(0f, 1f, 0f, 0.5f);
            sr.sprite = tilePrefab.GetComponent<SpriteRenderer>().sprite;
            sr.sortingOrder = 100;
            sr.transform.localScale = Vector3.one * 0.9f;

            highlight.layer = LayerMask.NameToLayer("Ignore Raycast");
            Collider2D col = highlight.GetComponent<Collider2D>();
            if (col != null) Destroy(col);

            // HighlightClick hc = highlight.AddComponent<HighlightClick>();
            // hc.targetTile = t;

            highlightObjects.Add(highlight);
        }
    }

    void ClearHighlights()
    {
        foreach (GameObject go in highlightObjects)
            Destroy(go);

        highlightObjects.Clear();
    }

    // ---------------------------------------------------------
    // TILE CLICK
    // ---------------------------------------------------------

    public void OnTileClicked(Tile targetTile)
    {
        Debug.Log($"[OnTileClicked] tile ({targetTile?.x},{targetTile?.y}), selectedPiece = {selectedPiece?.type}");

        if (selectedPiece == null || targetTile == null)
        {
            Debug.Log("[OnTileClicked] Early return: selectedPiece or targetTile null");
            return;
        }

        // Als ik een tile klik, verbreek ik verdedigen (alleen als dit zelf een defender is)
        if (selectedPiece.defendTarget != null)
        {
            Debug.Log($"[OnTileClicked] {selectedPiece.type} stopt met verdedigen {selectedPiece.defendTarget.type}");
            selectedPiece.defendTarget = null;
        }

        bool isDefenceMode = selectedPiece.moveMode == MoveMode.Defence;
        Debug.Log($"[OnTileClicked] {selectedPiece.type} moving in mode {selectedPiece.moveMode} to ({targetTile.x},{targetTile.y})");

        List<Tile> path = CalculatePath(selectedPiece, targetTile, isDefenceMode);
        Debug.Log($"[OnTileClicked] Path length = {path.Count}");

        if (path.Count == 0)
        {
            Debug.Log("[OnTileClicked] No path, aborting move");
            return;
        }

        foreach (Tile t in path)
        {
            if (
                (t.occupant != null && t.occupant != selectedPiece.defendTarget) ||
                (t.reservedBy != null && t.reservedBy != selectedPiece.defendTarget)
            )
            {
                Debug.Log($"[OnTileClicked] Path blocked / reserved at ({t.x},{t.y}) by {t.occupant?.type} / reservedBy {t.reservedBy?.type}");
                return;
            }
        }

        foreach (Tile t in path)
            t.reservedBy = selectedPiece;

        Tile reservedTarget = path[path.Count - 1];
        Debug.Log($"[OnTileClicked] Starting MoveAlongPath for {selectedPiece.type} to ({reservedTarget.x},{reservedTarget.y})");
        StartCoroutine(MoveAlongPath(selectedPiece, path, reservedTarget));

        selectedPiece = null;
        ClearHighlights();
    }

    // ---------------------------------------------------------
    // MOVEMENT
    // ---------------------------------------------------------

    IEnumerator MoveAlongPath(Piece piece, List<Tile> path, Tile reservedTarget)
    {
        Debug.Log($"[MoveAlongPath] START {piece.type} from ({piece.x},{piece.y}) to ({reservedTarget.x},{reservedTarget.y}), steps: {path.Count}");

        // --- MAIN MOVEMENT LOOP ---
        foreach (Tile t in path)
        {
            if (t.occupant != null && t.occupant != piece && t.occupant != piece.defendTarget)
            {
                Debug.Log($"[MoveAlongPath] Blocked at ({t.x},{t.y}) by {t.occupant.type}, breaking");
                break;
            }

            Vector3 targetPos = new Vector3(
                t.x * tileSize,
                t.y * tileSize,
                piece.view.transform.position.z
            );

            while ((piece.view.transform.position - targetPos).sqrMagnitude > 0.001f)
            {
                piece.view.transform.position =
                    Vector3.MoveTowards(piece.view.transform.position, targetPos, 3f * Time.deltaTime);
                yield return null;
            }

            Debug.Log($"[MoveAlongPath] {piece.type} reached tile ({t.x},{t.y})");

            board[piece.x, piece.y].occupant = null;
            piece.x = t.x;
            piece.y = t.y;
            board[t.x, t.y].occupant = piece;
        }

        // --- CLEAR RESERVATIONS ---
        foreach (Tile t in path)
        {
            if (t.reservedBy == piece)
                t.reservedBy = null;
        }

        Debug.Log($"[MoveAlongPath] {piece.type} finished at ({piece.x},{piece.y}). Checking defenders...");

        // --- DEFENDER FOLLOW-UP ---
        foreach (Piece p in pieces)
        {
            if (p.defendTarget == piece)
            {
                Debug.Log($"[MoveAlongPath] Defender FOUND: {p.type} at ({p.x},{p.y}) defending {piece.type}");

                // 1) Pathfind naar de exacte eindtile van het target (zonder defence mode)
                List<Tile> rawPath = CalculatePath(p, reservedTarget, false);
                Debug.Log($"[MoveAlongPath] Defender {p.type} raw path length: {rawPath.Count}");

                if (rawPath.Count == 0)
                {
                    Debug.Log($"[MoveAlongPath] Defender {p.type} has NO path toward ({reservedTarget.x},{reservedTarget.y})");
                    continue;
                }

                // 2) Defence-mode toepassen → laatste stap wegknippen
                rawPath = p.ApplyDefenceMode(rawPath);
                Debug.Log($"[MoveAlongPath] Defender {p.type} defence-trimmed path length: {rawPath.Count}");

                if (rawPath.Count == 0)
                {
                    Debug.Log($"[MoveAlongPath] Defender {p.type} cannot get 1 tile away from target");
                    continue;
                }

                Tile defenderEnd = rawPath[rawPath.Count - 1];
                Debug.Log($"[MoveAlongPath] Defender {p.type} will end at ({defenderEnd.x},{defenderEnd.y})");

                StartCoroutine(MoveAlongPath(p, rawPath, defenderEnd));
            }
        }

        Debug.Log($"[MoveAlongPath] END {piece.type}");
    }

    // ---------------------------------------------------------
    // PATHFINDING
    // ---------------------------------------------------------

    List<Tile> CalculatePath(Piece piece, Tile target, bool isDefenceMode)
    {
        Debug.Log($"[CalculatePath] {piece.type} from ({piece.x},{piece.y}) to ({target.x},{target.y}), defenceMode={isDefenceMode}");

        Queue<Tile> frontier = new Queue<Tile>();
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();

        Tile start = board[piece.x, piece.y];
        frontier.Enqueue(start);
        cameFrom[start] = null;

        while (frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();
            if (current == target)
                break;

            foreach (Tile neighbor in piece.GetMovementNeighbors(current))
            {
                if (neighbor == null) continue;

                if (!cameFrom.ContainsKey(neighbor) &&
                    (
                        neighbor.occupant == null ||
                        neighbor == target ||
                        neighbor.occupant == piece.defendTarget || neighbor == target

                    ))
                {
                    cameFrom[neighbor] = current;
                    frontier.Enqueue(neighbor);
                }
            }
        }

        if (!cameFrom.ContainsKey(target))
        {
            Debug.LogWarning($"[CalculatePath] No path found for {piece.type} to ({target.x},{target.y})");
            return new List<Tile>();
        }

        List<Tile> tilePath = new List<Tile>();
        Tile t = target;

        while (t != start)
        {
            tilePath.Insert(0, t);
            t = cameFrom[t];
        }

        Debug.Log($"[CalculatePath] Raw path length (tiles) = {tilePath.Count}");

        List<Tile> moves = piece.ConvertPathToMoves(tilePath);
        Debug.Log($"[CalculatePath] ConvertPathToMoves -> {moves.Count} moves (before defence)");

        if (isDefenceMode)
        {
            moves = piece.ApplyDefenceMode(moves);
            Debug.Log($"[CalculatePath] After ApplyDefenceMode -> {moves.Count} moves");
        }

        return moves;
    }

    // ---------------------------------------------------------
    // MOVEMENT RULES
    // ---------------------------------------------------------

    // List<Tile> GetNeighbors(Piece piece, Tile current)
    // {
    //     List<Tile> neighbors = piece.GetMovementNeighbors(current);
    //     return neighbors;
    // }

    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------

    public Tile GetTile(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        return board[x, y];
    }

    void SetupTestPieces()
    {
        Debug.Log("[Board] SetupTestPieces");
        PlacePiece(new Rook(Team.White, 0, 0, this));
        PlacePiece(new Knight(Team.White, 1, 0, this));
        PlacePiece(new Bishop(Team.White, 2, 0, this));
        PlacePiece(new King(Team.White, 4, 0, this));
        PlacePiece(new Pawn(Team.White, 0, 1, this));
        PlacePiece(new Pawn(Team.White, 1, 1, this));
        PlacePiece(new Pawn(Team.White, 2, 1, this));

        PlacePiece(new Rook(Team.Black, 0, 7, this));
        PlacePiece(new Knight(Team.Black, 1, 7, this));
        PlacePiece(new Bishop(Team.Black, 2, 7, this));
        PlacePiece(new King(Team.Black, 4, 7, this));
        PlacePiece(new Pawn(Team.Black, 0, 6, this));
        PlacePiece(new Pawn(Team.Black, 1, 6, this));
        PlacePiece(new Pawn(Team.Black, 2, 6, this));
    }

    void PlacePiece(Piece piece)
    {
        pieces.Add(piece);
        board[piece.x, piece.y].occupant = piece;

        GameObject pv = Instantiate(pieceViewPrefab, this.transform);
        pv.transform.position = new Vector3(piece.x * tileSize, piece.y * tileSize, 0);

        PieceView pvScript = pv.GetComponent<PieceView>();
        piece.view = pvScript;
        pvScript.piece = piece;

        Debug.Log($"[PlacePiece] Placed {piece.type} {piece.team} at ({piece.x},{piece.y})");
    }
}
