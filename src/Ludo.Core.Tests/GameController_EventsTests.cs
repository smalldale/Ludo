using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Tests.TestData;
using NUnit.Framework;

namespace Ludo.Core.Tests;

[TestFixture]
public class GameController_Event_Tests
{
    [Test]
    public void StartGame_WhenStarted_FiresOnTurnChangedWithCurrentPlayerId()
    {
        var (game, players, _) = GameFactory.CreateGameWithContext([1]);

        int? actualPlayerId = null;
        game.OnTurnChanged += playerId => actualPlayerId = playerId;

        game.StartGame();

        Assert.That(actualPlayerId, Is.EqualTo(players[0].Id));
    }

    [Test]
    public void RollDice_WhenRolled_FiresOnDiceRolledWithPlayerIdAndRoll()
    {
        var (game, players, _) = GameFactory.CreateGameWithContext([6]);
        game.StartGame();

        int? actualPlayerId = null;
        int? actualRoll = null;
        game.OnDiceRolled += (playerId, roll) =>
        {
            actualPlayerId = playerId;
            actualRoll = roll;
        };

        game.RollDice(players[0].Id);

        Assert.Multiple(() =>
        {
            Assert.That(actualPlayerId, Is.EqualTo(players[0].Id));
            Assert.That(actualRoll, Is.EqualTo(6));
        });
    }

    [Test]
    public void RollDice_WhenNoPieceCanMove_FiresOnNoMovablePiecesAndChangesTurn()
    {
        var (game, players, _) = GameFactory.CreateGameWithContext([1]);
        game.StartGame();

        int? actualPlayerId = null;
        int? actualRoll = null;
        int? nextPlayerId = null;

        game.OnNoMovablePieces += (playerId, roll) =>
        {
            actualPlayerId = playerId;
            actualRoll = roll;
        };
        game.OnTurnChanged += playerId => nextPlayerId = playerId;

        game.RollDice(players[0].Id);

        Assert.Multiple(() =>
        {
            Assert.That(actualPlayerId, Is.EqualTo(players[0].Id));
            Assert.That(actualRoll, Is.EqualTo(1));
            Assert.That(nextPlayerId, Is.EqualTo(players[1].Id));
        });
    }

    [Test]
    public void MovePieceById_WhenPieceMoves_FiresOnPieceMovedWithPieceIdAndPosition()
    {
        var (game, players, _) = GameFactory.CreateGameWithContext([6]);
        game.StartGame();
        game.RollDice(players[0].Id);

        const int pieceId = 1;
        int? actualPieceId = null;
        int? actualPosition = null;

        game.OnPieceMoved += (id, position) =>
        {
            actualPieceId = id;
            actualPosition = position;
        };

        MoveResult result = game.MovePieceById(players[0].Id, pieceId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(actualPieceId, Is.EqualTo(pieceId));
            Assert.That(actualPosition, Is.EqualTo(0));
        });
    }

    [Test]
    public void MovePieceById_WhenPieceCapturesOpponent_FiresOnPieceCapturedWithVictimId()
    {
        var board = new Board();
        Coordinate redPosition = default;
        Coordinate landingPosition = default;
        bool foundNormalLanding = false;

        int redStartIndex = board.StartIndex[PlayerColor.Red];

        for (int relativePosition = 0; relativePosition < 50; relativePosition++)
        {
            int sourceIndex = (redStartIndex + relativePosition) % board.Track.Count;
            int destinationIndex = (redStartIndex + relativePosition + 1) % board.Track.Count;

            Coordinate candidateLanding = board.Track[destinationIndex];
            ICell landingCell = board.Cells[candidateLanding.Row, candidateLanding.Col];

            if (landingCell.Type != CellType.Normal)
            {
                continue;
            }

            redPosition = board.Track[sourceIndex];
            landingPosition = candidateLanding;
            foundNormalLanding = true;
            break;
        }

        Assert.That(foundNormalLanding, Is.True, "Expected to find a normal track cell for the capture test.");

        const int movingPieceId = 1;
        const int victimPieceId = 5;

        var positions = new Dictionary<int, Coordinate>
        {
            [movingPieceId] = redPosition,
            [victimPieceId] = landingPosition
        };

        var game = GameFactory.CreateGameWithPiecePositions([1], positions, board);
        game.StartGame();

        int? actualVictimId = null;
        game.OnPieceCaptured += victimId => actualVictimId = victimId;

        game.RollDice(1);
        MoveResult result = game.MovePieceById(1, movingPieceId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.Captured, Is.True);
            Assert.That(actualVictimId, Is.EqualTo(victimPieceId));
            Assert.That(game.GetPieceState(game.GetPiece(victimPieceId)), Is.EqualTo(PieceState.Base));
        });
    }

    [Test]
    public void MovePieceById_WhenPlayerFinishesAllPieces_FiresOnPlayerFinished()
    {
        var (game, players, _) = GameFactory.CreateGameReadyToFinishPlayer();

        game.StartGame();

        int? actualPlayerId = null;
        int? actualRank = null;
        game.OnPlayerFinished += (playerId, rank) =>
        {
            actualPlayerId = playerId;
            actualRank = rank;
        };

        game.RollDice(players[0].Id);
        MoveResult result = game.MovePieceById(players[0].Id, 4);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(actualPlayerId, Is.EqualTo(players[0].Id));
            Assert.That(actualRank, Is.EqualTo(1));
            Assert.That(game.Rankings, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void MovePieceById_WhenGameEnds_FiresOnGameEndedWithWinnerId()
    {
        var (game, players, _) = GameFactory.CreateGameReadyToFinishPlayer();

        game.StartGame();

        int? actualWinnerId = null;
        game.OnGameEnded += winnerId => actualWinnerId = winnerId;

        game.RollDice(players[0].Id);
        MoveResult result = game.MovePieceById(players[0].Id, 4);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(actualWinnerId, Is.EqualTo(players[0].Id));
            Assert.That(game.Phase, Is.EqualTo(TurnPhase.Finished));
        });
    }
}
