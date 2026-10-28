
using Ludo.Core.Application;
using Ludo.Core.Tests.TestData;

namespace Ludo.Core.Tests.Application;

[TestFixture]
public class GameController_FindCellTests
{
    [Test]
    public void FindCell_WhenPieceIsNotOnBoard_ThrowsArgumentException()
    {
        // Arrange
        var game = GameFactory.CreateGame([1]);
        const int pieceId = 999;

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => game.FindCell(pieceId));

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(
                exception!.Message,
                Does.Contain($"Piece {pieceId} is not on the board."));

            Assert.That(exception.ParamName, Is.EqualTo("pieceId"));
        });
    }
}
