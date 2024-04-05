using UnityEngine;

public class Knight : Piece
{
    public override bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Piece[,] board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        int dx = Mathf.Abs(endX - startX);
        int dy = Mathf.Abs(endY - startY);
        return (dx == 2 && dy == 1) || (dx == 1 && dy == 2);
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
        int dx = Mathf.Abs(endX - startX);
        int dy = Mathf.Abs(endY - startY);
        if ((dx == 2 && dy == 1) || (dx == 1 && dy == 2))
        {
            Debug.Log("Valid move for the knight.");
            return true;
        }
        else
        {
            Debug.Log("Invalid move for the knight.");
            return false;
        }
    }
}
