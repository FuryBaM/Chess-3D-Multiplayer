using System.Collections;
using UnityEngine;
using System.Threading.Tasks;
using Stockfish.NET;
using Mirror;

public class AIController : NetworkBehaviour
{
    [SyncVar]
    [SerializeField] private Side _player = Side.black;
    [SerializeField] private Board _board;
    private IStockfish _stockfish;

    private void Start()
    {
        InitStockfish();
        _board = FindObjectOfType<Board>();
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
        MovePiece(gameMove.StartPosition, gameMove.EndPosition);
    }
    [TargetRpc]
    public void OnSideChanged(Side newSide)
    {
        print($"Side changed {newSide}");
    }
    [Server]
    public void SetPlayerSide(Side side)
    {
        _player = side;
        OnSideChanged(side);
    }
    [Server]
    public void MovePiece(Vector2Int startPosition, Vector2Int endPosition)
    {
        Debug.Log("Called move");
        _board.MakeMove(startPosition, endPosition);
    }

    private string CalculateBestMove()
    {
        _stockfish.SetFenPosition(_board.GetFEN());
        return _stockfish.GetBestMove();
    }
}