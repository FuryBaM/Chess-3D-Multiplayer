using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
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
    private void OnCapture(Piece capturedPiece)
    {
        print($"{capturedPiece} is captured.");
    }
    private void OnEnable() 
    {
        _board.OnCheck.AddListener(OnCheck);
        _board.OnMate.AddListener(OnMate);
        _board.OnStalemate.AddListener(OnStalemate);
        _board.OnCapture.AddListener(OnCapture);
    }

    private void OnDisable()
    {
        _board.OnMate.RemoveListener(OnMate);
        _board.OnCheck.RemoveListener(OnCheck);
        _board.OnStalemate.RemoveListener(OnStalemate);
        _board.OnCapture.RemoveListener(OnCapture);
    }
}
