using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using Stockfish.NET;

public class AIController : MonoBehaviour
{
    [SerializeField] private Side _player = Side.black;
    [SerializeField] private Board _board;
    private IStockfish _stockfish;

    private void Start()
    {
<<<<<<< HEAD
        _stockfish = new Stockfish.NET.Core.Stockfish(@"C:\Users\АхметовАкжол\Chess 3D Akzhol\Assets\stockfish-windows-x86-64.exe");
=======
        _stockfish = new Stockfish.NET.Core.Stockfish($"{Application.dataPath}/stockfish-windows-x86-64.exe");
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
        _stockfish.SkillLevel = 1;
        _board.OnMakeMove.AddListener(OnMakeMove);
    }

    private void OnMakeMove()
    {
<<<<<<< HEAD
        if ((Side)_board.Player != _player) return;
=======
        if ((Side)_board.Player != _player || _board.IsGameOver) return;
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
        StartCoroutine(MakeMoveAsync());
    }

    private IEnumerator MakeMoveAsync()
    {
        yield return new WaitForEndOfFrame();
        Task<string> calculateMoveTask = Task.Run(() => CalculateBestMove());

        while (!calculateMoveTask.IsCompleted)
        {
            yield return null;
        }
        string moveString = calculateMoveTask.Result;
        Move gameMove = _board.ConvertStringToMove(moveString);
        _board.MakeMove(gameMove.MovedPiece, gameMove.StartPosition, gameMove.EndPosition);
    }

    private string CalculateBestMove()
    {
        _stockfish.SetFenPosition(_board.GetFEN());
        return _stockfish.GetBestMove();
    }
}