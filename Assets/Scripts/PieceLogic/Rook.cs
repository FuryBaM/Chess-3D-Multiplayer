using UnityEngine;

public class Rook : Piece
{
    public override bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Piece[,] board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        return (startX == endX || startY == endY) && !(startX == endX && startY == endY);
    }
    public override bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Piece[,] board, Move lastMove)
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
            Debug.Log("Start and end positions are the same.");
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
                if (board[y, x] != null)
                {
                    if (board[y, x].Side == Side)
                    {
                        Debug.Log("There is a piece blocking the rook's path by own piece");
                        return false;
                    }
                    else if (board[y, x].Side == 1 - Side && x != endX && y != endY)
                    {
                        Debug.Log("There is a piece blocking the rook's path by enemy piece");
                        return false;
                    }
                }
                x += deltaX;
                y += deltaY;
            }
            if (board[endY, endX] == null || board[endY, endX].Side != this.Side)
            {
                Debug.Log("Valid move for the rook.");
                return true;
            }
            else
            {
                Debug.Log("Target position is occupied by own piece.");
                return false;
            }
        }
        else
        {
            Debug.Log("Invalid move for the rook.");
            return false;
        }
    }
}
