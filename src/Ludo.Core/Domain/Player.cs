using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;

namespace Ludo.Core.Domain;

public class Player: IPlayer
{
    public int Id { get; }
    public string Name { get; set; }
    public PlayerColor Color { get; }

    public Player(int id, string name, PlayerColor color)
    {
        Id = id;
        Name = name;
        Color = color;
    }
}