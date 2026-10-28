using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Tests.TestData;
using NUnit.Framework;

namespace Ludo.Core.Tests;

[TestFixture]
public class GameController_GetPosition_Tests
{
    [Test]
    public void GetPosition_WhenPieceIsInBase_ReturnsNegativeOne()
    {
        // Arrange
        GameController game = GameFactory.CreateGame(rolls: [3]);
        var piece = game.GetPiece(4);

        // Act
        int result = game.GetPosition(piece);

        // Assert
        Assert.That(result, Is.EqualTo(-1));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(10)]
    [TestCase(50)]
    public void GetPosition_WhenPieceIsOnTrack_ReturnsRelativePosition(
        int expectedPosition)
    {
        // Arrange
        var board = new Board();
        var players = PlayerFactory.CreateMany(2);
        var color = players[0].Color;
        int pieceId = players[0].Id * 4;

        int trackIndex =
            (board.StartIndex[color] + expectedPosition) % board.Track.Count;

        Coordinate coordinate = board.Track[trackIndex];

        GameController game = GameFactory.CreateGameWithPiecePositions(
            rolls: [3],
            positions: new Dictionary<int, Coordinate>
            {
                [pieceId] = coordinate
            },
            board: board,
            playerCount: 2);

        var piece = game.GetPiece(pieceId);

        // Act
        int result = game.GetPosition(piece);

        // Assert
        Assert.That(result, Is.EqualTo(expectedPosition));
    }

    [TestCase(0, 51)]
    [TestCase(1, 52)]
    [TestCase(4, 55)]
    public void GetPosition_WhenPieceIsInHomeColumn_ReturnsRelativePosition(
        int homeStretchIndex,
        int expectedPosition)
    {
        // Arrange
        var board = new Board();
        var players = PlayerFactory.CreateMany(2);
        var color = players[0].Color;
        int pieceId = players[0].Id * 4;

        Coordinate coordinate = board.HomeStretch[color][homeStretchIndex];

        GameController game = GameFactory.CreateGameWithPiecePositions(
            rolls: [3],
            positions: new Dictionary<int, Coordinate>
            {
                [pieceId] = coordinate
            },
            board: board,
            playerCount: 2);

        var piece = game.GetPiece(pieceId);

        // Act
        int result = game.GetPosition(piece);

        // Assert
        Assert.That(result, Is.EqualTo(expectedPosition));
    }

    [Test]
    public void GetPosition_WhenPieceIsFinished_Returns56()
    {
        // Arrange
        var board = new Board();
        var players = PlayerFactory.CreateMany(2);
        var color = players[0].Color;
        int pieceId = players[0].Id * 4;

        Coordinate finish = board.HomeStretch[color][^1];

        GameController game = GameFactory.CreateGameWithPiecePositions(
            rolls: [3],
            positions: new Dictionary<int, Coordinate>
            {
                [pieceId] = finish
            },
            board: board,
            playerCount: 2);

        var piece = game.GetPiece(pieceId);

        // Act
        int result = game.GetPosition(piece);

        // Assert
        Assert.That(result, Is.EqualTo(56));
    }
}