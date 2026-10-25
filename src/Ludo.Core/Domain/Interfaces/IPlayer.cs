using Ludo.Core.Domain.Enums;

namespace Ludo.Core.Domain.Interfaces;

public interface IPlayer
{
    public int Id { get; }
    public string Name { get; set; }
    public PlayerColor Color { get; }
}