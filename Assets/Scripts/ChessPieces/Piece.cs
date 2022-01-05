using System.Collections.Generic;
using UnityEngine;

public enum ChessPieceType { 
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6,
}
public class Piece : MonoBehaviour
{
    public int team; // 0 bia³e, 1 czarne
    public int currentX;
    public int currentY;
    public ChessPieceType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

    private void Update()
    {
        // Wyg³adzenie poruszania
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        // Zmiana skali przy zbiciu
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    private void Start()
    {
        transform.rotation = Quaternion.Euler((team == 0) ? Vector3.zero : new Vector3(0, 180, 0));
    }
    public virtual List<Vector2Int> GetPossibleMoves(ref Piece[,] chessBoard, int tileCountX, int tileCountY) {
        return null;
    }

    public virtual SpecialMove GetSpecialMoves(ref Piece[,] chessBoard, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves) {
        return SpecialMove.None;
    }
    public virtual void SetPosition(Vector3 position, bool force = false) {
        desiredPosition = position;
        if (force) {
            transform.position = desiredPosition;
        }
    }
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }
}
