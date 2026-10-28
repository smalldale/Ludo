using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;

namespace Ludo.Core.Tests.TestData;

public static class PlayerFactory
{
    private static readonly PlayerColor[] Colors =
    [
        PlayerColor.Red,
        PlayerColor.Green,
        PlayerColor.Yellow,
        PlayerColor.Blue
    ];

    public static IPlayer Create(
        int id = 1,
        string name = "Test Player",
        PlayerColor color = PlayerColor.Red)
    {
        return new Player(id, name, color);
    }

    public static List<IPlayer> CreateMany(int count)
    {
        return Enumerable
            .Range(0, count)
            .Select(i => Create(
                id: i + 1,
                name: $"Player {i + 1}",
                color: Colors[i % Colors.Length]))
            .ToList();
    }
}