using System.Collections.Generic;
using UnityEngine;
public class Pawn : Piece
{
    public override List<Vector2Int> GetPossibleMoves(ref Piece[,] chessBoard, int tileCountX, int tileCountY)
    {
        List<Vector2Int> posVector = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;

        // Ruch o jeden
        if (chessBoard[currentX, currentY + direction] == null) {
            posVector.Add(new Vector2Int(currentX, currentY + direction));
        }

        // Ruch o 2
        if (chessBoard[currentX, currentY + direction] == null){
            // Sprawdzenie czy jest bierka o 2 z przodu i czy pozycja Y jest taka sama
            if(team == 0 && currentY == 1 && chessBoard[currentX, currentY + (2 * direction)] == null){
                posVector.Add(new Vector2Int(currentX, currentY + 2*direction));
            }
            if (team == 1 && currentY == 6 && chessBoard[currentX, currentY + (2 * direction)] == null)
            {
                posVector.Add(new Vector2Int(currentX, currentY + 2 * direction));
            }
        }

        // Zbijanie przeciwnych bierek po prawej
        if (currentX != tileCountX - 1) {
            // check that there's a piece there and its the opposite team
            if (chessBoard[currentX + 1, currentY + direction] != null && chessBoard[currentX + 1, currentY + direction].team != team) {
                posVector.Add(new Vector2Int(currentX + 1, currentY + direction));
            }
        }
        // Zbijanie przeciwnych bierek po lewej
        if (currentX != 0)
        {
            if (chessBoard[currentX - 1, currentY + direction] != null && chessBoard[currentX -1, currentY + direction].team != team)
            {
                posVector.Add(new Vector2Int(currentX - 1, currentY + direction));
            }
        }
        return posVector;
    }

    public override SpecialMove GetSpecialMoves(ref Piece[,] chessBoard, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == 0) ? 1 : -1;

        // Promocja
        if((team==0 && currentY==6) || (team==1 && currentY == 1))
        {
            return SpecialMove.Promotion;
        }

        // En Passant
        if (moveList.Count > 0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            // Czy ostatni ruch to by³ pion
            if (chessBoard[lastMove[1].x, lastMove[1].y].type == ChessPieceType.Pawn) {
                // Czy ruszy³ siê o 2
                if (Mathf.Abs(lastMove[0].y - lastMove[1].y) == 2){
                    // czy ruch z przeciwnej druzyny
                    if (chessBoard[lastMove[1].x, lastMove[1].y].team != team){
                        // Czy ta sama pozcyja na Y
                        if (lastMove[1].y == currentY) {
                            // Ruch dalej do obs³ugi w Chessboard.cs
                            // Jest po prawej
                            if (lastMove[1].x == currentX - 1){
                                availableMoves.Add(new Vector2Int(currentX - 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                            // Jest po prawej
                            if (lastMove[1].x == currentX + 1){
                                availableMoves.Add(new Vector2Int(currentX + 1, currentY + direction));
                                return SpecialMove.EnPassant;
                            }
                        }
                    }
                }

            }

        }
        return SpecialMove.None;
    }
}
