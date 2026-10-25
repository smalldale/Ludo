namespace Ludo.Core.Domain.Enums;

/// <summary>
/// Enum for state of Piece depends on where the piece on Cell Board.
/// </summary>
public enum PieceState
{
    Base, // Piece on Base CellType
    OnTrack, // Piece on track progress (Start, Normal, Safe CellType)
    HomeColumn, // Piece on HomeColumn CellType
    Finished // Piece on HomeTriangle CellType
}