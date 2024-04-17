using UnityEngine;

public class Bishop : Piece
{
    public override bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        return Mathf.Abs(endX - startX) == Mathf.Abs(endY - startY) && !(startX == endX && startY == endY);
    }

    public override bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        if (!Board.IsPositionInBounds(startPosition)|| !Board.IsPositionInBounds(endPosition))
        {
            return false;
        }
        if (startPosition == endPosition)
        {
            Debug.Log("Start and end positions are the same.");
            return false;
        }
        if (Mathf.Abs(endX - startX) == Mathf.Abs(endY - startY))
        {
            int deltaX = (endX - startX) > 0 ? 1 : -1;
            int deltaY = (endY - startY) > 0 ? 1 : -1;
            int x = startX + deltaX;
            int y = startY + deltaY;
            while (x != endX && y != endY)
            {
                if (board.GameBoard[y, x] != null)
                {
                    if (board.GameBoard[y, x].Side == Side)
                    {
                        Debug.Log("There is a piece blocking the bishop's path by own piece");
                        return false;
                    }
                    else if (board.GameBoard[y, x].Side == 1 - Side && x != endX && y != endY)
                    {
                        Debug.Log("There is a piece blocking the bishop's path by enemy piece");
                        return false;
                    }
                }
                x += deltaX;
                y += deltaY;
            }
            if (board.GameBoard[endY, endX] == null || board.GameBoard[endY, endX].Side != this.Side)
            {
                Debug.Log("Valid move for the bishop.");
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
            Debug.Log("Invalid move for the bishop.");
            return false;
        }
    }

}
