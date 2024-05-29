using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
public sealed class Board : NetworkBehaviour
{
    public SyncList<int> boardState = new SyncList<int>();
    private Piece[,] _board = new Piece[8, 8];
    public Piece[,] GameBoard 
    {
        get 
        {
            return _board.Clone() as Piece[,]; 
        } 
    }
    [SyncVar]
    private int _currentPlayer = 0;
    public int Player
    {
        get
        {
            return _currentPlayer;
        }
    }
    [SyncVar]
    private int _currentMove = 1;
    public int CurrentMove
    {
        get
        {
            return _currentMove;
        }
    }
    public SyncHashSet<uint> movedPiecesData = new SyncHashSet<uint>();
    public HashSet<Piece> _movedPieces = new HashSet<Piece>();
    public IList<Piece> MovedPieces
    {
        get 
        {
            return _movedPieces.ToList();
        }
    }
    public SyncList<MoveData> movesHistoryData = new SyncList<MoveData>();
    private List<Move> _movesHistory = new List<Move>();
    public IList<Move> MovesHistory
    {
        get
        {
            return _movesHistory;
        }
    }
    public SyncList<uint> whiteCaptures = new SyncList<uint>();
    public SyncList<uint> blackCaptures = new SyncList<uint>();
    private Dictionary<Side, List<Piece>> _capturedPieces = new Dictionary<Side, List<Piece>>
    {
        {Side.white, new List<Piece>()},
        {Side.black, new List<Piece>()}
    };
    public IDictionary<Side, List<Piece>> CapturedPieces
    {
        get
        {
            return _capturedPieces;
        }
    }
    public bool IsGameOver
    {
        get;
        private set;
    }

    public UnityEvent OnCheck;
    public UnityEvent OnMate;
    public UnityEvent OnStalemate;
    public UnityEvent<uint> OnCapture;
    public UnityEvent OnMakeMove;
    public UnityEvent OnCastle;
    public UnityEvent OnPromotion;
    [Header("Piece Movement")]
    [SerializeField] private float _moveDuration = 1f;
    [SerializeField] private AnimationCurve _pieceMovementCurve;
    [SerializeField] private GameObject _highlightPrefab;
    [SerializeField] private GameObject _hightlightCapturablePrefab;
    private List<GameObject> _highlights = new List<GameObject>();
    [Header("Piece Prefabs")]
    [SerializeField] GameObject whiteKing;
    [SerializeField] GameObject whitePawn;
    [SerializeField] GameObject whiteKnight;
    [SerializeField] GameObject whiteBishop;
    [SerializeField] GameObject whiteRook;
    [SerializeField] GameObject whiteQueen;
    [SerializeField] GameObject blackKing;
    [SerializeField] GameObject blackPawn;
    [SerializeField] GameObject blackKnight;
    [SerializeField] GameObject blackBishop;
    [SerializeField] GameObject blackRook;
    [SerializeField] GameObject blackQueen;

    [Header("FEN Editor")]
    [SerializeField] private string fen;
    public override void OnStartClient()
    {
        base.OnStartClient();
        boardState.Callback += SyncBoardState;
        movesHistoryData.Callback += SyncMovesHistory;
        movedPiecesData.Callback += SyncMovedPieces;
        whiteCaptures.Callback += SyncWhiteCaptures;
        blackCaptures.Callback += SyncBlackCaptures;
    }

    private void Start()
    {
        InitBoard();
        OnMakeMove.AddListener(OnMove);
        OnCastle.AddListener(OnMove);
        OnPromotion.AddListener(OnMove);
    }
    private void OnDisable()
    {
        OnCheck.RemoveAllListeners();
        OnMate.RemoveAllListeners();
        OnStalemate.RemoveAllListeners();
        boardState.Callback -= SyncBoardState;
        movesHistoryData.Callback -= SyncMovesHistory;
        movedPiecesData.Callback -= SyncMovedPieces;
        whiteCaptures.Callback -= SyncWhiteCaptures;
        blackCaptures.Callback -= SyncBlackCaptures;
    }
    private void InitBoard()
    {
        if (!isServer) return;
        for (int i = 0; i < 64; i++)
        {
            boardState.Add(-1);
        }
    }
    [Server]
    public void Reset()
    {
        _currentPlayer = 0;
        movedPiecesData.Clear();
        movesHistoryData.Clear();
        _capturedPieces.Clear();
        _currentMove = 1;
        ClearBoard();
        ImportFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            LogBoardState();
            print(NetworkManager.singleton.numPlayers);
            print(_movedPieces.Count);
            print(_movesHistory.Count);
            print(GetLastMove());
        }
    }

    private void SyncWhiteCaptures(SyncList<uint>.Operation op, int itemIndex, uint oldItem, uint newItem)
    {
        switch(op)
        {
            case SyncList<uint>.Operation.OP_ADD:
            {
                Piece piece = NetworkClient.spawned[newItem].GetComponent<Piece>();
                if (_capturedPieces.Count == itemIndex)
                {
                    _capturedPieces[Side.white][itemIndex] = piece;
                }
                else
                {
                    _capturedPieces[Side.white].Add(piece);
                }
                break;
            }
        }
        switch(op)
        {
            case SyncList<uint>.Operation.OP_CLEAR:
            {
                _capturedPieces[Side.white].Clear();
                break;
            }
        }
        switch(op)
        {
            case SyncList<uint>.Operation.OP_INSERT:
            {
                Piece piece = NetworkClient.spawned[newItem].GetComponent<Piece>();
                _capturedPieces[Side.white].Insert(itemIndex, piece);
                break;
            }
        }
        switch(op)
        {
            case SyncList<uint>.Operation.OP_REMOVEAT:
            {
                _capturedPieces[Side.white].RemoveAt(itemIndex);
                break;
            }
        }
        switch(op)
        {
            case SyncList<uint>.Operation.OP_SET:
            {
                Piece piece = NetworkClient.spawned[newItem].GetComponent<Piece>();
                _capturedPieces[Side.white][itemIndex] = piece;
                break;
            }
        }
    }
    private void SyncBlackCaptures(SyncList<uint>.Operation op, int itemIndex, uint oldItem, uint newItem)
    {
        switch(op)
        {
            case SyncList<uint>.Operation.OP_ADD:
            {
                Piece piece = NetworkClient.spawned[newItem].GetComponent<Piece>();
                _capturedPieces[Side.black].Add(piece);
                break;
            }
        }
        switch(op)
        {
            case SyncList<uint>.Operation.OP_CLEAR:
            {
                _capturedPieces[Side.black].Clear();
                break;
            }
        }
        switch(op)
        {
            case SyncList<uint>.Operation.OP_INSERT:
            {
                Piece piece = NetworkClient.spawned[newItem].GetComponent<Piece>();
                _capturedPieces[Side.black].Insert(itemIndex, piece);
                break;
            }
        }
        switch(op)
        {
            case SyncList<uint>.Operation.OP_REMOVEAT:
            {
                _capturedPieces[Side.black].RemoveAt(itemIndex);
                break;
            }
        }
        switch(op)
        {
            case SyncList<uint>.Operation.OP_SET:
            {
                Piece piece = NetworkClient.spawned[newItem].GetComponent<Piece>();
                _capturedPieces[Side.black][itemIndex] = piece;
                break;
            }
        }
    }

    private void SyncMovedPieces(SyncSet<uint>.Operation op, uint item)
    {
        switch (op)
        {
            case SyncSet<uint>.Operation.OP_ADD:
            {
                Piece piece = NetworkClient.spawned[item].GetComponent<Piece>();
                _movedPieces.Add(piece);
                break;
            }
            case SyncSet<uint>.Operation.OP_CLEAR:
            {
                _movedPieces.Clear();
                break;
            }
            case SyncSet<uint>.Operation.OP_REMOVE:
            {
                Piece piece = NetworkClient.spawned[item].GetComponent<Piece>();
                _movedPieces.Remove(piece);
                break;
            }
        }
    }

    private void SyncMovesHistory(SyncList<MoveData>.Operation op, int itemIndex, MoveData oldItem, MoveData newItem)
    {
        switch (op)
        {
            case SyncList<MoveData>.Operation.OP_ADD:
            {
                Piece piece = NetworkClient.spawned[newItem.MovedPieceId].GetComponent<Piece>();
                _movesHistory.Add(new Move(piece, newItem.Castle, newItem.StartPosition, newItem.EndPosition, newItem.Promotion));
                break;
            }
            case SyncList<MoveData>.Operation.OP_INSERT:
            {
                Piece piece = NetworkClient.spawned[newItem.MovedPieceId].GetComponent<Piece>();
                _movesHistory.Insert(itemIndex, new Move(piece, newItem.Castle, newItem.StartPosition, newItem.EndPosition, newItem.Promotion));
                break;
            }
            case SyncList<MoveData>.Operation.OP_CLEAR:
            {
                _movesHistory.Clear();
                break;
            }
            case SyncList<MoveData>.Operation.OP_REMOVEAT:
            {
                _movesHistory.RemoveAt(itemIndex);
                break;
            }
            case SyncList<MoveData>.Operation.OP_SET:
            {
                _movesHistory[itemIndex].MovedPiece = NetworkClient.spawned[newItem.MovedPieceId].GetComponent<Piece>();
                _movesHistory[itemIndex].Castle = newItem.Castle;
                _movesHistory[itemIndex].StartPosition = newItem.StartPosition;
                _movesHistory[itemIndex].EndPosition = newItem.EndPosition;
                _movesHistory[itemIndex].Promotion = newItem.Promotion;
                break;
            }
            default:
            {
                break;
            }
        }
    }
    private void SyncBoardState(SyncList<int>.Operation op, int index, int oldItem, int newItem)
    {
        switch (op)
        {
            case SyncList<int>.Operation.OP_SET:
            {
                int netId = newItem;
                int x = index % 8;
                int y = index / 8;

                if (netId == -1)
                {
                    _board[y, x] = null;
                }
                else
                {
                    var piece = NetworkClient.spawned[(uint)netId].GetComponent<Piece>();
                    _board[y, x] = piece;
                }
                break;
            }
            default:
            {
                break;
            }
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdSyncBoard()
    {
        SyncBoard();
    }
    [Server]
    public void SyncBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (_board[i, j] == null)
                {
                    boardState[i * 8 + j] = -1;
                }
                else
                {
                    boardState[i * 8 + j] = (int)_board[i, j].GetComponent<NetworkIdentity>().netId;
                }
            }
        }
        
    }
    [ClientRpc]
    public void SyncData(int currentPlayer, int currentMove, List<MoveData> moveHistoryData, List<uint> movedPieces, List<uint> whiteCaptures, List<uint> blackCaptures)
    {
        _currentPlayer = currentPlayer;
        _currentMove = currentMove;
        _movesHistory.Clear();
        foreach(var move in moveHistoryData)
        {
            _movesHistory.Add(new Move(NetworkClient.spawned[move.MovedPieceId].GetComponent<Piece>(), move.Castle, move.StartPosition, move.EndPosition, move.Promotion));
        }
        _movedPieces.Clear();
        foreach(var pieceId in movedPieces)
        {
            _movedPieces.Add(NetworkClient.spawned[pieceId].GetComponent<Piece>());
        }
        _capturedPieces[Side.white].Clear();
        foreach(var pieceId in whiteCaptures)
        {
            _capturedPieces[Side.white].Add(NetworkClient.spawned[pieceId].GetComponent<Piece>());
        }
        _capturedPieces[Side.white].Clear();
        foreach(var pieceId in blackCaptures)
        {
            _capturedPieces[Side.black].Add(NetworkClient.spawned[pieceId].GetComponent<Piece>());
        }
    }
    public static bool IsPositionInBounds(Vector2 position) => !(position.x < 0 || position.x >= 8 || position.y < 0 || position.y >= 8);
    public void ClearBoard()
    {
        for (int i = 0; i < _board.GetLength(0); i++)
        {
            for (int j = 0; j < _board.GetLength(1); j++)
            {
                if (_board[i, j] != null)
                {
                    NetworkServer.Destroy(_board[i, j].gameObject);
                    Destroy(_board[i, j].gameObject);
                }
            }
        }
    }
    public void LogBoardState()
    {
        string[,] boardState = new string[8, 8];

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (_board[y, x] != null)
                {
                    boardState[y, x] = _board[y, x].name;
                }
                else
                {
                    boardState[y, x] = "Empty";
                }
            }
        }

        string logMessage = "Board State:\n";
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                logMessage += boardState[y, x] + " ";
            }
            logMessage += "\n";
        }

        Debug.Log(logMessage);
    }

    public void HighlightCell(Vector2Int position, bool selfPiece)
    {
        GameObject highlight;
        if (selfPiece)
        {
            highlight = Instantiate(_hightlightCapturablePrefab, new Vector3(position.x, 0, position.y), Quaternion.AngleAxis(90, Vector3.right));
        }
        else
        {
            highlight = Instantiate(_highlightPrefab, new Vector3(position.x, 0, position.y), Quaternion.AngleAxis(90, Vector3.right));
        }
        _highlights.Add(highlight);
    }

    public void UnhighlightAllCells()
    {
        foreach (GameObject highlight in _highlights)
        {
            Destroy(highlight);
        }
    }
    public Move GetLastMove()
    {
        Move lastMove = _movesHistory.Count > 0 ? _movesHistory[_movesHistory.Count - 1] : null;
        return lastMove;
    }
    public Vector2Int GetPiecePosition(Piece piece)
    {
        Vector2Int position = new Vector2Int(-1, -1);
        if (!piece) return position;
        for(int i = 0; i < _board.GetLength(0); i++)
        {
            for (int j = 0; j < _board.GetLength(0); j++)
            {
                if (GameObject.ReferenceEquals(_board[i, j], piece))
                {
                    position = new Vector2Int(j, i);
                }
            }
        }
        return position;
    }

    public Piece GetPieceAtPosition(Vector2Int position)
    {
        if (!IsPositionInBounds(position)) return null;
        return _board[position.y, position.x];
    }
    public void ImportFEN(string fenString)
    {
        if (!Application.isPlaying || !isServer) return;
        ClearBoard();
        string[] parts = fenString.Split(' ');
        string[] rows = parts[0].Split('/');

        for (int y = 0; y < 8; y++)
        {
            int x = 0;
            foreach (char c in rows[7 - y])
            {
                if (char.IsDigit(c))
                {
                    int offset = (int)char.GetNumericValue(c);
                    x += offset;
                }
                else
                {
                    Piece piece = SpawnPieceByChar(c);
                    Vector3 position = new Vector3(x, 0, y);
                    piece.transform.position = position;
                    _board[y, x] = piece;
                    x++;
                }
            }
        }
    }
    private Piece SpawnPieceByChar(char fenChar)
    {
        GameObject prefab = null;
        switch (fenChar)
        {
            case 'K':
                prefab = whiteKing;
                break;
            case 'P':
                prefab = whitePawn;
                break;
            case 'N':
                prefab = whiteKnight;
                break;
            case 'B':
                prefab = whiteBishop;
                break;
            case 'R':
                prefab = whiteRook;
                break;
            case 'Q':
                prefab = whiteQueen;
                break;
            case 'k':
                prefab = blackKing;
                break;
            case 'p':
                prefab = blackPawn;
                break;
            case 'n':
                prefab = blackKnight;
                break;
            case 'b':
                prefab = blackBishop;
                break;
            case 'r':
                prefab = blackRook;
                break;
            case 'q':
                prefab = blackQueen;
                break;
            default:
                return null;
        }

        GameObject pieceObject = Instantiate(prefab);
        NetworkServer.Spawn(pieceObject);
        return pieceObject.GetComponent<Piece>();
    }

    private const string FEN_PIECE_MAPPING = "KQRBNPkqrbnp";

    public string GetFEN()
    {
        string fen = "";
        for (int rank = 7; rank >= 0; rank--)
        {
            int emptySquaresCount = 0;
            for (int file = 0; file < 8; file++)
            {
                Piece piece = _board[rank, file];

                if (piece == null)
                {
                    emptySquaresCount++;
                }
                else
                {
                    if (emptySquaresCount > 0)
                    {
                        fen += emptySquaresCount;
                        emptySquaresCount = 0;
                    }
                    char pieceChar = FEN_PIECE_MAPPING[(int)piece.Side * 6 + GetPieceIndex(piece)];
                    fen += pieceChar;
                }
            }
            if (emptySquaresCount > 0)
            {
                fen += emptySquaresCount;
            }
            if (rank > 0)
            {
                fen += "/";
            }
        }

        return fen;
    }

    public Move ConvertStringToMove(string moveString)
    {
        if (moveString.Length != 4 && moveString.Length != 5)
        {
            Debug.LogError("Invalid move string format. Must be in the format 'startEnd', e.g., 'd8g5'. " + moveString);
            return null;
        }
        else if (moveString == null)
        {
            Debug.LogError("Null string" + moveString);
            return null;
        }

        int startFile = moveString[0] - 'a';
        int startRank = int.Parse(moveString[1].ToString()) - 1;
        int endFile = moveString[2] - 'a';
        int endRank = int.Parse(moveString[3].ToString()) - 1;
        bool promotion = moveString.Length == 5;

        if (!IsPositionInBounds(new Vector2Int(startFile, startRank)) ||
            !IsPositionInBounds(new Vector2Int(endFile, endRank)))
        {
            Debug.LogError("Invalid move string. Positions are out of bounds.");
            return null;
        }

        Vector2Int startPosition = new Vector2Int(startFile, startRank);
        Vector2Int endPosition = new Vector2Int(endFile, endRank);
        return new Move(GetPieceAtPosition(startPosition), false, startPosition, endPosition, promotion);
    }
    private int GetPieceIndex(Piece piece)
    {
        if (piece is King) return 0;
        if (piece is Queen) return 1;
        if (piece is Rook) return 2;
        if (piece is Bishop) return 3;
        if (piece is Knight) return 4;
        if (piece is Pawn) return 5;
        return -1;
    }


    public IEnumerator MovePieceSmoothly(Piece piece, Vector2Int startPosition, Vector2Int endPosition, float moveDuration, AnimationCurve curve)
    {
        Vector3 start = new Vector3(startPosition.x, 0, startPosition.y);
        Vector3 end = new Vector3(endPosition.x, 0, endPosition.y);
        float timeElapsed = 0f;

        while (timeElapsed < moveDuration)
        {
            float t = timeElapsed / moveDuration;
            piece.transform.position = Vector3.Lerp(start, end, curve.Evaluate(t));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        piece.transform.position = end;
    }

    #region MakeMove
    public void MakeMove(Vector2Int startPosition, Vector2Int endPosition)
    {
        SyncBoard();
        SyncData(_currentPlayer, _currentMove, movesHistoryData.ToList(), movedPiecesData.ToList(), whiteCaptures.ToList(), blackCaptures.ToList());
        Piece piece = GetPieceAtPosition(startPosition);
        if (IsGameOver == true) return;
        if (((int)piece.Side) != _currentPlayer)
        { 
            Debug.Log($"It's not your turn! Makes move {Enum.ToObject(typeof(Side), 1 - piece.Side).ToString()}");
            return;
        }

        bool isCheck = IsKingInCheck((Side)_currentPlayer);

        bool canMove = false;
        if (piece is King && Mathf.Abs(endPosition.x - startPosition.x) > 1 && endPosition.y == startPosition.y && !isCheck)
        {
            canMove = Castle(endPosition.x > startPosition.x);
            if (canMove)
            {
                OnCastle?.Invoke();
                SyncBoard();
                SyncData(_currentPlayer, _currentMove, movesHistoryData.ToList(), movedPiecesData.ToList(), whiteCaptures.ToList(), blackCaptures.ToList());
                RpcCastleInvoke();
                return;
            }
        }
        else
        {
            Move lastMove = _movesHistory.Count > 0 ? _movesHistory[_movesHistory.Count - 1] : null;
            canMove = piece.MovePiece(startPosition, endPosition, this);
        }

        if (piece is Pawn)
        {
            Move lastMove = _movesHistory.Count > 0 ? _movesHistory[_movesHistory.Count - 1] : null;
            if (_movesHistory.Count > 0 && piece.GetComponent<Pawn>().IsEnPassant(startPosition, endPosition, lastMove))
            {
                Vector2Int enPassantCapturePosition = lastMove.EndPosition;
                StartCoroutine(MovePieceSmoothly(piece, startPosition, endPosition, _moveDuration, _pieceMovementCurve));
                movedPiecesData.Add(piece.netId);
                uint capturedPieceId = _board[enPassantCapturePosition.y, enPassantCapturePosition.x].GetComponent<NetworkIdentity>().netId;
                _board[enPassantCapturePosition.y, enPassantCapturePosition.x].gameObject.SetActive(false);
                RpcDeactivatePiece(_board[enPassantCapturePosition.y, enPassantCapturePosition.x].GetComponent<NetworkIdentity>().netId);
                if (_currentPlayer == (int)Side.white)
                {
                    whiteCaptures.Add(_board[enPassantCapturePosition.y, enPassantCapturePosition.x].netId);
                    //_capturedPieces[(Side)_currentPlayer].Add(_board[enPassantCapturePosition.y, enPassantCapturePosition.x]);
                }
                else
                {
                    blackCaptures.Add(_board[enPassantCapturePosition.y, enPassantCapturePosition.x].netId);
                }
                _board[enPassantCapturePosition.y, enPassantCapturePosition.x] = null;
                _board[endPosition.y, endPosition.x] = piece;
                _currentPlayer = 1 - _currentPlayer;
                _currentMove++;
                movesHistoryData.Add(new MoveData(piece.netId, false, startPosition, endPosition));
                OnCapture?.Invoke(capturedPieceId);
                SyncBoard();
                SyncData(_currentPlayer, _currentMove, movesHistoryData.ToList(), movedPiecesData.ToList(), whiteCaptures.ToList(), blackCaptures.ToList());
                RpcCaptureInvoke(capturedPieceId);
                return;
            }
        }

        if (!IsEmptyCell(endPosition) && piece.CanCapture(startPosition, endPosition, this) && _board[endPosition.y, endPosition.x].Side != (Side)_currentPlayer)
        {
            Piece[,] cloneBoard = GameBoard;
            _board[endPosition.y, endPosition.x] = piece;
            _board[startPosition.y, startPosition.x] = null;
            if (!IsKingInCheck((Side)_currentPlayer))
            {
                _board = cloneBoard;
                uint pieceId = _board[endPosition.y, endPosition.x].GetComponent<NetworkIdentity>().netId;
                _board[endPosition.y, endPosition.x].gameObject.SetActive(false);
                RpcDeactivatePiece(pieceId);
                if (_currentPlayer == (int)Side.white)
                {
                    whiteCaptures.Add(_board[endPosition.y, endPosition.x].netId);
                }
                else
                {
                    blackCaptures.Add(_board[endPosition.y, endPosition.x].netId);
                }
                // _capturedPieces[(Side)_currentPlayer].Add(_board[endPosition.y, endPosition.x]);
                OnCapture?.Invoke(pieceId);
                SyncBoard();
                SyncData(_currentPlayer, _currentMove, movesHistoryData.ToList(), movedPiecesData.ToList(), whiteCaptures.ToList(), blackCaptures.ToList());
                RpcCaptureInvoke(pieceId);
            }
            else
            {
                _board = cloneBoard;
            }
        }

        if (!canMove) return;
        Piece[,] clone = GameBoard;
        _board[endPosition.y, endPosition.x] = piece;
        _board[startPosition.y, startPosition.x] = null;
        
        if (IsKingInCheck((Side)_currentPlayer))
        {
            _board = clone;
            return;
        }

        movedPiecesData.Add(piece.netId);
        _currentPlayer = 1 - _currentPlayer;
        _currentMove++;
        
        if (piece is Pawn && (endPosition.y == 0 || endPosition.y == 7))
        {
            char pieceChar = 'Q';
            piece = PromotePawn(endPosition, 1 - _currentPlayer == 0 ? char.ToUpper(pieceChar) : char.ToLower(pieceChar));
            _movesHistory.Add(new Move(piece, false, startPosition, endPosition, true));
            movesHistoryData.Add(new MoveData(piece.netId, false, startPosition, endPosition, true));
            OnPromotion?.Invoke();
            SyncBoard();
            SyncData(_currentPlayer, _currentMove, movesHistoryData.ToList(), movedPiecesData.ToList(), whiteCaptures.ToList(), blackCaptures.ToList());
            RpcPromotionInvoke();
            return;
        }
        else
        {
            movesHistoryData.Add(new MoveData(piece.netId, false, startPosition, endPosition));
        }
        StartCoroutine(MovePieceSmoothly(piece, startPosition, endPosition, _moveDuration, _pieceMovementCurve));
        OnMakeMove?.Invoke();
        SyncBoard();
        SyncData(_currentPlayer, _currentMove, movesHistoryData.ToList(), movedPiecesData.ToList(), whiteCaptures.ToList(), blackCaptures.ToList());
        RpcMoveInvoke();
    }

    #endregion
    [ClientRpc]
    private void RpcActivatePiece(uint pieceId)
    {
        NetworkClient.spawned[pieceId].gameObject.SetActive(true);
    }
    [ClientRpc]
    private void RpcDeactivatePiece(uint pieceId)
    {
        NetworkClient.spawned[pieceId].gameObject.SetActive(false);
    }
    [ClientRpc]
    private void RpcMoveInvoke()
    {
        OnMakeMove?.Invoke();
    }
    [ClientRpc]
    private void RpcCaptureInvoke(uint pieceId)
    {
        OnCapture?.Invoke(pieceId);
    }
    [ClientRpc]
    private void RpcCastleInvoke()
    {
        OnCastle?.Invoke();
    }
    [ClientRpc]
    private void RpcMateInvoke()
    {
        OnMate?.Invoke();
    }
    [ClientRpc]
    private void RpcStalemateInvoke()
    {
        OnStalemate?.Invoke();
    }
    [ClientRpc]
    private void RpcCheckInvoke()
    {
        OnCheck?.Invoke();
    }
    [ClientRpc]
    private void RpcPromotionInvoke()
    {
        OnPromotion?.Invoke();
    }
    private void OnMove()
    {
        if (!isServer) return;
        bool isCheck = IsKingInCheck((Side)_currentPlayer);
        Piece king = FindKing((Side)_currentPlayer);
        Vector2Int kingPosition = GetPiecePosition(king);
        if (isCheck && king.GetPossibleMoves(GetPiecePosition(king), this).Count == 0)
        {
            if (!CanPieceCoverKing(kingPosition, king.Side))
            {
                IsGameOver = true;
                OnMate?.Invoke();
                RpcMateInvoke();
            }
            else
            {
                OnCheck?.Invoke();
                RpcCheckInvoke();
            }
        }
        else if (isCheck)
        {
            OnCheck?.Invoke();
            RpcCheckInvoke();
        }
        else if (!isCheck && AllPiecesHaveNoMoves((Side)_currentPlayer))
        {
            IsGameOver = true;
            OnStalemate?.Invoke();
            RpcStalemateInvoke();
        }
    }

    private bool CanPieceCoverKing(Vector2Int kingPosition, Side side)
    {
        Piece[,] originalBoard = GameBoard;
        for (int i = 0; i < _board.GetLength(0); i++)
        {
            for (int j = 0; j < _board.GetLength(1); j++)
            {
                Piece piece = _board[i, j];
                if (piece != null && piece.Side == side)
                {
                    List<Vector2Int> possibleMoves = piece.GetPossibleMoves(new Vector2Int(j, i), this);
                    foreach (Vector2Int move in possibleMoves)
                    {
                        if (!IsPositionInBounds(move)) continue;
                        Piece[,] cloneBoard = GameBoard;
                        _board[move.y, move.x] = piece;
                        _board[i, j] = null;
                        if (!IsKingInCheck(side))
                        {
                            _board = originalBoard;
                            return true;
                        }
                        _board = cloneBoard;
                    }
                }
            }
        }
        _board = originalBoard;
        return false;
    }


    private bool AllPiecesHaveNoMoves(Side currentPlayer)
    {
        for (int i = 0; i < _board.GetLength(0); i++)
        {
            for (int j = 0; j < _board.GetLength(1); j++)
            {
                Piece piece = _board[i, j];
                if (piece != null && piece.Side == currentPlayer)
                {
                    List<Vector2Int> possibleMoves = piece.GetPossibleMoves(new Vector2Int(j, i), this);
                    if (possibleMoves.Count > 0)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    #region Pawn Promotion
    public Piece PromotePawn(Vector2Int position, char newPieceChar = 'q')
    {
        Piece pawn = GetPieceAtPosition(position);
        if (pawn == null || !(pawn is Pawn))
        {
            Debug.LogError("No pawn found at the specified position.");
            return null;
        }

        Piece newPiece = SpawnPieceByChar(newPieceChar);
        if (newPiece == null)
        {
            Debug.LogError("Invalid piece character.");
            return null;
        }

        _board[position.y, position.x] = newPiece;
        Destroy(pawn.gameObject);
        newPiece.transform.position = new Vector3(position.x, 0, position.y);
        return newPiece;
    }
    #endregion
    #region Castle
    public bool Castle(bool isShortCastle)
    {
        int rank = _currentPlayer == 0 ? 0 : 7;
        int kingStartFile = 4;
        int rookStartFile = isShortCastle ? 7 : 0;
        int kingEndFile = isShortCastle ? 6 : 2;
        int rookEndFile = isShortCastle ? 5 : 3;

        Piece king = _board[rank, kingStartFile];
        Piece rook = _board[rank, rookStartFile];
        if (king == null || rook == null || _movedPieces.Contains(king) || _movedPieces.Contains(rook))
        {
            //Cannot castle: king or rook has moved.
            return false;
        }
        for (int file = Mathf.Min(kingStartFile, rookStartFile) + 1; file < Mathf.Max(kingStartFile, rookStartFile); file++)
        {
            if (_board[rank, file] != null)
            {
                //Cannot castle: there are pieces between the king and rook.
                return false;
            }
        }
        Vector2Int kingStartPosition = new Vector2Int(kingStartFile, rank);
        Vector2Int kingEndPosition = new Vector2Int(kingEndFile, rank);

        if (IsAttackedCell(king, kingStartPosition) || IsAttackedCell(king, kingEndPosition))
        {
            //Cannot castle: king would move through or into an attacked square
            return false;
        }
        StartCoroutine(MovePieceSmoothly(king, kingStartPosition, kingEndPosition, _moveDuration, _pieceMovementCurve));
        StartCoroutine(MovePieceSmoothly(rook, new Vector2Int(rookStartFile, rank), new Vector2Int(rookEndFile, rank), _moveDuration, _pieceMovementCurve));

        movedPiecesData.Add(king.netId);
        movedPiecesData.Add(rook.netId);
        _board[rank, kingStartFile] = null;
        _board[rank, rookStartFile] = null;
        _board[rank, kingEndFile] = king;
        _board[rank, rookEndFile] = rook;
        _currentPlayer = 1 - _currentPlayer;
        _currentMove++;
        movesHistoryData.Add(new MoveData(king.netId, true, new Vector2Int(kingStartFile, rank), new Vector2Int(kingEndFile, rank)));
        return true;
    }
    #endregion
    public bool IsEmptyCell(Vector2Int position) => !Board.IsPositionInBounds(position) || _board[position.y, position.x] == null;
    #region  Check Attacked Cell
    public bool IsAttackedCell(Vector2Int position)
    {
        if (!IsPositionInBounds(position)) return false;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Vector2Int startPosition = new Vector2Int(j, i);
                if (position == startPosition) continue;
                if (IsEmptyCell(startPosition)) continue;
                Piece piece = GetPieceAtPosition(startPosition);
                if (piece.Side != (Side)_currentPlayer)
                {
                    if (piece.GetType() == typeof(King)) continue;
                    Move lastMove = GetLastMove();
                    bool canMove = piece.MovePiece(startPosition, position, this);
                    bool canCapture = piece.CanCapture(startPosition, position, this);
                    if (canMove && canCapture) // It is attacked cell
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool IsAttackedCell(Piece attackedPiece, Vector2Int position)
    {
        if (!IsPositionInBounds(position)) return false;
        Piece originalPiece = GetPieceAtPosition(position);
        Vector2Int attackedPiecePosition = GetPiecePosition(attackedPiece);
        _board[position.y, position.x] = attackedPiece;
        _board[attackedPiecePosition.y, attackedPiecePosition.x] = null;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Vector2Int startPosition = new Vector2Int(j, i);
                if (position == startPosition) continue;
                if (IsEmptyCell(startPosition)) continue;
                Piece piece = GetPieceAtPosition(startPosition);
                if (piece.Side != attackedPiece.Side)
                {
                    if (piece.GetType() == typeof(King)) continue;
                    Move lastMove = GetLastMove();
                    bool canMove = piece.MovePiece(startPosition, position, this);
                    bool canCapture = piece.CanCapture(startPosition, position, this);
                    if (canMove && canCapture) // It is attacked cell
                    {
                        _board[position.y, position.x] = originalPiece;
                        _board[attackedPiecePosition.y, attackedPiecePosition.x] = attackedPiece;
                        return true;
                    }
                }
            }
        }
        _board[attackedPiecePosition.y, attackedPiecePosition.x] = attackedPiece;
        _board[position.y, position.x] = originalPiece;
        return false;
    }
    #endregion
    private bool IsKingInCheck(Side side)
    {
        Piece king = FindKing(side);
        Vector2Int kingPosition = GetPiecePosition(king);
        return IsAttackedCell(king, kingPosition);
    }

    private Piece FindKing(Side side)
    {
        Piece king = null;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piece piece = _board[i, j];
                if (piece == null) continue;
                else if (piece != null && piece.Side == side && piece.GetType() == typeof(King))
                {
                    king = piece;
                    break;
                }
            }
        }
        return king;
    }
}

