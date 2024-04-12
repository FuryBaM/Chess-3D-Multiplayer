using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Board : MonoBehaviour
{
    private Piece[,] _board = new Piece[8, 8];
    public Piece[,] GameBoard {get { return _board; } }
    private int _currentPlayer = 0;
    private int _currentMove = 1;
    private HashSet<Piece> _movedPieces = new HashSet<Piece>();
    private List<Move> _movesHistory = new List<Move>();
    [Header("Piece Movement")]
    [SerializeField] private float _moveDuration = 1f;
    [SerializeField] private AnimationCurve _pieceMovementCurve;
    [SerializeField] private GameObject highlightPrefab;
    private List<GameObject> highlights = new List<GameObject>();
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
        //fenImport(fen);
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
    public void HighlightCell(Vector2Int position)
    {
        GameObject highlight = Instantiate(highlightPrefab, new Vector3(position.x, 0, position.y), new Quaternion(-90f, 0, 0, 0));
        highlights.Add(highlight);
    }

    public void UnhighlightAllCells()
    {
        foreach (GameObject highlight in highlights)
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

    public bool MakeMove(Piece piece, Vector2Int startPosition, Vector2Int endPosition)
    {
        if (((int)piece.Side) != _currentPlayer)
        { 
            Debug.Log($"It's not your turn! Makes move {Enum.ToObject(typeof(Side), 1 - piece.Side).ToString()}");
            return false;
        }

        bool canMove = false;
        // Check castling
        if (piece.GetType() == typeof(King) && Mathf.Abs(endPosition.x - startPosition.x) > 1 && endPosition.y == startPosition.y)
        {
            canMove = Castle(endPosition.x > startPosition.x);
            if (canMove)
            {
                return true;
            }
        }
        else // Or check the move
        {
            Move lastMove = _movesHistory.Count > 0 ? _movesHistory[_movesHistory.Count - 1] : null;
            canMove = piece.MovePiece(startPosition, endPosition, _board, lastMove);
        }

        if (piece.GetType() == typeof(Pawn)) // Check if its enpassant
        {
            Move lastMove = _movesHistory.Count > 0 ? _movesHistory[_movesHistory.Count - 1] : null;
            if (_movesHistory.Count > 0 && piece.GetComponent<Pawn>().IsEnPassant(startPosition, endPosition, lastMove))
            {
                Vector2Int enPassantCapturePosition = lastMove.EndPosition;
                StartCoroutine(MovePieceSmoothly(piece, startPosition, endPosition, _moveDuration, _pieceMovementCurve));
                _movedPieces.Add(piece);
                Destroy(_board[enPassantCapturePosition.y, enPassantCapturePosition.x].gameObject);
                _board[enPassantCapturePosition.y, enPassantCapturePosition.x] = null;
                _board[endPosition.y, endPosition.x] = piece;
                _currentPlayer = 1 - _currentPlayer;
                _currentMove++;
                _movesHistory.Add(new Move(piece, false, startPosition, endPosition));
                return true;
            }
        }

        if (!canMove)
        {
            return false;
        }

        if (piece.GetType() == typeof(King) && IsAttackedCell(endPosition))
        {
            return false;
        }

        if (!IsEmptyCell(endPosition) && piece.CanCapture(startPosition, endPosition, _board) && _board[endPosition.y, endPosition.x].Side != (Side)_currentPlayer)
        {
            Destroy(_board[endPosition.y, endPosition.x].gameObject);
            Debug.Log("Captured a piece");
        }

        StartCoroutine(MovePieceSmoothly(piece, startPosition, endPosition, _moveDuration, _pieceMovementCurve));
        _movedPieces.Add(piece);
        _board[endPosition.y, endPosition.x] = piece;
        _board[startPosition.y, startPosition.x] = null;
        _currentPlayer = 1 - _currentPlayer;
        _currentMove++;
        _movesHistory.Add(new Move(piece, false, startPosition, endPosition));
        return true;
    }

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
            Debug.Log("Cannot castle: king or rook has moved.");
            return false;
        }
        for (int file = Mathf.Min(kingStartFile, rookStartFile) + 1; file < Mathf.Max(kingStartFile, rookStartFile); file++)
        {
            if (_board[rank, file] != null)
            {
                Debug.Log("Cannot castle: there are pieces between the king and rook.");
                return false;
            }
        }
        Vector2Int kingStartPosition = new Vector2Int(kingStartFile, rank);
        Vector2Int kingEndPosition = new Vector2Int(kingEndFile, rank);

        if (IsAttackedCell(kingStartPosition) || IsAttackedCell(kingEndPosition))
        {
            Debug.Log("Cannot castle: king would move through or into an attacked square.");
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
    public bool IsEmptyCell(Vector2Int position) => _board[position.y, position.x] == null;

    public bool IsAttackedCell(Vector2Int position)
    {
        if (!IsPositionInBounds(position)) return false;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piece piece = _board[i, j];
                if (piece != null && piece.Side != (Side)_currentPlayer)
                {
                    Move lastMove = GetLastMove();
                    bool canMove = piece.MovePiece(new Vector2Int(j, i), position, _board, lastMove);
                    bool canCapture = piece.CanCapture(new Vector2Int(j, i), position, _board);
                    if (canMove && canCapture) // It is attacked cell
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    [CustomEditor(typeof(Board))]
    public class customButton : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Board myScript = (Board)target;
            if (GUILayout.Button("Import FEN"))
            {
                myScript.ImportFEN(myScript.fen);
            }
        }
    }
}

