using Ludo.Core.Domain.Enums;
using Ludo.Core.Tests.TestData;
using Ludo.Core.Tests.TestDoubles;
using NUnit.Framework;

namespace Ludo.Core.Tests;

[TestFixture]
public class GameController_RollDice_Tests
{
    [Test]
    public void RollDice_BeforeGameStarts_ThrowsInvalidOperationException()
    {
        // Arrange
        var game = GameFactory.CreateGame([3]);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => game.RollDice(playerId: 1));
    }

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public void RollDice_WhenWrongPlayerRolls_ThrowsInvalidOperationException(int playerId)
    {
        // Arrange
        var game = GameFactory.CreateGame([4]);
        game.StartGame();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => game.RollDice(playerId: playerId));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(3)]
    public void RollDice_WhenPlayerIdIsInvalid_ThrowsInvalidOperationException(
        int playerId)
    {
        // Arrange
        var game = GameFactory.CreateGame([3]);
        game.StartGame();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => game.RollDice(playerId));
    }

    [Test]
    public void RollDice_WhenWaitingForMove_ThrowsInvalidOperationException()
    {
        // Arrange
        var game = GameFactory.CreateGame([6, 3]);
        game.StartGame();

        game.RollDice(playerId: 1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => game.RollDice(playerId: 1));
    }

    [Test]
    public void RollDice_WhenSixIsRolled_AllowsMovingPiecesFromBase()
    {
        // Arrange
        var game = GameFactory.CreateGame([6]);
        game.StartGame();

        // Act
        int roll = game.RollDice(playerId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(roll, Is.EqualTo(6));
            Assert.That(game.Phase, Is.EqualTo(TurnPhase.WaitingForMove));
            Assert.That(game.GetMovablePieceIds(), Is.Not.Empty);
            Assert.That(game.GetCurrentPlayer().Id, Is.EqualTo(1));
        });
    }

    [Test]
    public void RollDice_WhenNoPiecesCanMove_AdvancesToNextPlayer()
    {
        // Arrange
        var game = GameFactory.CreateGame([3]);
        game.StartGame();

        // Act
        int roll = game.RollDice(playerId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(roll, Is.EqualTo(3));
            Assert.That(game.GetCurrentPlayer().Id, Is.EqualTo(2));
            Assert.That(game.Turn, Is.EqualTo(2));
            Assert.That(game.Phase, Is.EqualTo(TurnPhase.WaitingForRoll));
        });
    }

    [TestCase(new int[] { 6, 3 }, 1)]
    [TestCase(new int[] { 6, 6, 3 }, 2)]
    public void RollDice_AfterOneOrTwoSixes_AllowsPlayerToRollAgain(
        int[] rolls,
        int expectedSixes)
    {
        // Arrange
        var game = GameFactory.CreateGame(rolls);
        game.StartGame();

        // Roll and move once for each consecutive six.
        for (int i = 0; i < expectedSixes; i++)
        {
            game.RollDice(playerId: 1);
            game.MovePieceById(playerId: 1, pieceId: i + 1);
        }

        // Act
        int roll = game.RollDice(playerId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(roll, Is.EqualTo(3));
            Assert.That(game.GetCurrentPlayer().Id, Is.EqualTo(1));
            Assert.That(game.Turn, Is.EqualTo(1));
            Assert.That(game.Phase, Is.EqualTo(TurnPhase.WaitingForMove));
            Assert.That(game.ConsecutiveSixes, Is.Zero);
        });
    }

    [Test]
    public void RollDice_OnThirdConsecutiveSix_ForfeitsTurn()
    {
        // Arrange
        var game = GameFactory.CreateGame([6, 6, 6]);
        game.StartGame();

        // First six: move one piece to keep the turn going.
        game.RollDice(playerId: 1);
        game.MovePieceById(playerId: 1, pieceId: 1);

        // Second six: move another piece to keep the turn going.
        game.RollDice(playerId: 1);
        game.MovePieceById(playerId: 1, pieceId: 2);

        // Act
        int roll = game.RollDice(playerId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(roll, Is.EqualTo(6));
            Assert.That(game.GetCurrentPlayer().Id, Is.EqualTo(2));
            Assert.That(game.Turn, Is.EqualTo(2));
            Assert.That(game.Phase, Is.EqualTo(TurnPhase.WaitingForRoll));
            Assert.That(game.ConsecutiveSixes, Is.Zero);
        });
    }
}