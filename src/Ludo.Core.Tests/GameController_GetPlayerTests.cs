
using Ludo.Core.Application;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Tests.TestData;

namespace Ludo.Core.Tests.Application;

[TestFixture]
public class GameController_GetPlayerTests
{
    [Test]
    public void GetPlayer_WhenPlayerExists_ReturnsMatchingPlayer()
    {
        // Arrange
        var (game, players, _) = GameFactory.CreateGameWithContext([1]);

        IPlayer expectedPlayer = players[1];

        // Act
        IPlayer actualPlayer = game.GetPlayer(expectedPlayer.Id);

        // Assert
        Assert.That(actualPlayer, Is.SameAs(expectedPlayer));
    }

    [Test]
    public void GetPlayer_WhenPlayerDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var game = GameFactory.CreateGame([1]);

        int unknownPlayerId = 999;

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => game.GetPlayer(unknownPlayerId));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(
                exception!.Message,
                Does.Contain($"Player with id {unknownPlayerId} not found"));

            Assert.That(exception.ParamName, Is.EqualTo("playerId"));
        });
    }
}
