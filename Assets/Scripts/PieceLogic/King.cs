﻿using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class King : Piece
{
    public override bool CanCapture(Vector2Int myPiecePosition, Vector2Int opponentPiecePosition, Board board)
    {
        int startX = Mathf.RoundToInt(myPiecePosition.x);
        int startY = Mathf.RoundToInt(myPiecePosition.y);
        int endX = Mathf.RoundToInt(opponentPiecePosition.x);
        int endY = Mathf.RoundToInt(opponentPiecePosition.y);
        bool friendlyFire = !board.IsEmptyCell(opponentPiecePosition) && board.GetPieceAtPosition(opponentPiecePosition).Side == Side;
        return Mathf.Abs(endX - startX) <= 1 && Mathf.Abs(endY - startY) <= 1 && !friendlyFire;
    }

    public bool CanCastle(Vector2 startPositon, Vector2 endPosition, Board board)
    {
        if (Side == Side.white && startPositon.y != 0 || Side == Side.black && startPositon.y != 7) return false;
        if (Side == Side.white && endPosition.y != 0 || Side == Side.black && endPosition.y != 7) return false;
        if (Side == Side.white && startPositon.x != 4) return false;
        bool isShortCastle = startPositon.x - endPosition.x < 0;
        int rank = Side == 0 ? 0 : 7;
        int kingStartFile = 4;
        int rookStartFile = isShortCastle ? 7 : 0;
        int kingEndFile = isShortCastle ? 6 : 2;

        Piece king = board.GameBoard[rank*8+kingStartFile];
        Piece rook = board.GameBoard[rank*8+rookStartFile];
        if (king == null || rook == null || board.MovedPieces.Contains(king) || board.MovedPieces.Contains(rook))
        {
            //Cannot castle: king or rook has moved.
            return false;
        }
        for (int file = Mathf.Min(kingStartFile, rookStartFile) + 1; file < Mathf.Max(kingStartFile, rookStartFile); file++)
        {
            if (board.GameBoard[rank*8+file] != null)
            {
                //Cannot castle: there are pieces between the king and rook.
                return false;
            }
        }
        Vector2Int kingStartPosition = new Vector2Int(kingStartFile, rank);
        Vector2Int kingEndPosition = new Vector2Int(kingEndFile, rank);

        if (board.IsAttackedCell(king, kingStartPosition) || board.IsAttackedCell(king, kingEndPosition))
        {
            //Cannot castle: king would move through or into an attacked square.
            return false;
        }
        return true;
    }

    public override bool MovePiece(Vector2Int startPosition, Vector2Int endPosition, Board board)
    {
        int startX = Mathf.RoundToInt(startPosition.x);
        int startY = Mathf.RoundToInt(startPosition.y);
        int endX = Mathf.RoundToInt(endPosition.x);
        int endY = Mathf.RoundToInt(endPosition.y);

        if (!Board.IsPositionInBounds(startPosition) || !Board.IsPositionInBounds(endPosition)) return false;
        if (startPosition == endPosition) return false;
        if (board.IsAttackedCell(this, endPosition))
        {
            return false;
        }

        if (Mathf.Abs(endX - startX) <= 1 && Mathf.Abs(endY - startY) <= 1)
        {
            if (board.GameBoard[endY*8+endX] == null || board.GameBoard[endY*8+endX].Side != this.Side)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (CanCastle(startPosition, endPosition, board))
        {
            return true;
        }
        else if (CanCastle(startPosition, endPosition, board))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public override List<Vector2Int> GetPossibleMoves(Vector2Int currentPosition, Board board)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        int startX = Mathf.RoundToInt(currentPosition.x);
        int startY = Mathf.RoundToInt(currentPosition.y);

        for (int deltaX = -1; deltaX <= 1; deltaX++)
        {
            for (int deltaY = -1; deltaY <= 1; deltaY++)
            {
                if (deltaX == 0 && deltaY == 0)
                {
                    continue;
                }

                int newX = startX + deltaX;
                int newY = startY + deltaY;

                if (Board.IsPositionInBounds(new Vector2Int(newX, newY)))
                {
                    Piece pieceAtNewPosition = board.GetPieceAtPosition(new Vector2Int(newX, newY));
                    Vector2Int endPosition = new Vector2Int(newX, newY);
                    if (MovePiece(currentPosition, endPosition, board))
                    {
                        possibleMoves.Add(endPosition);
                    }
                }
            }
        }

        return possibleMoves;
    }

}
