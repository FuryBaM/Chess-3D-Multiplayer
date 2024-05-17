using System.Collections.Generic;
using UnityEngine;

public sealed class Pawn : Piece
{
    public override bool CanCapture(Vector2Int myPiecePosition, Vector2Int opponentPiecePosition, Board board)
    {
        int startX = Mathf.RoundToInt(myPiecePosition.x);
        int startY = Mathf.RoundToInt(myPiecePosition.y);
        int endX = Mathf.RoundToInt(opponentPiecePosition.x);
        int endY = Mathf.RoundToInt(opponentPiecePosition.y);
        if (IsEnPassant(myPiecePosition, opponentPiecePosition, board.GetLastMove()))
        {
            return true;
        }
        if (Mathf.Abs(endX - startX) == 1 && Mathf.Abs(endY - startY) == 1)
        {
            if (board.GameBoard[endY*8+endX] == null) return false;
            bool friendlyFire = !board.IsEmptyCell(opponentPiecePosition) && board.GetPieceAtPosition(opponentPiecePosition).Side == Side;
            if (Side == Side.white)
            {
                return endY > startY && !friendlyFire;
            }
            else
            {
                return endY < startY && !friendlyFire;
            }
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
        if (Mathf.Abs(endX - startX) == 0)
        {
            if (endY - startY == 1 && Side == Side.white || endY - startY == -1 && Side == Side.black)
            { 
                return true && board.GameBoard[endY*8+endX] == null;
            }
            else
            {
                if (endY - startY == 2 && Side == Side.white && startY == 1 || endY - startY == -2 && Side == Side.black && startY == 6)
                    return true && board.GameBoard[endY*8+endX] == null;
                return false;
            }
        }
        else if (CanCapture(startPosition, endPosition, board))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool IsEnPassant(Vector2Int startPosition, Vector2Int endPosition, Move lastMove)
    {
        if (lastMove == null || lastMove.MovedPiece.Side == Side) return false; 
        Piece movedPiece = lastMove.MovedPiece;
        Vector2Int movedPieceEndPosition = lastMove.EndPosition;

        if (movedPiece.GetType() == typeof(Pawn) && Mathf.Abs(movedPieceEndPosition.y - lastMove.StartPosition.y) == 2 && startPosition.y == movedPieceEndPosition.y)
        {
            if (Mathf.Abs(startPosition.x - endPosition.x) == 1 &&
                endPosition.x == movedPieceEndPosition.x && 
                endPosition.y == movedPieceEndPosition.y + (movedPiece.Side == Side.white ? -1 : 1))
            {
                return true;
            }
        }
        return false;
    }

    public override List<Vector2Int> GetPossibleMoves(Vector2Int currentPosition, Board board)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        // Определяем начальные координаты
        int startX = Mathf.RoundToInt(currentPosition.x);
        int startY = Mathf.RoundToInt(currentPosition.y);

        // Задаем смещение в зависимости от цвета пешки
        int forwardDirection = (Side == Side.white) ? 1 : -1;

        // Проверяем клетку перед пешкой
        Vector2Int forwardOne = new Vector2Int(startX, startY + forwardDirection);
        if (board.IsEmptyCell(forwardOne))
        {
            possibleMoves.Add(forwardOne);

            // Если пешка еще не двигалась, проверяем движение на две клетки вперед
            if ((forwardDirection == 1 && startY == 1) || (forwardDirection == -1 && startY == 6))
            {
                Vector2Int forwardTwo = new Vector2Int(startX, startY + 2 * forwardDirection);
                if (board.IsEmptyCell(forwardTwo))
                {
                    possibleMoves.Add(forwardTwo);
                }
            }
        }

        // Проверяем возможные ходы для захвата
        Vector2Int captureLeft = new Vector2Int(startX - 1, startY + forwardDirection);
        Vector2Int captureRight = new Vector2Int(startX + 1, startY + forwardDirection);
        if (Board.IsPositionInBounds(captureLeft) && board.GetPieceAtPosition(captureLeft) != null && board.GetPieceAtPosition(captureLeft).Side != Side)
        {
            possibleMoves.Add(captureLeft);
        }
        if (Board.IsPositionInBounds(captureRight) && board.GetPieceAtPosition(captureRight) != null && board.GetPieceAtPosition(captureRight).Side != Side)
        {
            possibleMoves.Add(captureRight);
        }

        // Проверяем возможные ходы для взятия на проходе
        Move lastMove = board.GetLastMove();
        if (lastMove != null && IsEnPassant(currentPosition, lastMove.EndPosition, lastMove))
        {
            Vector2Int enPassantPosition = new Vector2Int(lastMove.EndPosition.x, startY + forwardDirection);
            possibleMoves.Add(enPassantPosition);
        }

        return possibleMoves;
    }

}
