using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;

namespace Ludo.Core.Domain;

public class Piece: IPiece
{
    public int Id { get; }
    public PlayerColor Color { get; set; }
    
    public Piece(int id, PlayerColor color)
    {
        Id = id;
        Color = color;
    }
}