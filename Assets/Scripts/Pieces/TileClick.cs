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

        // Klik op een stuk → selecteer
        if (tile.occupant != null)
        {
            bm.SelectPiece(tile.occupant);
        }
        // Klik op leeg vak → probeer verplaatsen
        else
        {
            bm.OnTileClicked(tile);
        }
    }
}
