using System.Collections.Generic;
using UnityEngine;
public class Queen : Piece
{
    public override List<Vector2Int> GetPossibleMoves(ref Piece[,] chessBoard, int tileCountX, int tileCountY)
    {
        List<Vector2Int> posVector = new List<Vector2Int>();
        // To samo co w wiezy i goncu, tylko razem
        // Dol
        for (int i = currentY - 1; i >= 0; i--)
        {
            if (chessBoard[currentX, i] == null)
            {
                posVector.Add(new Vector2Int(currentX, i));
            }

            if (chessBoard[currentX, i] != null)
            {
                if (chessBoard[currentX, i].team != team)
                {
                    posVector.Add(new Vector2Int(currentX, i));
                }
                break;
            }
        }
        // Gora
        for (int i = currentY + 1; i < tileCountY; i++)
        {
            if (chessBoard[currentX, i] == null)
            {
                posVector.Add(new Vector2Int(currentX, i));
            }

            if (chessBoard[currentX, i] != null)
            {
                if (chessBoard[currentX, i].team != team)
                {
                    posVector.Add(new Vector2Int(currentX, i));
                }
                break;
            }
        }
        // Lewo
        for (int i = currentX - 1; i >= 0; i--)
        {
            if (chessBoard[i, currentY] == null)
            {
                posVector.Add(new Vector2Int(i, currentY));
            }

            if (chessBoard[i, currentY] != null)
            {
                if (chessBoard[i, currentY].team != team)
                {
                    posVector.Add(new Vector2Int(i, currentY));
                }
                break;
            }
        }
        // Prawo
        for (int i = currentX + 1; i < tileCountX; i++)
        {
            if (chessBoard[i, currentY] == null)
            {
                posVector.Add(new Vector2Int(i, currentY));
            }

            if (chessBoard[i, currentY] != null)
            {
                if (chessBoard[i, currentY].team != team)
                {
                    posVector.Add(new Vector2Int(i, currentY));
                }
                break;
            }
        }

        // Gora prawo
        for (int x = currentX + 1, y = currentY + 1; x < tileCountX && y < tileCountY; x++, y++)
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
        // Gora lewo
        for (int x = currentX - 1, y = currentY + 1; x >= 0 && y < tileCountY; x--, y++)
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
        // Dol prawo
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
        // Dol lewo
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
