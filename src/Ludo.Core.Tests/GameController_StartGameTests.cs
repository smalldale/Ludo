using Ludo.Core.Domain.Enums;
using Ludo.Core.Tests.TestData;
using Ludo.Core.Tests.TestDoubles;
using Ludo.Core.Application;

namespace Ludo.Core.Tests;

[TestFixture]
public class GameController_StartGame_Tests
{
    [Test]
    public void StartGame_WhenCalled_InitializesFirstTurn()
    {
        // Arrange
        GameController game = GameFactory.CreateGame([]);

        // Act
        game.StartGame();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(game.Turn, Is.EqualTo(1));

            // Assert.That(game.currentTurnIndex) private
            // Assert.That(game.pendingRoll) private

            Assert.That(game.ConsecutiveSixes, Is.Zero);

            Assert.That(
                game.Phase,
                Is.EqualTo(TurnPhase.WaitingForRoll));

            Assert.That(
                game.GetCurrentPlayer().Id,
                Is.EqualTo(1));
        });
    }

    [Test]
    public void StartGame_WhenCalled_SetsFirstPlayerAsCurrentPlayer()
    {
        // Arrange
        var game = GameFactory.CreateGame([]);

        // Act
        game.StartGame();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(game.GetCurrentPlayer().Id, Is.EqualTo(1));
            Assert.That(game.Turn, Is.EqualTo(1));
            Assert.That(game.Phase, Is.EqualTo(TurnPhase.WaitingForRoll));
        });
    }


}
