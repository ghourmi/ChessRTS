using UnityEngine;

public class TileClick : MonoBehaviour
{
    public Tile tile;

    void OnMouseDown()
    {
        BoardManager.Instance.OnTileClicked(tile);
    }
}
