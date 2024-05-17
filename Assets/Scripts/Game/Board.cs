using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
public sealed class Board : NetworkBehaviour
{
    private SyncList<Piece> _board = new SyncList<Piece>();
    public SyncList<Piece> GameBoard 
    {
        get 
        {
            return new SyncList<Piece>(_board);
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
    private SyncHashSet<Piece> _movedPieces = new SyncHashSet<Piece>();
    public IList<Piece> MovedPieces
    {
        get 
        {
            return _movedPieces.ToList();
        }
    }
    private List<Move> _movesHistory = new List<Move>();
    public IList<Move> MovesHistory
    {
        get
        {
            return _movesHistory;
        }
    }
    private SyncDictionary<Side, List<Piece>> _capturedPieces = new SyncDictionary<Side, List<Piece>>
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
    public UnityEvent<Piece> OnCapture;
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
    private void Start()
    {
        OnMakeMove.AddListener(OnMove);
        OnCastle.AddListener(OnMove);
        OnPromotion.AddListener(OnMove);
        if (isServer)
        {
            InitializeBoard();
        }
    }
    private void InitializeBoard()
    {
        for (int i = 0; i < 64; i++)
        {
            _board.Add(null);
        }
    }
    public void Set(int x, int y, Piece piece)
    {
        int index = y * 8 + x;
        if (index >= 0 && index < _board.Count)
        {
            _board[index] = piece;
        }
        else
        {
            Debug.LogError("Index out of range.");
        }
    }

    public Piece Get(int x, int y)
    {
        int index = y * 8 + x;
        if (index >= 0 && index < _board.Count)
        {
            return _board[index];
        }
        else
        {
            Debug.LogError("Index out of range.");
            return null;
        }
    }

    public static bool IsPositionInBounds(Vector2 position) => !(position.x < 0 || position.x >= 8 || position.y < 0 || position.y >= 8);
    public void ClearBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (Get(j, i) != null)
                {
                    Destroy(Get(j, i).gameObject);
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
                if (Get(x, y)!= null)
                {
                    boardState[y, x] = Get(x, y).name;
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
        print(_board.Count);
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
        for(int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (GameObject.ReferenceEquals(Get(j, i), piece))
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
        return Get(position.x, position.y);
    }
    [ClientRpc]
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
                    Set(x, y, piece);
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
                Piece piece = Get(file, rank);

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
    [ClientRpc]
    public void MakeMove(Vector2Int startPosition, Vector2Int endPosition)
    {
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
                _movedPieces.Add(piece);
                Get(enPassantCapturePosition.x, enPassantCapturePosition.y).gameObject.SetActive(false);
                _capturedPieces[(Side)_currentPlayer].Add(Get(enPassantCapturePosition.x, enPassantCapturePosition.y));
                Set(enPassantCapturePosition.x, enPassantCapturePosition.y, null);
                Set(endPosition.x, endPosition.y, piece);
                _currentPlayer = 1 - _currentPlayer;
                _currentMove++;
                _movesHistory.Add(new Move(piece, false, startPosition, endPosition));
                OnCapture?.Invoke(Get(enPassantCapturePosition.x, enPassantCapturePosition.y));
                return;
            }
        }

        if (!IsEmptyCell(endPosition) && piece.CanCapture(startPosition, endPosition, this) && Get(endPosition.x, endPosition.y).Side != (Side)_currentPlayer)
        {
            SyncList<Piece> cloneBoard = _board;
            Set(endPosition.x, endPosition.y, piece);
            Set(startPosition.x, startPosition.y, null);
            if (!IsKingInCheck((Side)_currentPlayer))
            {
                _board = cloneBoard;
                Get(endPosition.x, endPosition.y).gameObject.SetActive(false);
                _capturedPieces[(Side)_currentPlayer].Add(Get(endPosition.x, endPosition.y));
                OnCapture?.Invoke(Get(endPosition.x, endPosition.y));
            }
            else
            {
                _board = cloneBoard;
            }
        }

        if (!canMove) return;
        SyncList<Piece> clone = GameBoard;
        Set(endPosition.x, endPosition.y, piece);
        Set(startPosition.x, startPosition.y, null);
        
        if (IsKingInCheck((Side)_currentPlayer))
        {
            _board = clone;
            return;
        }

        _movedPieces.Add(piece);
        _currentPlayer = 1 - _currentPlayer;
        _currentMove++;
        
        if (piece is Pawn && (endPosition.y == 0 || endPosition.y == 7))
        {
            char pieceChar = 'Q';
            piece = PromotePawn(endPosition, 1 - _currentPlayer == 0 ? char.ToUpper(pieceChar) : char.ToLower(pieceChar));
            _movesHistory.Add(new Move(piece, false, startPosition, endPosition, true));
            OnPromotion?.Invoke();
            return;
        }
        else
        {
            _movesHistory.Add(new Move(piece, false, startPosition, endPosition));
        }
        StartCoroutine(MovePieceSmoothly(piece, startPosition, endPosition, _moveDuration, _pieceMovementCurve));
        OnMakeMove?.Invoke();
    }

    #endregion
    private void OnMove()
    {
        bool isCheck = IsKingInCheck((Side)_currentPlayer);
        Piece king = FindKing((Side)_currentPlayer);
        Vector2Int kingPosition = GetPiecePosition(king);
        if (isCheck && king.GetPossibleMoves(GetPiecePosition(king), this).Count == 0)
        {
            if (!CanPieceCoverKing(kingPosition, king.Side))
            {
                IsGameOver = true;
                OnMate?.Invoke();
            }
            else
            {
                OnCheck?.Invoke();
            }
        }
        else if (isCheck)
        {
            OnCheck?.Invoke();
        }
        else if (!isCheck && AllPiecesHaveNoMoves((Side)_currentPlayer))
        {
            IsGameOver = true;
            OnStalemate?.Invoke();
        }
    }

    private bool CanPieceCoverKing(Vector2Int kingPosition, Side side)
    {
        SyncList<Piece> originalBoard = GameBoard;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piece piece = Get(j, i);
                if (piece != null && piece.Side == side)
                {
                    List<Vector2Int> possibleMoves = piece.GetPossibleMoves(new Vector2Int(j, i), this);
                    foreach (Vector2Int move in possibleMoves)
                    {
                        if (!IsPositionInBounds(move)) continue;
                        SyncList<Piece> cloneBoard = GameBoard;
                        Set(move.x, move.y, piece);
                        Set(j, i, null);
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
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piece piece = Get(j, i);
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
        Set(position.x, position.y, newPiece);
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

        Piece king = Get(kingStartFile, rank);
        Piece rook = Get(rookStartFile, rank);
        if (king == null || rook == null || _movedPieces.Contains(king) || _movedPieces.Contains(rook))
        {
            //Cannot castle: king or rook has moved.
            return false;
        }
        for (int file = Mathf.Min(kingStartFile, rookStartFile) + 1; file < Mathf.Max(kingStartFile, rookStartFile); file++)
        {
            if (Get(file, rank) != null)
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

        _movedPieces.Add(king);
        _movedPieces.Add(rook);
        Set(kingStartFile, rank, null);
        Set(rookStartFile, rank, null);
        Set(kingEndFile, rank, king);
        Set(rookEndFile, rank, rook);
        _currentPlayer = 1 - _currentPlayer;
        _currentMove++;
        _movesHistory.Add(new Move(king, true, new Vector2Int(kingStartFile, rank), new Vector2Int(kingEndFile, rank)));
        return true;
    }
    #endregion
    public bool IsEmptyCell(Vector2Int position) => !Board.IsPositionInBounds(position) || Get(position.x, position.y) == null;
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
        Set(position.x, position.y, attackedPiece);
        Set(attackedPiecePosition.x, attackedPiecePosition.y, null);
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
                        Set(position.x, position.y, originalPiece);
                        Set(attackedPiecePosition.x, attackedPiecePosition.y, attackedPiece);
                        return true;
                    }
                }
            }
        }
        Set(attackedPiecePosition.x, attackedPiecePosition.y, attackedPiece);
        Set(position.x, position.y, originalPiece);
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
                Piece piece = Get(j, i);
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

    private void OnDisable()
    {
        OnCheck.RemoveAllListeners();
        OnMate.RemoveAllListeners();
        OnStalemate.RemoveAllListeners();
    }
}

