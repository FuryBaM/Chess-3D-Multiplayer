using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override bool CanCapture(Vector2Int myPiecePosition, Vector2Int opponentPiecePosition, Piece[,] board)
    {
        int startX = Mathf.RoundToInt(myPiecePosition.x);
        int startY = Mathf.RoundToInt(myPiecePosition.y);
        int endX = Mathf.RoundToInt(opponentPiecePosition.x);
        int endY = Mathf.RoundToInt(opponentPiecePosition.y);
        if (Mathf.Abs(endX - startX) == 1 && Mathf.Abs(endY - startY) == 1)
        {
            if (Side == Side.white)
            {
                return endY > startY;
            }
            else
            {
                return endY < startY;
            }
        }
        return false;
    }

    public override bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Piece[,] board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        if (!Board.IsPositionInBounds(new Vector2(startX, startY)) || !Board.IsPositionInBounds(new Vector2(endX, endY)))
        {
            return false;
        }
        if (startPosition == endPosition)
        {
            Debug.Log("Start and end positions are the same.");
            return false;
        }
        if (Mathf.Abs(endX - startX) == 0)
        {
            if (endY - startY == 1 && Side == Side.white || endY - startY == -1 && Side == Side.black)
            { 
                return true && board[endY, endX] == null;
            }
            else
            {
                if (endY - startY == 2 && Side == Side.white && startY == 1 || endY - startY == -2 && Side == Side.black && startY == 6)
                    return true && board[endY, endX] == null;
                return false;
            }
        }
        else if (CanCapture(startPosition, endPosition, board))
        {
            return true;
        }
        else
        {
            Debug.Log("Invalid move for the pawn.");
            return false;
        }
    }
}
