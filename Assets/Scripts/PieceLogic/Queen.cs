using UnityEngine;

public class Queen : Piece
{
    public override bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        bool friendlyFire = board.GameBoard[endPosition.y, endPosition.x] && board.GameBoard[endY, endX].Side == Side;
        if (startX == endX || startY == endY || Mathf.Abs(endX - startX) == Mathf.Abs(endY - startY) && !friendlyFire)
        {
            return true;
        }
        return false;
    }
    public override bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Board board)
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
            return false;
        }

        if (startX == endX || startY == endY || Mathf.Abs(endX - startX) == Mathf.Abs(endY - startY))
        {
            int deltaX = (endX - startX) == 0 ? 0 : (endX - startX) / Mathf.Abs(endX - startX);
            int deltaY = (endY - startY) == 0 ? 0 : (endY - startY) / Mathf.Abs(endY - startY);
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
                    else if (board.GameBoard[y, x].Side != Side)
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
