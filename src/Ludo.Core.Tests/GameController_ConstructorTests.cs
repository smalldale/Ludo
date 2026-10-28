using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Tests.TestData;

namespace Ludo.Core.Tests;

[TestFixture]
public class GameController_Constructor_Tests
{
    private IBoard _board;
    private IDice _dice;

    [SetUp]
    public void SetUp()
    {
        _board = new Board();
        _dice = new Dice(6);
    }

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public void Constructor_WithValidPlayerCount_CreatesGameController(
        int playerCount)
    {
        // Arrange
        List<IPlayer> players = PlayerFactory.CreateMany(playerCount);

        Dictionary<PlayerColor, List<IPiece>> piecesByColor = PiecesFactory.CreateForPlayers(players);

        // Act
        var game = new GameController(
            players, _board, _dice, piecesByColor);

        // Assert
        Assert.That(game, Is.Not.Null);
        Assert.That(game.players, Has.Count.EqualTo(playerCount));
    }

    [TestCase(1)]
    [TestCase(5)]
    public void Constructor_WithInvalidPlayerCount_ThrowsArgumentException(
        int playerCount)
    {
        // Arrange
        List<IPlayer> players = PlayerFactory.CreateMany(playerCount);

        Dictionary<PlayerColor, List<IPiece>> piecesByColor = PiecesFactory.CreateForPlayers(players);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new GameController(players, _board, _dice, piecesByColor));
    }

    [Test]
    public void Constructor_WithDuplicatePlayerColors_ThrowsArgumentException()
    {
        // Arrange
        List<IPlayer> players =
        [
            new Player(1, "Player 1", PlayerColor.Red),
            new Player(2, "Player 2", PlayerColor.Red)
        ];

        Dictionary<PlayerColor, List<IPiece>> piecesByColor =
            PiecesFactory.CreateForPlayers(players);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new GameController(players, _board, _dice, piecesByColor));
    }

    [Test]
    public void Constructor_WithDuplicatePlayerIds_ThrowsArgumentException()
    {
        // Arrange
        List<IPlayer> players =
        [
            new Player(1, "Player 1", PlayerColor.Red),
            new Player(1, "Player 2", PlayerColor.Green)
        ];

        Dictionary<PlayerColor, List<IPiece>> piecesByColor =
            PiecesFactory.CreateForPlayers(players);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new GameController(players, _board, _dice, piecesByColor));
    }
}