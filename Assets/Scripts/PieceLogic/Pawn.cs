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
            if (board[endY, endX] == null) return false;
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

    public override bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Piece[,] board, Move lastMove)
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
        else if (IsEnPassant(startPosition, endPosition, lastMove))
        {
            return true;
        }
        else
        {
            Debug.Log("Invalid move for the pawn.");
            return false;
        }
    }
    public bool IsEnPassant(Vector2Int startPosition, Vector2Int endPosition, Move lastMove)
    {
        if (lastMove == null) return false; 
        Piece movedPiece = lastMove.MovedPiece;
        Vector2Int movedPieceEndPosition = lastMove.EndPosition;

        if (movedPiece.GetType() == typeof(Pawn) && Mathf.Abs(movedPieceEndPosition.y - lastMove.StartPosition.y) == 2)
        {
            print(Mathf.Abs(startPosition.x - endPosition.x) == 1);
            print(endPosition.x == movedPieceEndPosition.x);
            print(endPosition.y == movedPieceEndPosition.y + (movedPiece.Side == Side.white ? -1 : 1));
            if (Mathf.Abs(startPosition.x - endPosition.x) == 1 &&
                endPosition.x == movedPieceEndPosition.x && 
                endPosition.y == movedPieceEndPosition.y + (movedPiece.Side == Side.white ? -1 : 1))
            {
                return true;
            }
        }
        return false;
    }
}
