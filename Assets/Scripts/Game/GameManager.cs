using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Board _board;

    private void OnCheck()
    {
        print($"Check! Player {_board.Player} has to move.");
    }
    private void OnMate()
    {
        print($"Mate! Player {1 - _board.Player} has won.");
    }
    private void OnStalemate()
    {
        print($"Draw!");
    }
<<<<<<< HEAD

    private void OnEnable()
=======
    private void OnCapture(Piece capturedPiece)
    {
        print($"{capturedPiece} is captured.");
    }
    private void OnEnable() 
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
    {
        _board.OnCheck.AddListener(OnCheck);
        _board.OnMate.AddListener(OnMate);
        _board.OnStalemate.AddListener(OnStalemate);
<<<<<<< HEAD
=======
        _board.OnCapture.AddListener(OnCapture);
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
    }

    private void OnDisable()
    {
        _board.OnMate.RemoveListener(OnMate);
        _board.OnCheck.RemoveListener(OnCheck);
        _board.OnStalemate.RemoveListener(OnStalemate);
<<<<<<< HEAD
=======
        _board.OnCapture.RemoveListener(OnCapture);
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
    }
}
