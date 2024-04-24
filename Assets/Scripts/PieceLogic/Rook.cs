using System.Collections.Generic;
using UnityEngine;

public sealed class Rook : Piece
{
    public override bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        return (startX == endX || startY == endY) && !(startX == endX && startY == endY) && board.GetPieceAtPosition(endPosition).Side != Side;
    }
    public override bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Board board)
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
                if (board.GameBoard[y, x] != null)
                {
                    if (board.GameBoard[y, x].Side == Side)
                    {
                        return false;
                    }
                    else if (board.GameBoard[y, x].Side == 1 - Side && x != endX && y != endY)
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
    public override List<Vector2Int> GetPossibleMoves(Vector2Int currentPosition, Board board)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        // Определяем начальные координаты
        int startX = Mathf.RoundToInt(currentPosition.x);
        int startY = Mathf.RoundToInt(currentPosition.y);
        int width = board.GameBoard.GetLength(1);
        int height = board.GameBoard.GetLength(0);

        // Добавляем возможные ходы для ладьи (по вертикали и горизонтали)
        for (int x = startX - 1; x >= 0; x--)
        {
            if (!AddMoveIfValid(new Vector2Int(x, startY), possibleMoves, board))
                break;
        }
        for (int x = startX + 1; x < width; x++)
        {
            if (!AddMoveIfValid(new Vector2Int(x, startY), possibleMoves, board))
                break;
        }
        for (int y = startY - 1; y >= 0; y--)
        {
            if (!AddMoveIfValid(new Vector2Int(startX, y), possibleMoves, board))
                break;
        }
        for (int y = startY + 1; y < height; y++)
        {
            if (!AddMoveIfValid(new Vector2Int(startX, y), possibleMoves, board))
                break;
        }

        return possibleMoves;
    }

    private bool AddMoveIfValid(Vector2Int position, List<Vector2Int> possibleMoves, Board board)
    {
        if (!Board.IsPositionInBounds(position))
            return false;

        Piece piece = board.GetPieceAtPosition(position);
        if (piece == null)
        {
            possibleMoves.Add(position);
            return true;
        }
        else if (piece.Side != Side)
        {
            possibleMoves.Add(position);
        }
        return false;
    }

}
