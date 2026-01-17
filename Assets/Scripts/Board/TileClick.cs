using UnityEngine;

public class TileClick : MonoBehaviour
{
    public Tile tile;

    void OnMouseDown()
    {
        if (tile == null)
        {
            Debug.LogWarning("TileClick without tile!");
            return;
        }

        BoardManager bm = BoardManager.Instance;
        
        bm.OnTileClicked(tile);

    }
}
