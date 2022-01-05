using System.Collections.Generic;
using UnityEngine;
public class Bishop : Piece
{
    public override List<Vector2Int> GetPossibleMoves(ref Piece[,] chessBoard, int tileCountX, int tileCountY)
    {
        List<Vector2Int> posVector = new List<Vector2Int>();

        // Gora-prawo
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++){
            if (chessBoard[x, y] == null)
            {
                posVector.Add(new Vector2Int(x, y));
            }
            else {
                if (chessBoard[x, y].team != team)
                {
                    posVector.Add(new Vector2Int(x, y));
                }
                break;
            }
        }
        // Gora-lewo
        for (int x = currentX - 1, y = currentY + 1; x >=0 && y < tileCountY; x--, y++)
        {
            if (chessBoard[x, y] == null)
            {
                posVector.Add(new Vector2Int(x, y));
            }
            else
            {
                if (chessBoard[x, y].team != team)
                {
                    posVector.Add(new Vector2Int(x, y));
                }
                break;
            }
        }
        // Dol-prawo
        for (int x = currentX + 1, y = currentY - 1; x < tileCountX && y >= 0; x++, y--)
        {
            if (chessBoard[x, y] == null)
            {
                posVector.Add(new Vector2Int(x, y));
            }
            else
            {
                if (chessBoard[x, y].team != team)
                {
                    posVector.Add(new Vector2Int(x, y));
                }
                break;
            }
        }
        // Dol-lewo
        for (int x = currentX - 1, y = currentY - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (chessBoard[x, y] == null)
            {
                posVector.Add(new Vector2Int(x, y));
            }
            else
            {
                if (chessBoard[x, y].team != team)
                {
                    posVector.Add(new Vector2Int(x, y));
                }
                break;
            }
        }

        return posVector;
    }
}
