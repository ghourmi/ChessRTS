using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance; // singleton

    [Header("Board Settings")]
    public int width = 8;
    public int height = 8;
    public float tileSize = 1f;
    public GameObject tilePrefab;

    [Header("Piece Settings")]
    public GameObject pieceViewPrefab;

    [HideInInspector]
    public Tile[,] board;

    private Piece selectedPiece;
    private List<GameObject> highlightObjects = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateBoard();
        SpawnVisualBoard();
        CenterCamera();

        TestRook();
    }

    // ----------------------
    // Board generation
    // ----------------------
    void GenerateBoard()
    {
        board = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isDark = (x + y) % 2 == 1;
                board[x, y] = new Tile(x, y, isDark);
            }
        }
    }

    void SpawnVisualBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tileGO = Instantiate(tilePrefab, this.transform);
                tileGO.transform.position = new Vector3(x * tileSize, y * tileSize, 0);
                tileGO.name = $"Tile_{x}_{y}";

                SpriteRenderer sr = tileGO.GetComponent<SpriteRenderer>();
                sr.color = board[x, y].isDark
                    ? new Color(0.35f, 0.35f, 0.35f)
                    : new Color(0.8f, 0.8f, 0.8f);

                if (tileGO.GetComponent<Collider2D>() == null)
                    tileGO.AddComponent<BoxCollider2D>();

                TileClick tc = tileGO.AddComponent<TileClick>();
                tc.tile = board[x, y];
            }
        }
    }

    void CenterCamera()
    {
        Camera.main.transform.position = new Vector3(
            (width - 1) * tileSize / 2f,
            (height - 1) * tileSize / 2f,
            -10
        );
    }

    // ----------------------
    // Piece selection & movement
    // ----------------------
    public void SelectPiece(Piece piece)
    {
        selectedPiece = piece;
        HighlightLegalMoves(piece);
    }

    public void HighlightLegalMoves(Piece piece)
    {
        ClearHighlights();

        List<Tile> moves = piece.GetLegalMoves();

        foreach (Tile t in moves)
        {
            GameObject highlight = new GameObject($"Highlight_{t.x}_{t.y}");
            highlight.transform.position = new Vector3(t.x * tileSize + tileSize / 2f, t.y * tileSize + tileSize / 2f, -0.5f);
            SpriteRenderer sr = highlight.AddComponent<SpriteRenderer>();
            sr.color = new Color(0f, 1f, 0f, 0.5f);
            sr.sprite = tilePrefab.GetComponent<SpriteRenderer>().sprite;
            sr.sortingOrder = 100;
            sr.transform.localScale = Vector3.one * 0.9f;

            highlightObjects.Add(highlight);

            TileClick tc = highlight.AddComponent<TileClick>();
            tc.tile = t;
        }
    }

    void ClearHighlights()
    {
        foreach (GameObject go in highlightObjects)
            Destroy(go);
        highlightObjects.Clear();
    }

    public void OnTileClicked(Tile targetTile)
    {
        if (selectedPiece == null) return;

        List<Tile> path = CalculatePath(selectedPiece, targetTile);

        if (path.Count == 0)
        {
            Debug.Log("Illegal move!");
            return;
        }

        StartCoroutine(MoveAlongPath(selectedPiece, path));

        selectedPiece = null;
        ClearHighlights();
    }

    IEnumerator MoveAlongPath(Piece piece, List<Tile> path)
    {
        foreach (Tile t in path)
        {
            Vector3 targetPos = new Vector3(
                t.x * tileSize,
                t.y * tileSize,
                piece.view.transform.position.z
            );

            while ((piece.view.transform.position - targetPos).sqrMagnitude > 0.001f)
            {
                piece.view.transform.position = Vector3.MoveTowards(piece.view.transform.position, targetPos, 3f * Time.deltaTime);
                yield return null;
            }

            // Board update
            board[piece.x, piece.y].occupant = null;

            if (t.occupant != null && t.occupant.team != piece.team)
                Capture(t.occupant);

            piece.x = t.x;
            piece.y = t.y;
            board[t.x, t.y].occupant = piece;
        }
    }

    void Capture(Piece target)
    {
        Destroy(target.view.gameObject);
        board[target.x, target.y].occupant = null;
    }

    // ----------------------
    // Pathfinding
    // ----------------------
    List<Tile> CalculatePath(Piece piece, Tile target)
    {
        // BFS
        Queue<Tile> frontier = new Queue<Tile>();
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();

        Tile start = board[piece.x, piece.y];
        frontier.Enqueue(start);
        cameFrom[start] = null;

        while (frontier.Count > 0)
        {
            Tile current = frontier.Dequeue();
            if (current == target) break;

            foreach (Tile neighbor in GetNeighbors(piece, current))
            {
                if (!cameFrom.ContainsKey(neighbor) && (neighbor.occupant == null || neighbor == target))
                {
                    frontier.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        List<Tile> path = new List<Tile>();
        if (!cameFrom.ContainsKey(target))
        {
            Debug.LogWarning("No path found to target!");
            return path;
        }

        Tile t = target;
        while (t != start)
        {
            path.Insert(0, t);
            t = cameFrom[t];
        }

        return path;
    }

    List<Tile> GetNeighbors(Piece piece, Tile current)
    {
        List<Tile> neighbors = new List<Tile>();

        switch (piece.type)
        {
            case PieceType.Rook:
                neighbors.AddRange(GetRookMoves(current, piece));
                break;
            case PieceType.Bishop:
                neighbors.AddRange(GetBishopMoves(current, piece));
                break;
            case PieceType.Knight:
                neighbors.AddRange(GetKnightMoves(current, piece));
                break;
            case PieceType.King:
                neighbors.AddRange(GetKingMoves(current, piece));
                break;
            case PieceType.Pawn:
                neighbors.AddRange(GetPawnMoves(current, piece));
                break;
        }

        return neighbors;
    }

    // ----------------------
    // Movement rules helpers
    // ----------------------
    List<Tile> GetRookMoves(Tile current, Piece piece)
    {
        List<Tile> moves = new List<Tile>();
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        for (int dir = 0; dir < 4; dir++)
        {
            int nx = current.x;
            int ny = current.y;
            while (true)
            {
                nx += dx[dir];
                ny += dy[dir];
                Tile t = GetTile(nx, ny);
                if (t == null) break;
                if (t.occupant != null)
                {
                    if (t.occupant.team == piece.team) break;
                    moves.Add(t);
                    break;
                }
                moves.Add(t);
            }
        }
        return moves;
    }

    List<Tile> GetBishopMoves(Tile current, Piece piece)
    {
        List<Tile> moves = new List<Tile>();
        int[] dx = { 1, 1, -1, -1 };
        int[] dy = { 1, -1, 1, -1 };

        for (int dir = 0; dir < 4; dir++)
        {
            int nx = current.x;
            int ny = current.y;
            while (true)
            {
                nx += dx[dir];
                ny += dy[dir];
                Tile t = GetTile(nx, ny);
                if (t == null) break;
                if (t.occupant != null)
                {
                    if (t.occupant.team == piece.team) break;
                    moves.Add(t);
                    break;
                }
                moves.Add(t);
            }
        }
        return moves;
    }

    List<Tile> GetKnightMoves(Tile current, Piece piece)
    {
        List<Tile> moves = new List<Tile>();
        int[] dx = {1,2,2,1,-1,-2,-2,-1};
        int[] dy = {2,1,-1,-2,2,1,-1,-2};

        for(int i=0;i<8;i++)
        {
            Tile t = GetTile(current.x + dx[i], current.y + dy[i]);
            if(t != null && (t.occupant == null || t.occupant.team != piece.team))
                moves.Add(t);
        }
        return moves;
    }

    List<Tile> GetKingMoves(Tile current, Piece piece)
    {
        List<Tile> moves = new List<Tile>();
        for(int dx=-1; dx<=1; dx++)
        {
            for(int dy=-1; dy<=1; dy++)
            {
                if(dx==0 && dy==0) continue;
                Tile t = GetTile(current.x + dx, current.y + dy);
                if(t != null && (t.occupant == null || t.occupant.team != piece.team))
                    moves.Add(t);
            }
        }
        return moves;
    }

    List<Tile> GetPawnMoves(Tile current, Piece piece)
    {
        List<Tile> moves = new List<Tile>();
        int dir = piece.team == Team.White ? 1 : -1;

        // 1 vooruit
        Tile fwd = GetTile(current.x, current.y + dir);
        if(fwd != null && fwd.occupant == null) moves.Add(fwd);

        // 2 vooruit vanaf start
        if((piece.team == Team.White && current.y==1) || (piece.team == Team.Black && current.y==6))
        {
            Tile fwd2 = GetTile(current.x, current.y + 2*dir);
            if(fwd2 != null && fwd2.occupant == null) moves.Add(fwd2);
        }

        // Captures
        Tile capL = GetTile(current.x - 1, current.y + dir);
        Tile capR = GetTile(current.x + 1, current.y + dir);
        if(capL != null && capL.occupant != null && capL.occupant.team != piece.team) moves.Add(capL);
        if(capR != null && capR.occupant != null && capR.occupant.team != piece.team) moves.Add(capR);

        return moves;
    }

    // ----------------------
    // TEST
    // ----------------------
    void TestRook()
    {
        Piece rook = new Rook(Team.White, 0, 0, this);
        board[0, 0].occupant = rook;

        GameObject pv = Instantiate(pieceViewPrefab, this.transform);
        pv.transform.position = new Vector3(
            rook.x * tileSize,
            rook.y * tileSize,
            0
        );

        PieceView pvScript = pv.GetComponent<PieceView>();
        rook.view = pvScript;
        pvScript.piece = rook;

        HighlightLegalMoves(rook);
    }

    // ----------------------
    // Get tile helper
    // ----------------------
    public Tile GetTile(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        return board[x, y];
    }
}
