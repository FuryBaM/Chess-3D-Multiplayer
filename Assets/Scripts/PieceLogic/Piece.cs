using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public enum Side
{
    white = 0,
    black = 1
}
public enum PieceType
{
    king = 'k',
    pawn = 'p',
    knight = 'n',
    bishop = 'b',
    rook = 'r',
    queen = 'q'
}
[Serializable]
public abstract class Piece : NetworkBehaviour
{
    [SyncVar]
    [SerializeField] private Side _pieceSide = Side.white;
    public Side Side { get => _pieceSide; protected set { _pieceSide = value; } }

    protected bool IsFriendlyTarget(Board board, Vector2Int endPosition)
    {
        Piece target = board.GetPieceAtPosition(endPosition);
        return target != null && target.Side == Side;
    }

    protected bool IsPathClear(Vector2Int startPosition, Vector2Int endPosition, Board board, Vector2Int step)
    {
        Vector2Int current = startPosition + step;
        while (current != endPosition)
        {
            if (board.GetPieceAtPosition(current) != null)
            {
                return false;
            }
            current += step;
        }
        return true;
    }

    public abstract bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Board board);
    public virtual bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        return true;
    }
    public abstract List<Vector2Int> GetPossibleMoves(Vector2Int currentPosition, Board board);
}
