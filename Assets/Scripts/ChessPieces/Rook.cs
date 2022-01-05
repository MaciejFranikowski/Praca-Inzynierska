using System.Collections.Generic;
using UnityEngine;
public class Rook : Piece
{
    public override List<Vector2Int> GetPossibleMoves(ref Piece[,] chessBoard, int tileCountX, int tileCountY)
    {
        List<Vector2Int> posVector = new List<Vector2Int>();

        // Kierunek w dol
        for (int i = currentY - 1; i >= 0; i--){
            if (chessBoard[currentX, i] == null) {
                posVector.Add(new Vector2Int(currentX, i));
            }

            if (chessBoard[currentX, i] != null)
            {
                if (chessBoard[currentX, i].team != team) {
                    posVector.Add(new Vector2Int(currentX, i));
                }
                // Stop dodawania ruchow po pierwszej napotkanej bierce
                break;
            }
        }

        // Góra
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

        return posVector;
    }
}
