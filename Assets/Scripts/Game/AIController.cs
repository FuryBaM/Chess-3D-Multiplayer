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
    public char PromotionPiece { get; private set; } = 'q';

    private void Start()
    {
        if (!isServer) return;
        MessageClient("Im server");
        _board = FindObjectOfType<Board>();
        _board.OnMakeMove.AddListener(OnMakeMove);
        _board.OnCastle.AddListener(OnMakeMove);
        _board.OnPromotion.AddListener(OnMakeMove);
    }
    [ClientRpc]
    private void MessageClient(string message)
    {
        print(message);
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
        if (!PlayerPrefs.HasKey("Depth"))
        {
            PlayerPrefs.SetInt("Depth", 5);
        }
        string pathToStockfish = PlayerPrefs.GetString("StockfishPath");
        int stockfishSkillLevel = PlayerPrefs.GetInt("SkillLevel");
        int depth = PlayerPrefs.GetInt("Depth");
        _stockfish = new Stockfish.NET.Core.Stockfish(pathToStockfish)
        {
            SkillLevel = stockfishSkillLevel,
            Depth = depth
        };
    }

    private void OnMakeMove()
    {
        MakeMove();
    }
    [Server]
    public void MakeMove()
    {
        if (!isServer) return;
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
        Move gameMove = MoveConverter.ConvertStringToMove(_board, moveString);
        PromotionPiece = (char)gameMove.PromotionPiece;
        PromotionPiece = _player == Side.white ? char.ToUpper(PromotionPiece) : char.ToLower(PromotionPiece);
        MovePiece(gameMove.StartPosition, gameMove.EndPosition);
    }
    public void SetPlayerSide(Side side)
    {
        _player = side;
    }
    [Server]
    public void MovePiece(Vector2Int startPosition, Vector2Int endPosition)
    {
        Debug.Log("Called move ai");
        _board.MakeMove(startPosition, endPosition);
    }

    private string CalculateBestMove()
    {
        _stockfish.SetFenPosition(_board.GetFEN());
        return _stockfish.GetBestMove();
    }
    private void OnEnable()
    {
        InitStockfish();
    }
    private void OnDisable()
    {
        _stockfish.CloseProcess();
    }
}