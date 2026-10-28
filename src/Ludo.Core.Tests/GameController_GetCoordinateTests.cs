
using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Tests.TestData;

namespace Ludo.Core.Tests.Application;

[TestFixture]
public class GameController_GetCoordinateTests
{
    [Test]
    public void MovePieceById_WhenCapturedPlayerHasNoEmptyBaseCell_ThrowsInvalidOperationException()
    {
        // Arrange
        IBoard board = new Board();

        // Find a normal landing cell on Red's track.
        int relativePosition = Enumerable.Range(0, 50)
            .First(position =>
            {
                int trackIndex =
                    (board.StartIndex[PlayerColor.Red] + position + 1)
                    % board.Track.Count;

                Coordinate target = board.Track[trackIndex];

                return board.Cells[target.Row, target.Col].Type
                    == CellType.Normal;
            });

        Coordinate redPosition =
            board.Track[
                (board.StartIndex[PlayerColor.Red] + relativePosition)
                % board.Track.Count];

        Coordinate greenTarget =
            board.Track[
                (board.StartIndex[PlayerColor.Red] + relativePosition + 1)
                % board.Track.Count];

        // Green piece 5 is captured; piece 2 occupies its vacated base cell.
        var positions = new Dictionary<int, Coordinate>
        {
            [1] = redPosition,
            [2] = board.BaseCells[PlayerColor.Green][0],
            [5] = greenTarget
        };

        GameController game = GameFactory.CreateGameWithPiecePositions(
            [1],
            positions,
            board);

        game.StartGame();
        game.RollDice(1);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(
            () => game.MovePieceById(1, 1));

        // Assert
        Assert.That(
            exception!.Message,
            Is.EqualTo("No empty Base cell for Green."));
    }
}
