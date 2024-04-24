using System;
using UnityEngine;

public sealed class PlayerController : MonoBehaviour
{
    [Header("Board Features")]
    [SerializeField] private Board board;
    [SerializeField] private Material _outline;
    private Piece _selectedPiece = null;
    [SerializeField] private Side _player = Side.white;
    [SerializeField] private LayerMask _whatIsSelectable;
    private void Update()
    {
        if (_selectedPiece == null)
        {
            SelectPiece();
        }
        else if (_selectedPiece != null)
        {
            MovePieceToMouse();
            if (Input.GetMouseButtonDown(1))
            {
                UnselectPiece();
            }
        }
    }
    private void SelectPiece()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, _whatIsSelectable))
            {
                if (hit.collider.gameObject.CompareTag("Piece"))
                {
                    _selectedPiece = hit.collider.GetComponent<Piece>();
                    HighlightPiece();
                }
                else if (hit.collider.gameObject.CompareTag("Board"))
                {
                    Board board = hit.collider.GetComponent<Board>();
                    Vector2Int position = new Vector2Int(Mathf.RoundToInt(hit.point.x), Mathf.RoundToInt(hit.point.z));
                    Piece piece = board.GetPieceAtPosition(position);
                    if (piece)
                    {
                        _selectedPiece = piece;
                        HighlightPiece();
                    }
                }
            }
        }
    }
    private void UnselectPiece()
    {
        if (_selectedPiece == null) return;
        Material[] materials = _selectedPiece.GetComponent<MeshRenderer>().materials;
        Array.Resize(ref materials, materials.Length - 1);
        _selectedPiece.GetComponent<MeshRenderer>().materials = materials;
        _selectedPiece = null;
        board.UnhighlightAllCells();
    }
    public void HighlightPiece()
    {
        _selectedPiece.GetComponent<MeshRenderer>().materials = new Material[] { _selectedPiece.GetComponent<MeshRenderer>().materials[0], _outline };
        HighlightPossibleMoves(board.GetPiecePosition(_selectedPiece));
    }
    private void HighlightPossibleMoves(Vector2Int position)
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Vector2Int targetPosition = new Vector2Int(x, y);
                Move lastMove = board.GetLastMove();
                if (_selectedPiece.MovePiece(position, targetPosition, board))
                {
                    bool isPawn = _selectedPiece.GetType() == typeof(Pawn) && _selectedPiece.CanCapture(position, targetPosition, board);
                    if (!board.IsEmptyCell(targetPosition) && _selectedPiece.Side != board.GetPieceAtPosition(targetPosition).Side || isPawn)
                    {
                        board.HighlightCell(targetPosition, true);
                    }
                    else
                    {
                        board.HighlightCell(targetPosition, false);
                    }
                }
            }
        }
    }
    private void MovePieceToMouse()
    {
        Vector2Int movePosition;
        Vector2Int startPosition = new Vector2Int(Mathf.RoundToInt(_selectedPiece.transform.position.x), Mathf.RoundToInt(_selectedPiece.transform.position.z));
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Board"))
                {
                    movePosition = new Vector2Int(Mathf.RoundToInt(hit.point.x), Mathf.RoundToInt(hit.point.z));
                    board.MakeMove(_selectedPiece, startPosition, movePosition);
                    UnselectPiece();
                }
                else if (hit.collider.gameObject.CompareTag("Piece"))
                {
                    Piece piece = hit.collider.GetComponent<Piece>();
                    if (piece.Side == _player) return;
                    movePosition = board.GetPiecePosition(piece);
                    board.MakeMove(_selectedPiece, startPosition, movePosition);
                    UnselectPiece();
                }
            }
        }
    }
}
