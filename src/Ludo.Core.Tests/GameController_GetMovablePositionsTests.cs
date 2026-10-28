
using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Tests.TestData;

namespace Ludo.Core.Tests.Application;

[TestFixture]
public class GameController_GetMovablePositionsTests
{
    [Test]
    public void GetMovablePositions_BeforeGameStarts_ReturnsEmptyList()
    {
        // Arrange
        var game = GameFactory.CreateGame([6]);

        // Act
        List<int> positions = game.GetMovablePositions();

        // Assert
        Assert.That(positions, Is.Empty);
    }

    [Test]
    public void GetMovablePositions_WhenSixRolled_ReturnsBasePositionOnce()
    {
        // Arrange
        var game = GameFactory.CreateGame([6]);
        game.StartGame();
        game.RollDice(1);

        // Act
        List<int> positions = game.GetMovablePositions();

        // Assert
        Assert.That(positions, Is.EqualTo(new[] { -1 }));
    }

    [Test]
    public void GetMovablePositions_WhenPieceCanMoveOnTrack_ReturnsItsCurrentPosition()
    {
        // Arrange
        IBoard board = new Board();

        Coordinate startingCoordinate =
            board.Track[board.StartIndex[PlayerColor.Red]];

        var positions = new Dictionary<int, Coordinate>
        {
            [1] = startingCoordinate
        };

        GameController game = GameFactory.CreateGameWithPiecePositions(
            [1],
            positions,
            board);

        game.StartGame();
        game.RollDice(1);

        // Act
        List<int> actualPositions = game.GetMovablePositions();

        // Assert
        Assert.That(actualPositions, Is.EqualTo(new[] { 0 }));
    }
}
