using Ludo.Core.Domain.Enums;

namespace Ludo.Core.Domain.Interfaces;

public interface IPiece
{
    public int Id { get; }
    public PlayerColor Color { get; }
}