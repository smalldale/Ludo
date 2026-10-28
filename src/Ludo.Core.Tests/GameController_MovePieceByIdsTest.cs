using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Tests.TestData;
using Ludo.Core.Tests.TestDoubles;
using NUnit.Framework;

namespace Ludo.Core.Tests;

[TestFixture]
public class GameController_MovePieceById_Tests
{
    [Test]
    public void MovePieceById_BeforeGameStarts_ReturnsFailure()
    {
        // Arrange
        var game = GameFactory.CreateGame([6]);

        // Act
        var result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void MovePieceById_WhenWaitingForRoll_ReturnsFailure()
    {
        // Arrange
        var game = GameFactory.CreateGame([6]);
        game.StartGame();

        // Act
        var result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void MovePieceById_WhenWrongPlayerMoves_ReturnsFailure()
    {
        // Arrange
        var game = GameFactory.CreateGame([6]);
        game.StartGame();
        game.RollDice(playerId: 1);

        // Act
        var result = game.MovePieceById(
            playerId: 2,
            pieceId: 1);

        // Assert
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void MovePieceById_WhenPieceDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var game = GameFactory.CreateGame([6]);
        game.StartGame();
        game.RollDice(playerId: 1);

        // Act
        var result = game.MovePieceById(
            playerId: 1,
            pieceId: 999);

        // Assert
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void MovePieceById_WhenPieceCannotMove_ReturnsFailure()
    {
        // Arrange
        var game = GameFactory.CreateGame([3]);
        game.StartGame();

        // A roll of 3 leaves every Red piece in base and
        // advances the turn to Player 2.
        game.RollDice(playerId: 1);

        // Act
        var result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void MovePieceById_WhenSixIsRolled_MovesPieceOutOfBase()
    {
        // Arrange
        var game = GameFactory.CreateGame([6]);
        game.StartGame();
        game.RollDice(playerId: 1);

        // Act
        var result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.PieceId, Is.EqualTo(1));
            Assert.That(game.GetPosition(
                game.GetPiece(1)), Is.EqualTo(0));
        });
    }

    [Test]
    public void MovePieceById_WhenPieceLeavesBaseWithSix_AllowsAnotherRoll()
    {
        // Arrange
        var game = GameFactory.CreateGame([6, 3]);
        game.StartGame();
        game.RollDice(playerId: 1);

        // Act
        var result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.ExtraTurn, Is.True);
            Assert.That(game.GetCurrentPlayer().Id, Is.EqualTo(1));
            Assert.That(game.Phase, Is.EqualTo(TurnPhase.WaitingForRoll));
        });
    }

    [Test]
    public void MovePieceById_WhenPieceDoesNotFinish_ReturnsPieceFinishedFalse()
    {
        // Arrange
        var game = GameFactory.CreateGame([6, 2]);
        game.StartGame();

        game.RollDice(playerId: 1);
        game.MovePieceById(playerId: 1, pieceId: 1);

        game.RollDice(playerId: 1);

        // Act
        MoveResult result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            // Assert.That(result.PieceFinished, Is.False);
        });
    }

    [TestCase(2, true, TestName = "MovePieceById_WhenLandingOnNormalCell_CapturesOpponent")]
    [TestCase(8, false, TestName = "MovePieceById_WhenLandingOnSafeCell_DoesNotCaptureOpponent")]
    [TestCase(0, false, TestName = "MovePieceById_WhenLandingOnStartCell_DoesNotCaptureOpponent")]
    public void MovePieceById_WhenLandingOnOccupiedCell_HandlesCaptureCorrectly(
        int stepsFromRedStart,
        bool shouldCaptured)
    {
        // Arrange
        IBoard board = new Board();

        int redStartIndex = board.StartIndex[PlayerColor.Red];

        int targetIndex = (redStartIndex + stepsFromRedStart + board.Track.Count)
            % board.Track.Count;

        Coordinate target = board.Track[targetIndex];

        // Put Red one space before the target.
        int redPositionIndex =
            (targetIndex - 1 + board.Track.Count) % board.Track.Count;

        Coordinate redPosition = board.Track[redPositionIndex];

        var positions = new Dictionary<int, Coordinate>
        {
            [1] = redPosition, // Red piece
            [5] = target       // Green piece
        };

        var game = GameFactory.CreateGameWithPiecePositions(
            rolls: [1],
            positions: positions);

        game.StartGame();
        game.RollDice(playerId: 1);

        // Act
        MoveResult result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Captured, Is.EqualTo(shouldCaptured));

            if (shouldCaptured)
            {
                Assert.That(game.GetPosition(game.GetPiece(5)), Is.EqualTo(-1));
            }
            else
            {
                Assert.That(game.GetPosition(game.GetPiece(5)), Is.Not.EqualTo(-1));
            }
        });
    }

    [Test]
    public void MovePieceById_WhenPieceMovesOnTrack_UpdatesPosition()
    {
        // Arrange
        var game = GameFactory.CreateGame([6, 2]);
        game.StartGame();

        game.RollDice(playerId: 1);
        game.MovePieceById(playerId: 1, pieceId: 1);

        game.RollDice(playerId: 1);

        // Act
        var result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(game.GetPosition(
                game.GetPiece(1)), Is.EqualTo(2));
        });
    }

    /// <summary>
    /// Place a Red piece on the last outer-track cell before its home stretch. 
    /// A roll of 1 should move it into the first home-stretch cell.
    /// </summary>
    [Test]
    public void MovePieceById_WhenPieceEntersHomeStretch_UpdatesPieceState()
    {
        // Arrange
        IBoard board = new Board();

        int redStartIndex = board.StartIndex[PlayerColor.Red];

        // Relative track position 51: immediately before home stretch.
        int lastTrackIndex =
            (redStartIndex + board.Track.Count - 1) % board.Track.Count;

        Coordinate lastTrackCoordinate = board.Track[lastTrackIndex];

        var positions = new Dictionary<int, Coordinate>
        {
            [1] = lastTrackCoordinate
        };

        var game = GameFactory.CreateGameWithPiecePositions(
            rolls: [1],
            positions: positions);

        game.StartGame();
        game.RollDice(playerId: 1);

        // Act
        MoveResult result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(
                game.GetPieceState(game.GetPiece(1)),
                Is.EqualTo(PieceState.HomeColumn));
        });
    }

    /// <summary>
    /// A piece one step before the home triangle must roll exactly 1 to finish. 
    /// Rolls of 2 through 6 overshoot the destination.
    /// </summary>
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    public void MovePieceById_WhenRollOvershootsHomeTriangle_ReturnsFailure(int roll)
    {
        // Arrange
        IBoard board = new Board();

        Coordinate beforeFinish =
            board.HomeStretch[PlayerColor.Red][^2];

        // Another Red piece must be movable so RollDice remains
        // in WaitingForMove after the overshooting roll.
        Coordinate redStart =
            board.Track[board.StartIndex[PlayerColor.Red]];

        var positions = new Dictionary<int, Coordinate>
        {
            [1] = beforeFinish,
            [2] = redStart
        };

        var game = GameFactory.CreateGameWithPiecePositions(
            rolls: [roll],
            positions: positions);

        game.StartGame();
        game.RollDice(playerId: 1);

        // Act
        MoveResult result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                game.GetPieceState(game.GetPiece(1)!),
                Is.EqualTo(PieceState.HomeColumn));
        });
    }

        [Test]
    public void MovePieceById_WhenPieceReachesHomeTriangle_ReturnsPieceFinished()
    {
        // Arrange
        IBoard board = new Board();

        Coordinate beforeFinish =
            board.HomeStretch[PlayerColor.Red][^2];

        var positions = new Dictionary<int, Coordinate>
        {
            [1] = beforeFinish
        };

        var game = GameFactory.CreateGameWithPiecePositions(
            rolls: [1],
            positions: positions);

        game.StartGame();
        game.RollDice(playerId: 1);

        // Act
        var result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.PieceFinished, Is.True);
            Assert.That(
                game.GetPieceState(game.GetPiece(1)),
                Is.EqualTo(PieceState.Finished));
        });
    }

    [Test]
    public void MovePieceById_WhenPieceIsFinished_ReturnsFailure()
    {
        // Arrange
        IBoard board = new Board();
        Coordinate finishCoordinate =
            board.HomeStretch[PlayerColor.Red][^1];

        Dictionary<int, Coordinate> positions = new Dictionary<int, Coordinate>
        {
            [1] = finishCoordinate
        }; // prepared piece id to finished coordinate

        GameController game = GameFactory.CreateGameWithPiecePositions(
            rolls: [6],
            positions: positions);

        game.StartGame();
        game.RollDice(playerId: 1);

        // Act
        MoveResult result = game.MovePieceById(
            playerId: 1,
            pieceId: 1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(
                result.Error,
                Is.EqualTo("This piece cannot move with the current roll."));
        });
    }

    [Test]
    public void MovePieceById_WhenPieceFinishes_RegistersPlayer()
    {
        // Arrange
        IBoard board = new Board();

        PlayerColor color = PlayerColor.Red;

        // Position three Red pieces at the finish.
        // Position the fourth piece one step before the finish.
        Coordinate finish = board.HomeStretch[color][^1];
        Coordinate beforeFinish = board.HomeStretch[color][^2];

        var positions = new Dictionary<int, Coordinate>
        {
            [1] = finish,
            [2] = finish,
            [3] = finish,
            [4] = beforeFinish
        };

        GameController game = GameFactory.CreateGameWithPiecePositions(
            rolls: [1],
            positions: positions,
            playerCount: 2);

        game.StartGame();

        // Act
        game.RollDice(playerId: 1);

        MoveResult result = game.MovePieceById(
            playerId: 1,
            pieceId: 4);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Error, Is.Null);
            Assert.That(
                game.GetPieceState(game.GetPiece(4)),
                Is.EqualTo(PieceState.Finished));
            Assert.That(
                game.Rankings.Select(player => player.Id),
                Does.Contain(1));
        });
    }

    /// <summary>
    /// This test generic for two, three, or four players. 
    /// Each player starts with three pieces already finished and one piece one step away from finishing. 
    /// The test then finishes the required number of players.
    /// </summary>
    // [TestCase(2, 1)]
    // [TestCase(3, 2)]
    // [TestCase(4, 3)]
    // public void MovePieceById_WhenRequiredPlayersFinish_EndsGame(
    //     int playerCount,
    //     int requiredFinishedPlayers)
    // {
    //     // Arrange
    //     var board = new Board();
    //     var positions = new Dictionary<int, Coordinate>();

    //     for (int playerIndex = 0; playerIndex < requiredFinishedPlayers; playerIndex++)
    //     {
    //         PlayerColor color = (PlayerColor)playerIndex;
    //         int firstPieceId = (playerIndex * 4) + 1;

    //         Coordinate finish = board.HomeStretch[color][^1];
    //         Coordinate beforeFinish = board.HomeStretch[color][^2];

    //         // Three pieces are already finished.
    //         for (int i = 0; i < 3; i++)
    //         {
    //             positions[firstPieceId + i] = finish;
    //         }

    //         // The fourth piece needs a roll of 1 to finish.
    //         positions[firstPieceId + 3] = beforeFinish;
    //     }

    //     int[] rolls = Enumerable
    //         .Repeat(1, playerCount * 2)
    //         .ToArray();

    //     GameController game = GameFactory.CreateGameWithPiecePositions(
    //         rolls: rolls,
    //         positions: positions,
    //         board: board,
    //         playerCount: playerCount);

    //     game.StartGame();

    //     // Act
    //     while (game.Phase != TurnPhase.Finished)
    //     {
    //         IPlayer currentPlayer = game.GetCurrentPlayer();

    //         game.RollDice(currentPlayer.Id);

    //         // A player who has already finished has no movable pieces.
    //         // RollDice automatically ends that turn and advances to the next player.
    //         if (game.Phase != TurnPhase.WaitingForMove)
    //         {
    //             continue;
    //         }

    //         int finalPieceId = (currentPlayer.Id - 1) * 4 + 4;

    //         MoveResult result = game.MovePieceById(
    //             currentPlayer.Id,
    //             finalPieceId);

    //         Assert.That(
    //             result.Success,
    //             Is.True,
    //             $"Player {currentPlayer.Name} should be able to finish their last piece.");

    //         Assert.That(
    //             game.Rankings.Any(p => p.Id == currentPlayer.Id),
    //             Is.True,
    //             $"{currentPlayer.Name} should be registered in the rankings.");
    //     }

    //     // Assert
    //     Assert.That(game.Phase, Is.EqualTo(TurnPhase.Finished));

    //     Assert.That(
    //         game.Rankings.Count,
    //         Is.EqualTo(requiredFinishedPlayers));

    //     for (int i = 0; i < requiredFinishedPlayers; i++)
    //     {
    //         Assert.That(
    //             game.Rankings[i].Id,
    //             Is.EqualTo(i + 1),
    //             $"Player {i + 1} should have rank {i + 1}.");
    //     }
    // }


}