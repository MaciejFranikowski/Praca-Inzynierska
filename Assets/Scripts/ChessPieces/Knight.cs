using System.Collections.Generic;
using UnityEngine;
public class Knight : Piece
{
    public override List<Vector2Int> GetPossibleMoves(ref Piece[,] chessBoard, int tileCountX, int tileCountY)
    {
        List<Vector2Int> posVector = new List<Vector2Int>();

        // Gora-prawo gorny ruch
        int x = currentX + 1;
        int y = currentY + 2;
        if (x < tileCountX && y < tileCountY) {
            if (chessBoard[x, y] == null || chessBoard[x, y].team != team) {
                posVector.Add(new Vector2Int(x, y));
            }
        }

        // Gora-prawo dolny ruch
        x = currentX + 2;
        y = currentY + 1;
        if (x < tileCountX && y < tileCountY)
        {
            if (chessBoard[x, y] == null || chessBoard[x, y].team != team)
            {
                posVector.Add(new Vector2Int(x, y));
            }
        }

        // Gora-lewa gorny ruch
        x = currentX - 1;
        y = currentY + 2;
        if (x >= 0 && y < tileCountY)
        {
            if (chessBoard[x, y] == null || chessBoard[x, y].team != team)
            {
                posVector.Add(new Vector2Int(x, y));
            }
        }

        // Gora-lewa dolny ruch
        x = currentX - 2;
        y = currentY + 1;
        if (x >= 0 && y < tileCountY)
        {
            if (chessBoard[x, y] == null || chessBoard[x, y].team != team)
            {
                posVector.Add(new Vector2Int(x, y));
            }
        }

        x = currentX + 1;
        y = currentY - 2;
        if (x < tileCountX && y >= 0)
        {
            if (chessBoard[x, y] == null || chessBoard[x, y].team != team)
            {
                posVector.Add(new Vector2Int(x, y));
            }
        }

        x = currentX + 2;
        y = currentY - 1;
        if (x < tileCountX && y >= 0)
        {
            if (chessBoard[x, y] == null || chessBoard[x, y].team != team)
            {
                posVector.Add(new Vector2Int(x, y));
            }
        }

        x = currentX - 1;
        y = currentY - 2;
        if (x >= 0 && y >= 0)
        {
            if (chessBoard[x, y] == null || chessBoard[x, y].team != team)
            {
                posVector.Add(new Vector2Int(x, y));
            }
        }

        x = currentX - 2;
        y = currentY - 1;
        if (x >= 0 && y >= 0)
        {
            if (chessBoard[x, y] == null || chessBoard[x, y].team != team)
            {
                posVector.Add(new Vector2Int(x, y));
            }
        }

        return posVector;
    }
}
