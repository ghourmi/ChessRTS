using UnityEngine;

public class InputController : MonoBehaviour
{
    public BoardManager board;

    void Update()
    {
        // ---------------------------------------------------------
        // 1) Selecteer stuk op nummer
        // ---------------------------------------------------------
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Key 1 pressed");
            board.SelectPieceType(PieceType.Pawn);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Key 2 pressed");
            board.SelectPieceType(PieceType.Bishop);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Key 3 pressed");
            board.SelectPieceType(PieceType.Knight);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Key 4 pressed");
            board.SelectPieceType(PieceType.Rook);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("Key 5 pressed");
            board.SelectPieceType(PieceType.King);
        }

        // ---------------------------------------------------------
        // 2) Mode wisselen (AZERTY + QWERTY)
        //    A = Offence
        //    D = Defence
        // ---------------------------------------------------------

        // A op QWERTY = A
        // A op AZERTY = Q
        bool offenceKey =
            Input.GetKeyDown(KeyCode.A) ||   // QWERTY
            Input.GetKeyDown(KeyCode.Q);     // AZERTY

        // D is op beide layouts hetzelfde
        bool defenceKey =
            Input.GetKeyDown(KeyCode.D);

        if (defenceKey)
        {
            Debug.Log("Defence key pressed");
            if (board.CurrentPiece != null)
            {
                board.CurrentPiece.PreviousMode();
                board.CurrentPiece.defendTarget = null; 
                Debug.Log("Mode after Defence: " + board.CurrentPiece.moveMode);
            }
            else
            {
                Debug.Log("Defence pressed but CurrentPiece is null");
            }
        }

        if (offenceKey)
        {
            Debug.Log("Offence key pressed");
            if (board.CurrentPiece != null)
            {
                board.CurrentPiece.NextMode();
                board.CurrentPiece.defendTarget = null; 
                Debug.Log("Mode after Offence: " + board.CurrentPiece.moveMode);
            }
            else
            {
                Debug.Log("Offence pressed but CurrentPiece is null");
            }
        }
    }
}
