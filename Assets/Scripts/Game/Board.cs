using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Board : MonoBehaviour
{
    private Piece[,] _board = new Piece[8, 8];
    private int _currentPlayer = 0;
    private int _currentMove = 1;
    private bool _enpassantAvailable = false;
    private bool _canCastle = false;
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
        fenImport("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
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

    public void fenImport(string fenString)
    {
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
                    Piece piece = spawnPieceByChar(c);
                    Vector3 position = new Vector3(x, 0, y);
                    piece.transform.position = position;
                    _board[y, x] = piece;
                    x++;
                }
            }
        }
    }
    private Piece spawnPieceByChar(char fenChar)
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

    public bool MakeMove(Piece piece, Vector2Int startPosition, Vector2Int endPosition)
    {
        if (((int)piece.Side) != _currentPlayer)
        { 
            print($"It's not your turn! Makes move {Enum.ToObject(typeof(Side), 1 - piece.Side).ToString()}");
            return false;
        }
        bool canMove = piece.MovePiece(startPosition, endPosition, _board);
        if (piece.GetType() == typeof(King) && IsAttackedCell(endPosition))
        {
            return false;
        }
        if (canMove)
        {
            if (!IsEmptyCell(endPosition) && piece.CanCapture(startPosition, endPosition, _board) && _board[endPosition.y, endPosition.x].Side != (Side)_currentPlayer)
            {
                Destroy(_board[endPosition.y, endPosition.x].gameObject);
                print("Captured a piece");
            }
            piece.transform.position = new Vector3(endPosition.x, 0, endPosition.y);
            _board[endPosition.y, endPosition.x] = piece;
            _board[startPosition.y, startPosition.x] = null;
            _currentPlayer = 1 - _currentPlayer;
        }
        return canMove;
    }

    public bool IsEmptyCell(Vector2Int position) => _board[position.y, position.x] == null;

    public bool IsAttackedCell(Vector2Int position)
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Piece piece = _board[i, j];
                if (piece != null && piece.Side != (Side)_currentPlayer)
                {
                    bool canMove = piece.MovePiece(new Vector2Int(j, i), position, _board);
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
                myScript.fenImport(myScript.fen);
            }
        }
    }
}

