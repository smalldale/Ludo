using Ludo.Api.Contracts;
using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;

namespace Ludo.Api.Services;

public sealed class GameApplicationService
{
    private readonly GameRoomManager _roomManager;

    public GameApplicationService(GameRoomManager roomManager)
    {
        _roomManager = roomManager;
    }


    // Register a game after its players and controller have been created.
    /// <summary>
    /// Registers an already-created game session in GameRoomManager.
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>game ID</returns>
    public async Task<string> RegisterGameAsync(
        GameController controller,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(controller);

        var gameId = Guid.NewGuid().ToString("N");
        var session = new GameSession(gameId, controller);

        await session.StateLock.WaitAsync(cancellationToken);

        try
        {
            _roomManager.Create(session);
        }
        finally
        {
            session.StateLock.Release();
        }

        return gameId;
    }

    /// <summary>
    /// Validates player data, creates players, constructs the controller,
    /// starts the game, registers it,and returns its initial state.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<CreateGameResponse> CreateGameAsync(
        CreateGameRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Players is null)
            throw new ArgumentException("Players are required.");

        if (request.Players.Count < 2 || request.Players.Count > 4)
            throw new ArgumentException(
                "A game requires between 2 and 4 players.");

        if (request.Players.Any(p =>
            p is null || string.IsNullOrWhiteSpace(p.Name)))
            throw new ArgumentException(
                "Every player must have a name.");

        if (request.Players.Any(p =>
            !Enum.IsDefined(typeof(PlayerColor), p.Color)))
            throw new ArgumentException(
                "One or more player colors are invalid.");

        if (request.Players.Select(p => p.Color).Distinct().Count()
            != request.Players.Count)
            throw new ArgumentException(
                "Player colors must be unique.");

        var players = request.Players
            .Select((p, index) => (IPlayer)new Player(
                index + 1,
                p.Name.Trim(),
                p.Color))
            .ToList();

        IBoard board = new Board();
        IDice dice =  new Dice(6);
        Dictionary<PlayerColor, List<IPiece>> piecesByColor = new Dictionary<PlayerColor, List<IPiece>>();

        foreach (IPlayer player in players)
        {
            List<IPiece> pieces = [];
            for (int i = 0; i < 4; i++)
            {
                IPiece piece = new Piece((player.Id - 1) * 4 + i + 1, player.Color);
                pieces.Add(piece);
            }
            piecesByColor[player.Color] = pieces;
        }

        GameController controller = new GameController(players, board, dice, piecesByColor);
        controller.StartGame();

        string? gameId = await RegisterGameAsync(controller, cancellationToken);

        return new CreateGameResponse(
            new GameStateDto(
                gameId,
                controller.Turn,
                controller.Phase.ToString(),
                controller.GetCurrentPlayer().Id,
                controller.players
                    .Select(p => new PlayerDto(
                        p.Id,
                        p.Name,
                        p.Color.ToString()))
                    .ToList(),
                CreateMovablePieceSnapshots(controller),
                CreatePieceSnapshots(controller),
                CreateRankings(controller),
                CreateBoardSnapshot(controller)
            ),
            controller.players
                .Select(p => new PlayerAssignmentDto(
                    p.Id,
                    _roomManager.GetRequired(gameId).GetPlayerToken(p.Id)))
                .ToList()
        );
    }

    
    
    private static BoardDto CreateBoardSnapshot(GameController controller)
    {
        IBoard board = controller.board;

        var trackPositions = board.Track
            .Select((coordinate, index) => new
            {
                coordinate.Row,
                coordinate.Col,
                Index = index
            })
            .ToDictionary(
                cell => (cell.Row, cell.Col),
                cell => cell.Index);

        var cells = new List<BoardCellDto>();

        for (int row = 0; row < Board.Size; row++)
        {
            for (int col = 0; col < Board.Size; col++)
            {
                ICell cell = board.Cells[row, col];

                if (cell is null)
                    continue;

                int? trackPosition =
                    trackPositions.TryGetValue((row, col), out int index)
                        ? index
                        : null;

                string? color = null;

                // Identify base-cell ownership.
                foreach (var entry in board.BaseCells)
                {
                    if (entry.Value.Any(c => c.Row == row && c.Col == col))
                    {
                        color = entry.Key.ToString();
                        break;
                    }
                }

                // Identify home-stretch ownership.
                if (color is null)
                {
                    foreach (var entry in board.HomeStretch)
                    {
                        if (entry.Value.Any(c => c.Row == row && c.Col == col))
                        {
                            color = entry.Key.ToString();
                            break;
                        }
                    }
                }

                // Identify starting-cell ownership.
                if (color is null)
                {
                    foreach (var entry in board.StartIndex)
                    {
                        var start = board.Track[entry.Value];

                        if (start.Row == row && start.Col == col)
                        {
                            color = entry.Key.ToString();
                            break;
                        }
                    }
                }

                cells.Add(new BoardCellDto(
                    row,
                    col,
                    cell.Type.ToString(),
                    trackPosition,
                    color));
            }
        }

        var startIndexes = board.StartIndex.ToDictionary(
            entry => entry.Key.ToString(),
            entry => entry.Value);

        var homeStretch = board.HomeStretch.ToDictionary(
            entry => entry.Key.ToString(),
            entry => (IReadOnlyList<BoardCoordinateDto>)entry.Value
                .Select(c => new BoardCoordinateDto(c.Row, c.Col))
                .ToList());

        var baseCells = board.BaseCells.ToDictionary(
            entry => entry.Key.ToString(),
            entry => (IReadOnlyList<BoardCoordinateDto>)entry.Value
                .Select(c => new BoardCoordinateDto(c.Row, c.Col))
                .ToList());

        return new BoardDto(
            Board.Size,
            cells,
            startIndexes,
            homeStretch,
            baseCells);
    }

    public async Task<GameStateDto> GetStateAsync(
        string gameId,
        CancellationToken cancellationToken = default)
    {
        var session = _roomManager.GetRequired(gameId);

        await session.StateLock.WaitAsync(cancellationToken);

        try
        {
            return CreateSnapshot(session);
        }
        finally
        {
            session.StateLock.Release();
        }
    }

    public async Task<RollResultDto> RollDiceAsync(
        string gameId,
        int playerId,
        CancellationToken cancellationToken = default)
    {
        var session = _roomManager.GetRequired(gameId);

        await session.StateLock.WaitAsync(cancellationToken);

        try
        {
            EnsurePlayerBelongsToGame(session, playerId);

            // GameController generates the roll and validates the turn.
            int roll = session.Controller.RollDice(playerId);

            return new RollResultDto(
                roll,
                CreateSnapshot(session));
        }
        finally
        {
            session.StateLock.Release();
        }
    }

    private static IReadOnlyList<PieceDto> CreatePieceSnapshots(
    GameController controller)
    {
        return controller.players
            .SelectMany(player =>
                Enumerable.Range(0, 4)
                    .Select(i => controller.GetPiece((player.Id - 1) * 4 + i + 1)))
            .Select(piece => new PieceDto(
                piece.Id,
                piece.Color.ToString(),
                controller.GetPosition(piece),
                controller.GetPieceState(piece).ToString()))
            .ToList();
    }

    public async Task<MoveResultDto> MovePieceAsync(
        string gameId,
        int playerId,
        int pieceId,
        CancellationToken cancellationToken = default)
    {
        var session = _roomManager.GetRequired(gameId);

        await session.StateLock.WaitAsync(cancellationToken);

        try
        {
            EnsurePlayerBelongsToGame(session, playerId);

            var result = session.Controller.MovePieceById(
                playerId,
                pieceId);

            return new MoveResultDto(
                result,
                CreateSnapshot(session));
        }
        finally
        {
            session.StateLock.Release();
        }
    }

    private static void EnsurePlayerBelongsToGame(
        GameSession session,
        int playerId)
    {
        if (!session.HasPlayer(playerId))
        {
            throw new UnauthorizedAccessException(
                $"Player '{playerId}' does not belong to game '{session.Id}'.");
        }
    }

    
    private static void EnsurePlayerToken(
        GameSession session,
        int playerId,
        string playerToken)
    {
        EnsurePlayerBelongsToGame(session, playerId);

        if (!session.IsPlayerTokenValid(playerId, playerToken))
        {
            throw new UnauthorizedAccessException(
                "Invalid player token.");
        }
    }


    private static GameStateDto CreateSnapshot(GameSession session)
    {
        var controller = session.Controller;

        var players = controller.players
            .Select(player => new PlayerDto(
                player.Id,
                player.Name,
                player.Color.ToString()))
            .ToList();

        var movablePositions =
            controller.Phase == TurnPhase.WaitingForMove
                ? CreateMovablePieceSnapshots(controller)
                : [];

        return new GameStateDto(
            session.Id,
            controller.Turn,
            controller.Phase.ToString(),
            controller.GetCurrentPlayer().Id,
            players,
            movablePositions,
            CreatePieceSnapshots(controller),
            CreateRankings(controller),
            CreateBoardSnapshot(controller)
        );
    }

    private static IReadOnlyList<RankingDto> CreateRankings(
        GameController controller)
    {
        var rankings = controller.Rankings
            .Select((player, index) => new RankingDto(
                index + 1,
                player.Id,
                player.Name,
                player.Color.ToString(),
                true))
            .ToList();

        if (controller.Phase != TurnPhase.Finished)
        {
            return rankings;
        }

        // The game ends when all but one player have finished. Add the
        // remaining player as the final place so the UI can show a complete
        // ranking table.
        var rankedPlayerIds = rankings
            .Select(ranking => ranking.PlayerId)
            .ToHashSet();

        foreach (var player in controller.players
                     .Where(player => !rankedPlayerIds.Contains(player.Id)))
        {
            rankings.Add(new RankingDto(
                rankings.Count + 1,
                player.Id,
                player.Name,
                player.Color.ToString(),
                false));
        }

        return rankings;
    }

    private static IReadOnlyList<MovablePieceDto> CreateMovablePieceSnapshots(
    GameController controller)
    {
        return controller.GetMovablePieceIds()
            .Select(pieceId =>
            {
                var piece = controller.GetPiece(pieceId);

                return new MovablePieceDto(
                    pieceId,
                    controller.GetPosition(piece));
            })
            .ToList();
    }
}

public sealed record RollResultDto(
    int Roll,
    GameStateDto State
);

public sealed record MoveResultDto(
    Ludo.Core.Application.MoveResult Result,
    GameStateDto State
);