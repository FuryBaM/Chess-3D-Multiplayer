using System.Threading;
using UnityEngine;
using System;
[Serializable]
public class Move
{
    public Piece MovedPiece { get; }
    public bool Castle { get; }
    public Vector2Int StartPosition { get; }
    public Vector2Int EndPosition { get; }
    public bool Promotion { get; }

    public Move(Piece movedPiece, bool castle, Vector2Int startPosition, Vector2Int endPosition, bool promotion = false)
    {
        MovedPiece = movedPiece;
        Castle = castle;
        StartPosition = startPosition;
        EndPosition = endPosition;
        Promotion = promotion;
    }
    public override string ToString()
    {
        return String.Format("Name:{0}, Castle:{1}, Start position:{2}, End position:{3}", MovedPiece, Castle, StartPosition, EndPosition);
    }
}

public static class MoveConverter
{
    public static string ConvertMoveToString(Move move)
    {
        string moveString = "";
        
        // Если ход является рокировкой, вернем "O-O" или "O-O-O" в зависимости от типа рокировки
        if (move.Castle)
        {
            if (move.EndPosition.x == 6) // Короткая рокировка
            {
                return "O-O";
            }
            else if (move.EndPosition.x == 2) // Длинная рокировка
            {
                return "O-O-O";
            }
        }
        
        // В противном случае вернем ход в алгебраической нотации
        string pieceSymbol = GetPieceSymbol(move.MovedPiece);
        string endPosition = GetPositionString(move.EndPosition);

        moveString += pieceSymbol;
        moveString += endPosition;

        // Добавляем обозначение превращения пешки, если есть
        if (move.Promotion)
        {
            moveString += "=Q"; // Пример: если пешка превращается в ферзя
        }

        return moveString;
    }

    private static string GetPieceSymbol(Piece piece)
    {
        // Получаем символ фигуры в алгебраической нотации
        if (piece is King) return "K";
        if (piece is Queen) return "Q";
        if (piece is Rook) return "R";
        if (piece is Bishop) return "B";
        if (piece is Knight) return "N";
        if (piece is Pawn) return ""; // Пешка не указывает свой символ

        return ""; // Если тип фигуры не распознан
    }

    private static string GetPositionString(Vector2Int position)
    {
        // Получаем строковое представление позиции в алгебраической нотации
        char file = (char)('a' + position.x);
        int rank = position.y + 1;
        return file.ToString() + rank.ToString();
    }
}
