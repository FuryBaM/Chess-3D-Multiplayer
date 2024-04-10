using UnityEngine;

public enum Side
{
    white = 0,
    black = 1
}
public abstract class Piece : MonoBehaviour
{
    [SerializeField] private Side _pieceSide = Side.white;
    public Side Side { get => _pieceSide; protected set { _pieceSide = value; } }
    public abstract bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Piece[,] board);
    public virtual bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Piece[,] board)
    {
        return true;
    }
}
