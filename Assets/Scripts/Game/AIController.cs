using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using Stockfish.NET;
using Mirror;

public class AIController : NetworkBehaviour
{
    [SerializeField] private Side _player = Side.black;
    [SerializeField] private Board _board;
    private IStockfish _stockfish;

    private void Start()
    {
        InitStockfish();
        _board.OnMakeMove.AddListener(OnMakeMove);
        _board.OnCastle.AddListener(OnMakeMove);
        _board.OnPromotion.AddListener(OnMakeMove);
    }

    public void InitStockfish()
    {
        if (!PlayerPrefs.HasKey("StockfishPath"))
        {
            return;
        }
        if (!PlayerPrefs.HasKey("SkillLevel"))
        {
            PlayerPrefs.SetInt("SkillLevel", 1);
        }
        string pathToStockfish = PlayerPrefs.GetString("StockfishPath");
        int stockfishSkillLevel = PlayerPrefs.GetInt("SkillLevel");
        _stockfish = new Stockfish.NET.Core.Stockfish(pathToStockfish)
        {
            SkillLevel = 1
        };
    }

    private void OnMakeMove()
    {
        if ((Side)_board.Player != _player || _board.IsGameOver) return;
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
        _board.MakeMove(gameMove.StartPosition, gameMove.EndPosition);
    }

    private string CalculateBestMove()
    {
        _stockfish.SetFenPosition(_board.GetFEN());
        return _stockfish.GetBestMove();
    }
}