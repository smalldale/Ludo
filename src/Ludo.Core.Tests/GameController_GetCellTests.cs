
using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Tests.TestDoubles;
using Ludo.Core.Tests.TestData;

namespace Ludo.Core.Tests.Application;

[TestFixture]
public class GameController_GetCellTests
{
    [Test]
    public void Constructor_WhenBaseCellIsMissing_ThrowsInvalidOperationException()
    {
        // Arrange
        var players = PlayerFactory.CreateMany(2);
        var board = new Board();

        Coordinate missingCoordinate =
            board.BaseCells[PlayerColor.Red][0];

        board.Cells[missingCoordinate.Row, missingCoordinate.Col] = null!;

        var piecesByColor = PiecesFactory.CreateForPlayers(players);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(
            () => new GameController(
                players,
                board,
                new SequenceDice([1]),
                piecesByColor));

        // Assert
        Assert.That(
            exception!.Message,
            Is.EqualTo(
                $"No Cell at ({missingCoordinate.Row},{missingCoordinate.Col})."));
    }
}
