using UnityEngine;

public class Tile
{
    public int x, y;
    public bool isDark;
    public Piece occupant;
    public Piece reservedBy;

    // Runtime visual
    public GameObject tileGO;

    public Tile(int x, int y, bool isDark)
    {
        this.x = x;
        this.y = y;
        this.isDark = isDark;
    }

    public void SetHighlight(bool highlight)
    {
        if (tileGO != null)
        {
            SpriteRenderer sr = tileGO.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = highlight 
                ? new Color(0f, 1f, 0f, 0.5f) // semi-transparent groen
                : (isDark ? new Color(0.35f,0.35f,0.35f) : new Color(0.8f,0.8f,0.8f));

            }
        }
    }
}


