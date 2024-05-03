﻿using System;
using UnityEngine;

public sealed class PlayerController : MonoBehaviour
{
    [Header("Board Features")]
<<<<<<< HEAD
    [SerializeField] private Board board;
=======
    [SerializeField] private Board _board;
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
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
<<<<<<< HEAD
        board.UnhighlightAllCells();
=======
        _board.UnhighlightAllCells();
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
    }
    public void HighlightPiece()
    {
        _selectedPiece.GetComponent<MeshRenderer>().materials = new Material[] { _selectedPiece.GetComponent<MeshRenderer>().materials[0], _outline };
<<<<<<< HEAD
        HighlightPossibleMoves(board.GetPiecePosition(_selectedPiece));
=======
        HighlightPossibleMoves(_board.GetPiecePosition(_selectedPiece));
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
    }
    private void HighlightPossibleMoves(Vector2Int position)
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Vector2Int targetPosition = new Vector2Int(x, y);
<<<<<<< HEAD
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
=======
                Move lastMove = _board.GetLastMove();
                if (_selectedPiece.MovePiece(position, targetPosition, _board))
                {
                    bool isPawn = _selectedPiece.GetType() == typeof(Pawn) && _selectedPiece.CanCapture(position, targetPosition, _board);
                    if (!_board.IsEmptyCell(targetPosition) && _selectedPiece.Side != _board.GetPieceAtPosition(targetPosition).Side || isPawn)
                    {
                        _board.HighlightCell(targetPosition, true);
                    }
                    else
                    {
                        _board.HighlightCell(targetPosition, false);
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
                    }
                }
            }
        }
    }
    private void MovePieceToMouse()
    {
<<<<<<< HEAD
        if (_selectedPiece == null) return;
=======
        if (_selectedPiece == null || _board.IsGameOver) return;
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
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
<<<<<<< HEAD
                    board.MakeMove(_selectedPiece, startPosition, movePosition);
=======
                    _board.MakeMove(_selectedPiece, startPosition, movePosition);
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
                    UnselectPiece();
                }
                else if (hit.collider.gameObject.CompareTag("Piece"))
                {
                    Piece piece = hit.collider.GetComponent<Piece>();
                    if (piece.Side == _player) return;
<<<<<<< HEAD
                    movePosition = board.GetPiecePosition(piece);
                    board.MakeMove(_selectedPiece, startPosition, movePosition);
=======
                    movePosition = _board.GetPiecePosition(piece);
                    _board.MakeMove(_selectedPiece, startPosition, movePosition);
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
                    UnselectPiece();
                }
            }
        }
    }
}
