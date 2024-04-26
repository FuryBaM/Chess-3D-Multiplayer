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
        bool friendlyFire = board.GameBoard[endY, endX] != null && board.GameBoard[endY, endX].Side == Side;
        return (startX == endX || startY == endY) && !(startX == endX && startY == endY) && board.GetPieceAtPosition(endPosition).Side != Side && !friendlyFire;
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
                        Debug.Log("There is a piece blocking the rook's path by own piece");
                        return false;
                    }
                    else if (board.GameBoard[y, x].Side == 1 - Side)
                    {
                        Debug.Log("There is a piece blocking the rook's path by enemy piece");
                        return false;
                    }
                }
                x += deltaX;
                y += deltaY;
            }
            if (board.GameBoard[endY, endX] == null || board.GameBoard[endY, endX].Side != this.Side)
            {
                Debug.Log("Valid move for the rook.");
                return true;
            }
            else
            {
                Debug.Log("Target position is occupied by own piece.");
                return false;
            }
        }
        else
        {
            Debug.Log("Invalid move for the rook.");
            return false;
        }
    }

    public override List<Vector2Int> GetPossibleMoves(Vector2Int currentPosition, Board board)
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        // Define initial coordinates
        int startX = Mathf.RoundToInt(currentPosition.x);
        int startY = Mathf.RoundToInt(currentPosition.y);

        // Add possible moves for the rook (horizontally and vertically)
        AddMovesInDirection(startX, startY, 1, 0, possibleMoves, board); // Right
        AddMovesInDirection(startX, startY, -1, 0, possibleMoves, board); // Left
        AddMovesInDirection(startX, startY, 0, 1, possibleMoves, board); // Up
        AddMovesInDirection(startX, startY, 0, -1, possibleMoves, board); // Down

        return possibleMoves;
    }

    private void AddMovesInDirection(int startX, int startY, int deltaX, int deltaY, List<Vector2Int> possibleMoves, Board board)
    {
        int x = startX + deltaX;
        int y = startY + deltaY;

        while (Board.IsPositionInBounds(new Vector2Int(x, y)))
        {
            Vector2Int newPosition = new Vector2Int(x, y);

            // Check if the move is legal
            if (MovePiece(new Vector2Int(startX, startY), newPosition, board))
            {
                possibleMoves.Add(newPosition);

                // If the position is occupied, stop further moves in this direction
                if (board.GameBoard[y, x] != null)
                    break;
            }
            else
            {
                // If the move is illegal, stop further moves in this direction
                break;
            }

            // Move to the next position
            x += deltaX;
            y += deltaY;
        }
    }
}
