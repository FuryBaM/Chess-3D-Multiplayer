using System;
using UnityEngine;
using Mirror;

public sealed class PlayerController : NetworkBehaviour
{
    [Header("Board Features")]
    [SerializeField] private Board _board;
    [SerializeField] private Material _outline;
    private Piece _selectedPiece = null;
    [SyncVar]
    [SerializeField] private Side _player = Side.white;
    private Camera _playerCamera;

    [SerializeField] private LayerMask _whatIsSelectable;
    private void Awake()
    {
        _board = FindObjectOfType<Board>();
        _playerCamera = FindObjectOfType<Camera>();
    }
    private void Update()
    {
        if (!isLocalPlayer || (Side)_board.Player != _player) return;
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
    public Side GetPlayerSide() => _player;
    [TargetRpc]
    public void OnSideChanged(Side newSide)
    {
        print($"Side changed {newSide}");
        _playerCamera.GetComponent<CameraPositionChanger>().SetSide(newSide);
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
        _board.UnhighlightAllCells();
    }
    public void HighlightPiece()
    {
        _selectedPiece.GetComponent<MeshRenderer>().materials = new Material[] { _selectedPiece.GetComponent<MeshRenderer>().materials[0], _outline };
        HighlightPossibleMoves(_board.GetPiecePosition(_selectedPiece));
    }
    private void HighlightPossibleMoves(Vector2Int position)
    {
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Vector2Int targetPosition = new Vector2Int(x, y);
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
                    }
                }
            }
        }
    }
    private void MovePieceToMouse()
    {
        if (!isLocalPlayer) return;
        if (_selectedPiece == null || _board.IsGameOver) return;
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
                    if (!NetworkClient.ready) NetworkClient.Ready();
                    CmdMovePiece(startPosition, movePosition);
                    UnselectPiece();
                }
                else if (hit.collider.gameObject.CompareTag("Piece"))
                {
                    Piece piece = hit.collider.GetComponent<Piece>();
                    if (piece.Side == _player) return;
                    movePosition = _board.GetPiecePosition(piece);
                    if (!NetworkClient.ready) NetworkClient.Ready();
                    CmdMovePiece(startPosition, movePosition);
                    UnselectPiece();
                }
            }
        }
    }
    [Server]
    public void SetPlayerSide(Side side)
    {
        _player = side;
        OnSideChanged(side);
    }
    [Command]
    public void CmdMovePiece(Vector2Int startPosition, Vector2Int endPosition)
    {
        Debug.Log("Called move");
        _board.MakeMove(startPosition, endPosition);
    }
}
