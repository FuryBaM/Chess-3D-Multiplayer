using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
public sealed class Board : MonoBehaviour
{
    private Piece[,] _board = new Piece[8, 8];
    public Piece[,] GameBoard 
    {
        get 
        {
            return _board.Clone() as Piece[,]; 
        } 
    }
    private int _currentPlayer = 0;
    public int Player
    {
        get 
        {
            return _currentPlayer;
        }
    }
    private int _currentMove = 1;
    private HashSet<Piece> _movedPieces = new HashSet<Piece>();
    public IList<Piece> MovedPieces
    {
        get 
        {
            return _movedPieces.ToList();
        }
    }
    private List<Move> _movesHistory = new List<Move>();
    private bool _isGameOver = false;
    public bool IsGameOver
    {
        get
        {
            return _isGameOver;
        }
    }

    public UnityEvent OnCheck;
    public UnityEvent OnMate;
    public UnityEvent OnStalemate;
    public UnityEvent<Piece> OnCapture;
    public UnityEvent OnMakeMove;
    [Header("Piece Movement")]
    [SerializeField] private float _moveDuration = 1f;
    [SerializeField] private AnimationCurve _pieceMovementCurve;
    [SerializeField] private GameObject _highlightPrefab;
    [SerializeField] private GameObject _hightlightCapturablePrefab;
    private List<GameObject> _highlights = new List<GameObject>();
    [Header("Piece Prefabs")]
    [SerializeField] Piece whiteKing;
    [SerializeField] Piece whitePawn;
    [SerializeField] Piece whiteKnight;
    [SerializeField] Piece whiteBishop;
    [SerializeField] Piece whiteRook;
    [SerializeField] Piece whiteQueen;
    [SerializeField] Piece blackKing;
    [SerializeField] Piece blackPawn;
    [SerializeField] Piece blackKnight;
    [SerializeField] Piece blackBishop;
    [SerializeField] Piece blackRook;
    [SerializeField] Piece blackQueen;

    [Header("FEN Editor")]
    [SerializeField] private string fen;

    private void Start()
    {
        ImportFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        OnMakeMove.AddListener(OnMove);
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
                    Destroy(_board[i, j].gameObject);
                }
            }
        }
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
        if (!Application.isPlaying) return;
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
        Piece piece = null;
        switch (fenChar)
        {
            case 'K':
                piece = Instantiate(whiteKing);
                break;
            case 'P':
                piece = Instantiate(whitePawn);
                break;
            case 'N':
                piece = Instantiate(whiteKnight);
                break;
            case 'B':
                piece = Instantiate(whiteBishop);
                break;
            case 'R':
                piece = Instantiate(whiteRook);
                break;
            case 'Q':
                piece = Instantiate(whiteQueen);
                break;
            case 'k':
                piece = Instantiate(blackKing);
                break;
            case 'p':
                piece = Instantiate(blackPawn);
                break;
            case 'n':
                piece = Instantiate(blackKnight);
                break;
            case 'b':
                piece = Instantiate(blackBishop);
                break;
            case 'r':
                piece = Instantiate(blackRook);
                break;
            case 'q':
                piece = Instantiate(blackQueen);
                break;
            default:
                return piece;
        }
        return piece;
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
        if (moveString.Length != 4)
        {
            Debug.LogError("Invalid move string format. Must be in the format 'startEnd', e.g., 'd8g5'.");
            return null;
        }
        int startFile = moveString[0] - 'a';
        int startRank = int.Parse(moveString[1].ToString()) - 1;
        int endFile = moveString[2] - 'a';
        int endRank = int.Parse(moveString[3].ToString()) - 1;
        if (!IsPositionInBounds(new Vector2Int(startFile, startRank)) ||
            !IsPositionInBounds(new Vector2Int(endFile, endRank)))
        {
            Debug.LogError("Invalid move string. Positions are out of bounds.");
            return null;
        }
        Vector2Int startPosition = new Vector2Int(startFile, startRank);
        Vector2Int endPosition = new Vector2Int(endFile, endRank);
        return new Move(GetPieceAtPosition(startPosition), false, startPosition, endPosition);
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
    public bool MakeMove(Piece piece, Vector2Int startPosition, Vector2Int endPosition)
    {
        if (_isGameOver == true) return false;
        if (((int)piece.Side) != _currentPlayer)
        { 
            Debug.Log($"It's not your turn! Makes move {Enum.ToObject(typeof(Side), 1 - piece.Side).ToString()}");
            return false;
        }

        bool isCheck = IsKingInCheck((Side)_currentPlayer);

        bool canMove = false;
        if (piece.GetType() == typeof(King) && Mathf.Abs(endPosition.x - startPosition.x) > 1 && endPosition.y == startPosition.y && !isCheck)
        {
            canMove = Castle(endPosition.x > startPosition.x);
            if (canMove)
            {
                OnMakeMove?.Invoke();
                return true;
            }
        }
        else
        {
            Move lastMove = _movesHistory.Count > 0 ? _movesHistory[_movesHistory.Count - 1] : null;
            canMove = piece.MovePiece(startPosition, endPosition, this);
        }

        if (piece.GetType() == typeof(Pawn))
        {
            Move lastMove = _movesHistory.Count > 0 ? _movesHistory[_movesHistory.Count - 1] : null;
            if (_movesHistory.Count > 0 && piece.GetComponent<Pawn>().IsEnPassant(startPosition, endPosition, lastMove))
            {
                Vector2Int enPassantCapturePosition = lastMove.EndPosition;
                StartCoroutine(MovePieceSmoothly(piece, startPosition, endPosition, _moveDuration, _pieceMovementCurve));
                _movedPieces.Add(piece);
                OnCapture?.Invoke(_board[enPassantCapturePosition.y, enPassantCapturePosition.x]);
                Destroy(_board[enPassantCapturePosition.y, enPassantCapturePosition.x].gameObject);
                _board[enPassantCapturePosition.y, enPassantCapturePosition.x] = null;
                _board[endPosition.y, endPosition.x] = piece;
                _currentPlayer = 1 - _currentPlayer;
                _currentMove++;
                _movesHistory.Add(new Move(piece, false, startPosition, endPosition));
                OnMakeMove?.Invoke();
                return true;
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
                OnCapture?.Invoke(_board[endPosition.y, endPosition.x]);
                Destroy(_board[endPosition.y, endPosition.x].gameObject);
            }
            else
            {
                _board = cloneBoard;
            }
        }

        if (!canMove) return false;
        Piece[,] clone = GameBoard;
        _board[endPosition.y, endPosition.x] = piece;
        _board[startPosition.y, startPosition.x] = null;
        
        if (IsKingInCheck((Side)_currentPlayer))
        {
            _board = clone;
            return false;
        }

        _movedPieces.Add(piece);
        _currentPlayer = 1 - _currentPlayer;
        _currentMove++;
        _movesHistory.Add(new Move(piece, false, startPosition, endPosition));
        if (piece.GetType() == typeof(Pawn) && (endPosition.y == 0 || endPosition.y == 7))
        {
            char pieceChar = 'Q';
            piece = PromotePawn(endPosition, 1 - _currentPlayer == 0 ? char.ToUpper(pieceChar) : char.ToLower(pieceChar));
            OnMakeMove?.Invoke();
            return true;
        }
        StartCoroutine(MovePieceSmoothly(piece, startPosition, endPosition, _moveDuration, _pieceMovementCurve));
        OnMakeMove?.Invoke();
        return true;
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
                _isGameOver = true;
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
            _isGameOver = true;
            OnStalemate?.Invoke();
        }
    }

    private bool CanPieceCoverKing(Vector2Int kingPosition, Side side)
    {
        for (int i = 0; i < _board.GetLength(0); i++)
        {
            for (int j = 0; j < _board.GetLength(1); j++)
            {
                Piece piece = _board[i, j];
                if (piece != null && piece.Side == side)
                {
                    List<Vector2Int> possibleMoves = piece.GetPossibleMoves(new Vector2Int(j, i), this);
                    if (possibleMoves.Contains(kingPosition))
                    {
                        return true; 
                    }
                }
            }
        }
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

        _movedPieces.Add(king);
        _movedPieces.Add(rook);
        _board[rank, kingStartFile] = null;
        _board[rank, rookStartFile] = null;
        _board[rank, kingEndFile] = king;
        _board[rank, rookEndFile] = rook;
        _currentPlayer = 1 - _currentPlayer;
        _currentMove++;
        _movesHistory.Add(new Move(king, true, new Vector2Int(kingStartFile, rank), new Vector2Int(kingEndFile, rank)));
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

    private void OnDisable()
    {
        OnCheck.RemoveAllListeners();
        OnMate.RemoveAllListeners();
        OnStalemate.RemoveAllListeners();
    }

    // [CustomEditor(typeof(Board))]
    // public class customButton : Editor
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         DrawDefaultInspector();

    //         Board myScript = (Board)target;
    //         if (GUILayout.Button("Import FEN"))
    //         {
    //             myScript.ImportFEN(myScript.fen);
    //         }
    //     }
    // }
}

