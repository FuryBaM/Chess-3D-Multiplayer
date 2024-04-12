using System.Threading;
using UnityEngine;
using System;
public class Move
{
    public Piece MovedPiece { get; }
    public bool Castle { get; }
    public Vector2Int StartPosition { get; }
    public Vector2Int EndPosition { get; }

    public Move(Piece movedPiece, bool castle, Vector2Int startPosition, Vector2Int endPosition)
    {
        MovedPiece = movedPiece;
        Castle = castle;
        StartPosition = startPosition;
        EndPosition = endPosition;
    }
    public override string ToString()
    {
        return String.Format("Name:{0}, Castle:{1}, Start position:{2}, End position:{3}", MovedPiece, Castle, StartPosition, EndPosition);
    }
}