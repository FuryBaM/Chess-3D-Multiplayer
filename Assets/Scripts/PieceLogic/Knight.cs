using System.Collections.Generic;
using UnityEngine;

public sealed class Knight : Piece
{
    public override bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        int dx = Mathf.Abs(endX - startX);
        int dy = Mathf.Abs(endY - startY);
<<<<<<< HEAD
        return ((dx == 2 && dy == 1) || (dx == 1 && dy == 2)) && board.GetPieceAtPosition(endPosition).Side != Side;
=======
        bool friendlyFire = !board.IsEmptyCell(endPosition) && board.GetPieceAtPosition(endPosition).Side == Side;
        return ((dx == 2 && dy == 1) || (dx == 1 && dy == 2)) && !friendlyFire;
>>>>>>> 52f65a09fc87522973687a1a5596052063acc6ac
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
        int dx = Mathf.Abs(endX - startX);
        int dy = Mathf.Abs(endY - startY);
        if ((dx == 2 && dy == 1) || (dx == 1 && dy == 2))
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
    public override List<Vector2Int> GetPossibleMoves(Vector2Int currentPosition, Board board)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        // Определяем начальные координаты
        int startX = Mathf.RoundToInt(currentPosition.x);
        int startY = Mathf.RoundToInt(currentPosition.y);

        // Задаем возможные направления хода коня
        int[] dx = { 1, 1, 2, 2, -1, -1, -2, -2 };
        int[] dy = { 2, -2, 1, -1, 2, -2, 1, -1 };

        // Проверяем каждое направление хода коня
        for (int i = 0; i < dx.Length; i++)
        {
            int newX = startX + dx[i];
            int newY = startY + dy[i];

            // Проверяем, находится ли новая позиция в пределах доски
            if (Board.IsPositionInBounds(new Vector2Int(newX, newY)))
            {
                Piece pieceAtNewPosition = board.GetPieceAtPosition(new Vector2Int(newX, newY));

                // Проверяем, свободна ли клетка или стоит на ней вражеская фигура
                if (pieceAtNewPosition == null || pieceAtNewPosition.Side != Side)
                {
                    possibleMoves.Add(new Vector2Int(newX, newY));
                }
            }
        }

        return possibleMoves;
    }

}
