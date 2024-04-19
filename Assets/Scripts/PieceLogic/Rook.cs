﻿using UnityEngine;

public class Rook : Piece
{
    public override bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        return (startX == endX || startY == endY) && !(startX == endX && startY == endY) && board.GetPieceAtPosition(endPosition).Side != Side;
    }
    public override bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        if (!Board.IsPositionInBounds(startPosition) || !Board.IsPositionInBounds(endPosition))
        {
            return false;
        }
        if (startPosition == endPosition)
        {
            return false;
        }
        if (startX == endX || startY == endY)
        {
            int deltaX = Mathf.Clamp(endX - startX, -1, 1);
            int deltaY = Mathf.Clamp(endY - startY, -1, 1);
            int x = startX + deltaX;
            int y = startY + deltaY;
            while (x != endX || y != endY)
            {
                if (board.GameBoard[y, x] != null)
                {
                    if (board.GameBoard[y, x].Side == Side)
                    {
                        return false;
                    }
                    else if (board.GameBoard[y, x].Side == 1 - Side && x != endX && y != endY)
                    {
                        return false;
                    }
                }
                x += deltaX;
                y += deltaY;
            }
            if (board.GameBoard[endY, endX] == null || board.GameBoard[endY, endX].Side != this.Side)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
