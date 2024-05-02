using System.Collections.Generic;
using UnityEngine;

public sealed class Bishop : Piece
{
    public override bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        bool friendlyFire = !board.IsEmptyCell(endPosition) && board.GetPieceAtPosition(endPosition).Side == Side;
        return Mathf.Abs(endX - startX) == Mathf.Abs(endY - startY) && !(startX == endX && startY == endY) && !friendlyFire;
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
            //Start and end positions are the same.
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
                        //There is a piece blocking the bishop's path by own piece
                        return false;
                    }
                    else if (board.GameBoard[y, x].Side == 1 - Side && x != endX && y != endY)
                    {
                        //There is a piece blocking the bishop's path by enemy piece
                        return false;
                    }
                }
                x += deltaX;
                y += deltaY;
            }
            if (board.GameBoard[endY, endX] == null || board.GameBoard[endY, endX].Side != this.Side)
            {
                //Valid move for the bishop.
                return true;
            }
            else
            {
                //Target position is occupied by own piece.
                return false;
            }
        }
        else
        {
            //Invalid move for the bishop.
            return false;
        }
    }
    public override List<Vector2Int> GetPossibleMoves(Vector2Int currentPosition, Board board)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        int startX = Mathf.RoundToInt(currentPosition.x);
        int startY = Mathf.RoundToInt(currentPosition.y);

        for (int deltaX = -1; deltaX <= 1; deltaX += 2)
        {
            for (int deltaY = -1; deltaY <= 1; deltaY += 2)
            {
                int x = startX + deltaX;
                int y = startY + deltaY;

                while (Board.IsPositionInBounds(new Vector2Int(x, y)))
                {
                    if (board.GetPieceAtPosition(new Vector2Int(x, y)) == null ||
                        board.GetPieceAtPosition(new Vector2Int(x, y)).Side != Side)
                    {
                        Vector2Int endPosition = new Vector2Int(x, y);
                        if (MovePiece(currentPosition, endPosition, board))
                        {
                            possibleMoves.Add(endPosition);
                        }
                    }
                    if (board.GetPieceAtPosition(new Vector2Int(x, y)) != null)
                    {
                        break;
                    }
                    x += deltaX;
                    y += deltaY;
                }
            }
        }
        return possibleMoves;
    }
}
