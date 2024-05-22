﻿using System.Collections.Generic;
using UnityEngine;

public sealed class Queen : Piece
{
    public override bool CanCapture(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);
        bool friendlyFire = board.GameBoard[endPosition.y, endPosition.x] && board.GameBoard[endY, endX].Side == Side;
        if (startX == endX || startY == endY || Mathf.Abs(endX - startX) == Mathf.Abs(endY - startY) && !friendlyFire)
        {
            return true;
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

        if (startX == endX || startY == endY || Mathf.Abs(endX - startX) == Mathf.Abs(endY - startY))
        {
            int deltaX = (endX - startX) == 0 ? 0 : (endX - startX) / Mathf.Abs(endX - startX);
            int deltaY = (endY - startY) == 0 ? 0 : (endY - startY) / Mathf.Abs(endY - startY);
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
                    else if (board.GameBoard[y, x].Side != Side)
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
        int width = 8;
        int height = 8;

        // Добавляем возможные ходы для ладьи (по вертикали и горизонтали)
        for (int x = startX - 1; x >= 0; x--)
        {
            if (!AddMoveIfValid(currentPosition, new Vector2Int(x, startY), possibleMoves, board))
                break;
        }
        for (int x = startX + 1; x < width; x++)
        {
            if (!AddMoveIfValid(currentPosition, new Vector2Int(x, startY), possibleMoves, board))
                break;
        }
        for (int y = startY - 1; y >= 0; y--)
        {
            if (!AddMoveIfValid(currentPosition, new Vector2Int(startX, y), possibleMoves, board))
                break;
        }
        for (int y = startY + 1; y < height; y++)
        {
            if (!AddMoveIfValid(currentPosition, new Vector2Int(startX, y), possibleMoves, board))
                break;
        }

        // Добавляем возможные ходы для слона (по диагонали)
        for (int d = 1; startX - d >= 0 && startY - d >= 0; d++)
        {
            if (!AddMoveIfValid(currentPosition, new Vector2Int(startX - d, startY - d), possibleMoves, board))
                break;
        }
        for (int d = 1; startX - d >= 0 && startY + d < height; d++)
        {
            if (!AddMoveIfValid(currentPosition, new Vector2Int(startX - d, startY + d), possibleMoves, board))
                break;
        }
        for (int d = 1; startX + d < width && startY - d >= 0; d++)
        {
            if (!AddMoveIfValid(currentPosition, new Vector2Int(startX + d, startY - d), possibleMoves, board))
                break;
        }
        for (int d = 1; startX + d < width && startY + d < height; d++)
        {
            if (!AddMoveIfValid(currentPosition, new Vector2Int(startX + d, startY + d), possibleMoves, board))
                break;
        }

        return possibleMoves;
    }

    private bool AddMoveIfValid(Vector2Int startPosition, Vector2Int position, List<Vector2Int> possibleMoves, Board board)
    {
        if (!Board.IsPositionInBounds(position))
            return false;

        Piece piece = board.GetPieceAtPosition(position);
        if (MovePiece(startPosition, position, board))
        {
            possibleMoves.Add(position);
            return true;
        }
        return false;
    }
}
