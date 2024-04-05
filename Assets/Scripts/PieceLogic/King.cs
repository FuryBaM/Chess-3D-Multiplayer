using UnityEngine;

public class King : Piece
{
    public override bool CanCapture(Vector2Int myPiecePosition, Vector2Int opponentPiecePosition, Piece[,] board)
    {
        int startX = Mathf.RoundToInt(myPiecePosition.x);
        int startY = Mathf.RoundToInt(myPiecePosition.y);
        int endX = Mathf.RoundToInt(opponentPiecePosition.x);
        int endY = Mathf.RoundToInt(opponentPiecePosition.y);
        return Mathf.Abs(endX - startX) <= 1 && Mathf.Abs(endY - startY) <= 1;
    }
    public override bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Piece[,] board)
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

        if (Mathf.Abs(endX - startX) <= 1 && Mathf.Abs(endY - startY) <= 1)
        {
            Debug.Log("Valid move for the king.");
            return true;
        }
        else
        {
            Debug.Log("Invalid move for the king.");
            return false;
        }
    }
    public bool CanCastle(bool isShortCastle)
    {
        return false;
    }
}
