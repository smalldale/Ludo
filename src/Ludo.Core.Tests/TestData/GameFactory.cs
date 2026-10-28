
using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Tests.TestDoubles;

namespace Ludo.Core.Tests.TestData;

public static class GameFactory
{
    public static GameController CreateGame(
        int[] rolls,
        int playerCount = 2)
    {
        List<IPlayer> players = PlayerFactory.CreateMany(playerCount);

        IBoard board = new Board();
        IDice dice = new SequenceDice(rolls);

        var piecesByColor = PiecesFactory.CreateForPlayers(players);

        return new GameController(
            players,
            board,
            dice,
            piecesByColor);
    }

    public static (GameController Game, List<IPlayer> Players, IBoard Board)
        CreateGameWithContext(
            int[] rolls,
            int playerCount = 2)
    {
        var players = PlayerFactory.CreateMany(playerCount);
        IBoard board = new Board();
        IDice dice = new SequenceDice(rolls);

        var piecesByColor = PiecesFactory.CreateForPlayers(players);

        var game = new GameController(
            players,
            board,
            dice,
            piecesByColor);

        return (game, players, board);
    }

    public static (GameController Game, List<IPlayer> Players, IBoard Board)
        CreateGameReadyToFinishPlayer(
            PlayerColor color = PlayerColor.Red,
            int[]? rolls = null)
    {
        var players = PlayerFactory.CreateMany(2);
        IBoard board = new Board();

        Coordinate finishCell = board.HomeStretch[color][^1];
        Coordinate penultimateHomeCell = board.HomeStretch[color][^2];

        int firstPieceId = players
            .Single(player => player.Color == color)
            .Id;

        // PiecesFactory assigns four sequential IDs per player.
        firstPieceId = players.FindIndex(player => player.Color == color) * 4 + 1;

        var positions = new Dictionary<int, Coordinate>
        {
            [firstPieceId] = finishCell,
            [firstPieceId + 1] = finishCell,
            [firstPieceId + 2] = finishCell,
            [firstPieceId + 3] = penultimateHomeCell
        };

        var game = CreateGameWithPiecePositions(
            rolls ?? [1],
            positions,
            board,
            players.Count);

        return (game, players, board);
    }

    public static GameController CreateGameWithPiecePositions(
        int[] rolls,
        IReadOnlyDictionary<int, Coordinate> positions,
        int playerCount = 2)
    {
        return CreateGameWithPiecePositions(
            rolls,
            positions,
            new Board(),
            playerCount);
    }

    public static GameController CreateGameWithPiecePositions(
        int[] rolls,
        IReadOnlyDictionary<int, Coordinate> positions,
        IBoard board,
        int playerCount = 2)
    {
        var players = PlayerFactory.CreateMany(playerCount);
        var piecesByColor = PiecesFactory.CreateForPlayers(players);

        var game = new GameController(
            players,
            board,
            new SequenceDice(rolls),
            piecesByColor);

        foreach (var (pieceId, targetCoordinate) in positions)
        {
            var piece = piecesByColor
                .SelectMany(pair => pair.Value)
                .Single(p => p.Id == pieceId);

            foreach (ICell? cell in board.Cells)
            {
                cell?.Pieces.Remove(piece);
            }

            board.Cells[targetCoordinate.Row, targetCoordinate.Col]
                .Pieces.Add(piece);
        }

        return game;
    }
}
