using UnityEngine;

public class BoardGenerator
{
    private int width;
    private int height;
    private float tileSize;
    private GameObject tilePrefab;

    public BoardGenerator(int width, int height, float tileSize, GameObject tilePrefab)
    {
        this.width = width;
        this.height = height;
        this.tileSize = tileSize;
        this.tilePrefab = tilePrefab;
    }

    public Tile[,] GenerateBoard()
    {
        Tile[,] board = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isDark = (x + y) % 2 == 1;
                board[x, y] = new Tile(x, y, isDark);
            }
        }

        return board;
    }

    public void SpawnVisualBoard(Tile[,] board, Transform parent)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tileGO = Object.Instantiate(tilePrefab, parent);
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

                board[x, y].tileGO = tileGO;
            }
        }
    }
}
