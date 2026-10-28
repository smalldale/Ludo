using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Tests.TestData;
using NUnit.Framework;

namespace Ludo.Core.Tests;

[TestFixture]
public class GameController_GetFinishedPlayer_Tests
{
    [Test]
    public void GetFinishedPlayer_WhenNoPlayerHasFinished_ReturnsNull()
    {
        // Arrange
        GameController game = GameFactory.CreateGame(rolls: [3]);

        // Act
        var result = game.GetFinishedPlayer();

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetFinishedPlayer_WhenAllPlayerPiecesAreFinished_ReturnsPlayer()
    {
        // Arrange
        var board = new Board();
        var players = PlayerFactory.CreateMany(2);

        var positions = new Dictionary<int, Coordinate>();
        var expectedPlayer = players[0];
        var finish = board.HomeStretch[expectedPlayer.Color][^1];

        // Place all four pieces of the first player in the finish cell.
        for (int i = 0; i < 4; i++)
        {
            positions[expectedPlayer.Id * 4 + i] = finish;
        }

        GameController game = GameFactory.CreateGameWithPiecePositions(
            rolls: [3],
            positions: positions,
            board: board,
            playerCount: 2);

        // Act
        var result = game.GetFinishedPlayer();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(expectedPlayer.Id));
    }
}