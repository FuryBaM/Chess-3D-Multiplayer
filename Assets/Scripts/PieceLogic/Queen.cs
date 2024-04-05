using UnityEngine;

public class Queen : Piece
{
    public override bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Piece[,] board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        if (startX == endX || startY == endY || Mathf.Abs(endX - startX) == Mathf.Abs(endY - startY))
        {
            return true;
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

        if (startX == endX || startY == endY || Mathf.Abs(endX - startX) == Mathf.Abs(endY - startY))
        {
            int deltaX = (endX - startX) > 0 ? 1 : -1;
            int deltaY = (endY - startY) > 0 ? 1 : -1;
            int x = startX + deltaX;
            int y = startY + deltaY;
            while (x != endX && y != endY)
            {
                if (board[y, x] != null)
                {
                    if (board[y, x].Side == Side)
                    {
                        Debug.Log("There is a piece blocking the queen's path by own piece");
                        return false;
                    }
                    else if (board[y, x].Side == 1 - Side && x != endX && y != endY)
                    {
                        Debug.Log("There is a piece blocking the queen's path by enemy piece");
                        return false;
                    }
                }
                x += deltaX;
                y += deltaY;
            }
            Debug.Log("Valid move for the queen.");
            return true;
        }
        else
        {
            Debug.Log("Invalid move for the queen.");
            return false;
        }
    }
}
