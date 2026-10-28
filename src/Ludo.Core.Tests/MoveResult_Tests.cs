
using Ludo.Core.Application;

namespace Ludo.Core.Tests;

[TestFixture]
public class MoveResultTests
{
    [Test]
    public void ToString_WhenSuccessfulMove_ReturnsFormattedResult()
    {
        // Arrange
        var result = new MoveResult(
            success: true,
            pieceId: 3,
            captured: true,
            pieceFinished: false,
            extraTurn: true,
            error: null);

        // Act
        string actual = result.ToString();

        // Assert
        Assert.That(
            actual,
            Is.EqualTo(
                "OK (piece 3, captured=True, finished=False, extraTurn=True)"));
    }

    [Test]
    public void ToString_WhenMoveFails_ReturnsRejectionWithError()
    {
        // Arrange
        const string error = "Roll the dice first";
        MoveResult result = MoveResult.Fail(error);

        // Act
        string actual = result.ToString();

        // Assert
        Assert.That(actual, Is.EqualTo($"REJECTED: {error}"));
    }

    [Test]
    public void Fail_WhenCalled_SetsFailureProperties()
    {
        // Arrange
        const string error = "Not your turn.";

        // Act
        MoveResult result = MoveResult.Fail(error);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.PieceId, Is.EqualTo(-1));
            Assert.That(result.Captured, Is.False);
            Assert.That(result.PieceFinished, Is.False);
            Assert.That(result.ExtraTurn, Is.False);
            Assert.That(result.Error, Is.EqualTo(error));
        });
    }
}
