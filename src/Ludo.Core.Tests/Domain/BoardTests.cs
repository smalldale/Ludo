
using Ludo.Core.Domain;
using Ludo.Core.Domain.Interfaces;

namespace Ludo.Core.Tests.Domain;

[TestFixture]
public class BoardTests
{
    [Test]
    public void Size_WhenAccessed_Returns15()
    {
        // Arrange
        IBoard board = new Board();

        // Act
        int size = board.Size;

        // Assert
        Assert.That(size, Is.EqualTo(15));
    }
}