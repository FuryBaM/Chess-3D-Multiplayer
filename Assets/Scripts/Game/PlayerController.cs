using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Board Features")]
    [SerializeField] private Board board;
    [SerializeField] private Material _outline;
    private Piece _selectedPiece;
    [SerializeField] private Side _player = Side.white;
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
            LayerMask pieceLayerMask = LayerMask.GetMask("Piece");
            if (Physics.Raycast(ray, out hit, pieceLayerMask))
            {
                if (hit.collider.gameObject.CompareTag("Piece"))
                {
                    _selectedPiece = hit.collider.GetComponent<Piece>();
                    _selectedPiece.GetComponent<MeshRenderer>().materials = new Material[] { _selectedPiece.GetComponent<MeshRenderer>().materials[0], _outline };
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
