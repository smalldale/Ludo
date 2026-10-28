using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Tests.TestData;
using NUnit.Framework;

namespace Ludo.Core.Tests;

[TestFixture]
public class GameController_GetPieceState_Tests
{
    [Test]
    public void GetPieceState_WhenPieceIsInBase_ReturnsBase()
    {
        // Arrange
        GameController game = GameFactory.CreateGame(rolls: [3]);
        var piece = game.GetPiece(4);

        // Act
        PieceState result = game.GetPieceState(piece);

        // Assert
        Assert.That(result, Is.EqualTo(PieceState.Base));
    }

    [Test]
    public void GetPieceState_WhenPieceIsOnTrack_ReturnsOnTrack()
    {
        // Arrange
        var board = new Board();
        var players = PlayerFactory.CreateMany(2);
        var pieceId = players[0].Id * 4;
        var startIndex = board.StartIndex[players[0].Color];
        var trackPosition = board.Track[(startIndex + 1) % board.Track.Count];

        GameController game = GameFactory.CreateGameWithPiecePositions(
            rolls: [3],
            positions: new Dictionary<int, Coordinate>
            {
                [pieceId] = trackPosition
            },
            board: board,
            playerCount: 2);

        var piece = game.GetPiece(pieceId);

        // Act
        PieceState result = game.GetPieceState(piece);

        // Assert
        Assert.That(result, Is.EqualTo(PieceState.OnTrack));
    }

    [Test]
    public void GetPieceState_WhenPieceIsInHomeColumn_ReturnsHomeColumn()
    {
        // Arrange
        var board = new Board();
        var players = PlayerFactory.CreateMany(2);
        var pieceId = players[0].Id * 4;
        var homeColumnPosition = board.HomeStretch[players[0].Color][0];

        GameController game = GameFactory.CreateGameWithPiecePositions(
            rolls: [3],
            positions: new Dictionary<int, Coordinate>
            {
                [pieceId] = homeColumnPosition
            },
            board: board,
            playerCount: 2);

        var piece = game.GetPiece(pieceId);

        // Act
        PieceState result = game.GetPieceState(piece);

        // Assert
        Assert.That(result, Is.EqualTo(PieceState.HomeColumn));
    }

    [Test]
    public void GetPieceState_WhenPieceIsFinished_ReturnsFinished()
    {
        // Arrange
        var board = new Board();
        var players = PlayerFactory.CreateMany(2);
        var pieceId = players[0].Id * 4;
        var finish = board.HomeStretch[players[0].Color][^1];

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
        PieceState result = game.GetPieceState(piece);

        // Assert
        Assert.That(result, Is.EqualTo(PieceState.Finished));
    }
}