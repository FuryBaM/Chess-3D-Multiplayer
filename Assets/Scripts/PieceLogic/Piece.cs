using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public enum Side
{
    white = 0,
    black = 1
}
[Serializable]
public abstract class Piece : NetworkBehaviour
{
    [SyncVar]
    [SerializeField] private Side _pieceSide = Side.white;
    public Side Side { get => _pieceSide; protected set { _pieceSide = value; } }
    public abstract bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Board board);
    public virtual bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        return true;
    }
    public abstract List<Vector2Int> GetPossibleMoves(Vector2Int currentPosition, Board board);
}
