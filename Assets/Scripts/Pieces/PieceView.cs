using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PieceView : MonoBehaviour
{
    public Piece piece;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        BoardManager.Instance.SelectPiece(piece);
        BoardManager.Instance.HighlightLegalMoves(piece);
    }

    public void SetSelected(bool selected)
    {
        sr.color = selected ? Color.yellow : Color.white;
    }
}
