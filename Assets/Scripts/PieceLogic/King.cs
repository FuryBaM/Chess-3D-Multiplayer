using UnityEngine;

public class King : Piece
{
    public override bool CanCapture(Vector2Int myPiecePosition, Vector2Int opponentPiecePosition, Board board)
    {
        int startX = Mathf.RoundToInt(myPiecePosition.x);
        int startY = Mathf.RoundToInt(myPiecePosition.y);
        int endX = Mathf.RoundToInt(opponentPiecePosition.x);
        int endY = Mathf.RoundToInt(opponentPiecePosition.y);
        return Mathf.Abs(endX - startX) <= 1 && Mathf.Abs(endY - startY) <= 1;
    }

    public override bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);

        if (!Board.IsPositionInBounds(startPosition) || !Board.IsPositionInBounds(endPosition)) return false;
        if (startPosition == endPosition) return false;
        if (board.IsAttackedCell(endPosition))
        {
            return false;
        }

        if (Mathf.Abs(endX - startX) <= 1 && Mathf.Abs(endY - startY) <= 1)
        {
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
