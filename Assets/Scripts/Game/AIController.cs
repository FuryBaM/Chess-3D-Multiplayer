using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stockfish.NET;
using Stockfish.NET.Core;

using System.Collections;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] private Side _player = Side.black;
    [SerializeField] private Board _board;
    private IStockfish _stockfish;

    private void Start()
    {
        _stockfish = new Stockfish.NET.Core.Stockfish(@"C:\Users\АхметовАкжол\Chess 3D Akzhol\Assets\stockfish-windows-x86-64.exe");
        _board.OnMakeMove.AddListener(OnMakeMove);
    }

    private void OnMakeMove()
    {
        if ((Side)_board.Player != _player) return;
        StartCoroutine(MakeAIMove());
    }

    private IEnumerator MakeAIMove()
    {
        yield return null;

        _stockfish.SetFenPosition(_board.GetFEN());
        _stockfish.Depth = 12;
        string moveString = _stockfish.GetBestMove();
        Move gameMove = _board.ConvertStringToMove(moveString);

        yield return new WaitForFixedUpdate();

        _board.MakeMove(gameMove.MovedPiece, gameMove.StartPosition, gameMove.EndPosition);
    }
}

