using System.Threading;
using UnityEngine;
using System;
using Mirror;
[Serializable]
public struct MoveData
{
    public uint MovedPieceId;
    public bool Castle;
    public Vector2Int StartPosition;
    public Vector2Int EndPosition;
    public bool Promotion;
    public PieceType PromotionPiece;
    public MoveData(uint movedPieceId, bool castle, Vector2Int startPosition, Vector2Int endPosition, bool promotion = false, PieceType promotionPiece = PieceType.pawn)
    {
        MovedPieceId = movedPieceId;
        Castle = castle;
        StartPosition = startPosition;
        EndPosition = endPosition;
        Promotion = promotion;
        PromotionPiece = promotionPiece;
    }
}
public class Move
{
    public Piece MovedPiece;
    public bool Castle;
    public Vector2Int StartPosition;
    public Vector2Int EndPosition;
    public bool Promotion;
    public PieceType PromotionPiece;
    public Move(Piece movedPiece, bool castle, Vector2Int startPosition, Vector2Int endPosition, bool promotion = false, PieceType promotionPiece = PieceType.pawn)
    {
        MovedPiece = movedPiece;
        Castle = castle;
        StartPosition = startPosition;
        EndPosition = endPosition;
        Promotion = promotion;
        PromotionPiece = promotionPiece;
    }
    public override string ToString()
    {
        return String.Format("Name:{0}, Castle:{1}, Start position:{2}, End position:{3}", MovedPiece, Castle, StartPosition, EndPosition);
    }
}

public static class MoveConverter
{
    public static string ConvertMoveToString(Move move, Piece piece)
    {
        string moveString = "";
        Debug.Log(move);
        
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
        string pieceSymbol = GetPieceSymbol(piece);
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

    public static Move ConvertStringToMove(Board board, string moveString)
    {
        if (moveString.Length != 4 && moveString.Length != 5)
        {
            Debug.LogError("Invalid move string format. Must be in the format 'startEnd', e.g., 'd8g5'. " + moveString);
            return null;
        }
        else if (moveString == null)
        {
            Debug.LogError("Null string" + moveString);
            return null;
        }

        int startFile = moveString[0] - 'a';
        int startRank = int.Parse(moveString[1].ToString()) - 1;
        int endFile = moveString[2] - 'a';
        int endRank = int.Parse(moveString[3].ToString()) - 1;
        bool promotion = false;
        PieceType promotionPiece = PieceType.queen;
        if (moveString.Length == 5)
        {
            promotion = true;
            switch (char.ToLower(moveString[4]))
            {
                case 'n':
                {
                    promotionPiece = PieceType.knight;
                    break;
                }
                case 'b':
                {
                    promotionPiece = PieceType.bishop;
                    break;
                }
                case 'r':
                {
                    promotionPiece = PieceType.rook;
                    break;
                }
                case 'q':
                {
                    promotionPiece = PieceType.queen;
                    break;
                }
                default:
                {
                    promotionPiece = PieceType.queen;
                    break;
                }
            }
        }

        if (!Board.IsPositionInBounds(new Vector2Int(startFile, startRank)) ||
            !Board.IsPositionInBounds(new Vector2Int(endFile, endRank)))
        {
            Debug.LogError("Invalid move string. Positions are out of bounds.");
            return null;
        }

        Vector2Int startPosition = new Vector2Int(startFile, startRank);
        Vector2Int endPosition = new Vector2Int(endFile, endRank);
        return new Move(board.GetPieceAtPosition(startPosition), false, startPosition, endPosition, promotion, promotionPiece);
    }
}
