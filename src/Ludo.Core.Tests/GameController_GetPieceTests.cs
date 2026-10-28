using Ludo.Core.Application;
using Ludo.Core.Tests.TestData;
using NUnit.Framework;

namespace Ludo.Core.Tests;

[TestFixture]
public class GameController_GetPiece_Tests
{
    GameController _game;

    /// <summary>
    /// Setup GameController with 2 Player and SequenceDice [3].
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _game = GameFactory.CreateGame(rolls: [3]);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    [TestCase(7)]
    [TestCase(8)]
    public void GetPiece_WhenPieceExists_ReturnsPiece(int pieceId)
    {
        // Arrange
        var expectedPiece = _game.GetPiece(pieceId);

        // Act
        var actualPiece = _game.GetPiece(expectedPiece.Id);

        // Assert
        Assert.That(actualPiece, Is.SameAs(expectedPiece));
    }

    [TestCase(0)]
    [TestCase(9)]
    [TestCase(10)]
    [TestCase(11)]
    [TestCase(12)]
    public void GetPiece_WhenPieceDoesNotExist_ThrowsArgumentException(int invalidPieceId)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _game.GetPiece(invalidPieceId));
    }
}