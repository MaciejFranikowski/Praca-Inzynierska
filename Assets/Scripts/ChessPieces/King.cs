using System.Collections.Generic;
using UnityEngine;
public class King : Piece
{
    public override List<Vector2Int> GetPossibleMoves(ref Piece[,] chessBoard, int tileCountX, int tileCountY)
    {
        List<Vector2Int> posVector = new List<Vector2Int>();

        // Skecja w prawo
        if (currentX + 1 < tileCountX)
        {
            // Prawo
            if (chessBoard[currentX + 1, currentY] == null)
            {
                posVector.Add(new Vector2Int(currentX + 1, currentY));
            }
            else if (chessBoard[currentX + 1, currentY].team != team)
            {
                posVector.Add(new Vector2Int(currentX + 1, currentY));
            }
            // Gora prawo
            if (currentY + 1 < tileCountY) {
                if (chessBoard[currentX + 1, currentY + 1] == null)
                {
                    posVector.Add(new Vector2Int(currentX + 1, currentY + 1));
                }
                else if (chessBoard[currentX + 1, currentY + 1].team != team)
                {
                    posVector.Add(new Vector2Int(currentX + 1, currentY + 1));
                }
            }
            // Dol prawo
            if (currentY - 1 >= 0)
            {
                if (chessBoard[currentX + 1, currentY - 1] == null)
                {
                    posVector.Add(new Vector2Int(currentX + 1, currentY - 1));
                }
                else if (chessBoard[currentX + 1, currentY - 1].team != team)
                {
                    posVector.Add(new Vector2Int(currentX + 1, currentY - 1));
                }
            }
        }
        // Lewo
        if (currentX - 1 >= 0)
        {
            // Lewo
            if (chessBoard[currentX - 1, currentY] == null)
            {
                posVector.Add(new Vector2Int(currentX - 1, currentY));
            }
            else if (chessBoard[currentX - 1, currentY].team != team)
            {
                posVector.Add(new Vector2Int(currentX - 1, currentY));
            }
            // Gora lewo
            if (currentY + 1 < tileCountY)
            {
                if (chessBoard[currentX - 1, currentY + 1] == null)
                {
                    posVector.Add(new Vector2Int(currentX - 1, currentY + 1));
                }
                else if (chessBoard[currentX - 1, currentY + 1].team != team)
                {
                    posVector.Add(new Vector2Int(currentX - 1, currentY + 1));
                }
            }
            // Dol lewo
            if (currentY - 1 >= 0)
            {
                if (chessBoard[currentX - 1, currentY - 1] == null)
                {
                    posVector.Add(new Vector2Int(currentX - 1, currentY - 1));
                }
                else if (chessBoard[currentX - 1, currentY - 1].team != team)
                {
                    posVector.Add(new Vector2Int(currentX - 1, currentY - 1));
                }
            }

        }
        //Gora
        if (currentY + 1 < tileCountY) {
            if (chessBoard[currentX, currentY + 1] == null || chessBoard[currentX, currentY + 1].team != team)
            {
                posVector.Add(new Vector2Int(currentX, currentY + 1));
            }
        }
        //Dol
        if (currentY - 1 >= 0)
        {
            if (chessBoard[currentX, currentY - 1] == null || chessBoard[currentX, currentY - 1].team != team)
            {
                posVector.Add(new Vector2Int(currentX, currentY - 1));
            }
        }


        return posVector;
    }
    public override SpecialMove GetSpecialMoves(ref Piece[,] chessBoard, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        SpecialMove move = SpecialMove.None;

        // Szukanie ruchów które zaczê³y siê od (4,0) or (4,7) dla króla itd
        var kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == 0) ? 0 : 7));
        var leftRookMove = moveList.Find(m => m[0].x == 0 && m[0].y == ((team == 0) ? 0 : 7));
        var rightRookMove = moveList.Find(m => m[0].x == 7 && m[0].y == ((team == 0) ? 0 : 7));

        // Jesli krol nie mial takiego ruchu
        if(kingMove == null && currentX == 4){
            if(team == 0)
            {
                // Lewa wieza sie nie ruszala
                if (leftRookMove == null) {
                    // Sprawdzenie czy to na pewno wieza
                    if (chessBoard[0, 0].type == ChessPieceType.Rook) { 
                        // Ta sama druzyna
                        if(chessBoard[0,0].team == 0)
                        {
                            // Brak bierek pomiedzy
                            if(chessBoard[3,0] == null && chessBoard[2, 0] == null && chessBoard[1, 0] == null)
                            {
                                // Do mozliwych ruchow dodanie dla krola ruchu o 2
                                availableMoves.Add(new Vector2Int(2, 0));
                                // Roszada do obs³ugi dalej w ChessBoard.cs
                                move = SpecialMove.Castle;
                            }
                        }
                    }
                }
                // Alterantywnie dla prawej wiezy
                if (rightRookMove == null)
                {
                    if (chessBoard[7, 0].type == ChessPieceType.Rook)
                    {
                        if (chessBoard[7, 0].team == 0)
                        {
                            if (chessBoard[5, 0] == null && chessBoard[6, 0] == null)
                            {
                                availableMoves.Add(new Vector2Int(6, 0));
                                move = SpecialMove.Castle;
                            }
                        }
                    }
                }
            } else // Druzyna czarna
            {
                if (leftRookMove == null)
                {
                    if (chessBoard[0, 7].type == ChessPieceType.Rook)
                    {
                        if (chessBoard[0, 7].team == 1)
                        {
                            if (chessBoard[3, 7] == null && chessBoard[2, 7] == null && chessBoard[1, 7] == null)
                            {
                                availableMoves.Add(new Vector2Int(2, 7));
                                move = SpecialMove.Castle;
                            }
                        }
                    }
                }
                if (rightRookMove == null)
                {
                    if (chessBoard[7, 7].type == ChessPieceType.Rook)
                    {
                        if (chessBoard[7, 7].team == 1)
                        {
                            if (chessBoard[5, 7] == null && chessBoard[6, 7] == null)
                            {
                                availableMoves.Add(new Vector2Int(6, 7));
                                move = SpecialMove.Castle;
                            }
                        }
                    }
                }
            }

        }
        return move;
    }
}
