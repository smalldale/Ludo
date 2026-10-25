using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Domain.Enums;
using Spectre.Console;

//https://ludoking.com/play/


void SubscribeEvent(GameController game)
{
    game.OnDiceRolled += (playerId, roll) =>
    {
        var player = game.GetPlayer(playerId);

        AnsiConsole.Write(
            new Panel(
                new Align(
                    new Markup(
                        $"[bold yellow]🎲  {Markup.Escape(player.Name)} rolled a {roll}![/]"),
                    HorizontalAlignment.Center))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Yellow)
                .Padding(2, 0));
    };

    game.OnNoMovablePieces += (playerId, roll) =>
    {
        IPlayer player = game.players
            .First(p => p.Id == playerId);

        AnsiConsole.MarkupLine(
            $"[yellow]{Markup.Escape(player.Name)} rolled a {roll}, " +
            "but has no movable pieces for this roll.[/]");
    };

    game.OnPieceCaptured += victimPieceId =>
    {
        var piece = game.GetPiece(victimPieceId);

        string color = piece.Color switch
        {
            PlayerColor.Red => "red",
            PlayerColor.Green => "green",
            PlayerColor.Yellow => "yellow",
            PlayerColor.Blue => "blue",
            _ => "white"
        };

        AnsiConsole.Write(
            new Panel(
                new Markup(
                    $"[{color}]⚔ {piece.Color} piece #{victimPieceId}[/]\n" +
                    "[bold red]Captured and returned to base![/]"))
                .Header("[bold red]PIECE CAPTURED[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Red));
    };

    game.OnPieceMoved += (pieceId, position) =>
    {
        var piece = game.GetPiece(pieceId);

        string color = GetSpectreColor(piece.Color);

        AnsiConsole.MarkupLine(
            $"[{color}]● {piece.Color} piece #{pieceId}[/] " +
            $"[bold]moved to position {position}[/].");
    };

    game.OnTurnChanged += currentPlayerId =>
    {
        var player = game.GetPlayer(currentPlayerId);

        AnsiConsole.MarkupLine(
            $"[bold cyan]▶ {Markup.Escape(player.Name)}'s turn[/] " +
            $"[dim](Turn {game.Turn}, consecutive sixes: {game.ConsecutiveSixes})[/]");
    };

    game.OnGameEnded += playerId =>
    {
        var player = game.GetPlayer(playerId);

        AnsiConsole.Write(
            new Rule($"[bold green]🏆 {Markup.Escape(player.Name)} wins![/]")
                .RuleStyle("green"));
    };

    game.OnPlayerFinished += (playerId, rank) =>
    {
        var player = game.GetPlayer(playerId);

        AnsiConsole.Write(
            new Rule(
                $"[bold green]🏁 {Markup.Escape(player.Name)} " +
                $"finished in place #{rank}![/]")
                .RuleStyle("green"));
    };
}


List<IPiece> GetPlayerPieces(GameController game, IPlayer player)
{
    return Enumerable.Range(0, 4)
        .Select(i => (player.Id - 1) * 4 + i + 1)
        .Select(pieceId => game.GetPiece(pieceId))
        .ToList();
}

static string CenteredText(string text, int width)
{
    int leftPadding = (width - text.Length) / 2 + text.Length;
    return text.PadLeft(leftPadding).PadRight(width);
}

static string FormatBoardCell(
    string value,
    string? style = null,
    int width = 3)
{
    if (value.Length > width)
        value = value[..width];

    string padding = new(' ', width - value.Length);
    string content = Markup.Escape(value);

    if (!string.IsNullOrWhiteSpace(style))
        content = $"[{style}]{content}[/]";

    return padding + content;
}

static void DrawBoard(
    Board board,
    GameController game,
    IEnumerable<IPlayer> players)
{
    var table = new Table()
        .Border(TableBorder.None)
        .HideHeaders();

    for (int col = 0; col < Board.Size; col++)
    {
        table.AddColumn(
            new TableColumn(" ")
                .Centered()
                .NoWrap());
    }

    for (int row = 0; row < Board.Size; row++)
    {
        var cells = new string[Board.Size];

        for (int col = 0; col < Board.Size; col++)
        {
            ICell? cell = board.Cells[row, col];

            if (cell is null)
            {
                cells[col] = FormatBoardCell("");
                continue;
            }

            var pieces = cell.Pieces
                .OfType<IPiece>()
                .ToList();

            if (pieces.Count > 1)
            {
                cells[col] = FormatBoardCell(
                    $"×{pieces.Count}",
                    "bold yellow");
            }
            else if (pieces.Count == 1)
            {
                IPiece piece = pieces[0];

                cells[col] = FormatBoardCell(
                    piece.Id.ToString(),
                    GetSpectreColor(piece.Color));
            }
            else
            {
                PlayerColor? cellColor =
                    GetCellPlayerColor(board, row, col);

                string color = cellColor.HasValue
                    ? GetSpectreColor(cellColor.Value)
                    : "grey";

                cells[col] = cell.Type switch
                {
                    CellType.Start =>
                        FormatBoardCell("S", $"bold white on {color}"),

                    CellType.Safe =>
                        FormatBoardCell("✦", "bold cyan"),

                    CellType.Base =>
                        FormatBoardCell("○", color),

                    CellType.HomeColumn =>
                        FormatBoardCell("·", $"bold {color}"),

                    CellType.HomeTriangle =>
                        FormatBoardCell("◆", $"bold {color}"),

                    _ =>
                        FormatBoardCell("·", "grey")
                };
            }
        }

        table.AddRow(cells);
    }

    AnsiConsole.Write(
        new Panel(table)
            .Header("[bold]LUDO BOARD[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey));
    var legend = new Markup(
        "[bold]Legend:[/] " +
        "[red]●[/] Red  " +
        "[green]●[/] Green  " +
        "[yellow]●[/] Yellow  " +
        "[blue]●[/] Blue  " +
        "[bold]S[/] Start  " +
        "[cyan]✦[/] Safe  " +
        "[dim]○[/] Base  " +
        "[bold]◆[/] Finish  " +
        "[bold yellow]×N[/] Stacked pieces");

    AnsiConsole.Write(
        new Panel(legend)
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Grey));
}


void SelectAndMovePiece(GameController game, IPlayer player)
{
    var movablePositions = game.GetMovablePositions();

    var movablePieces = GetPlayerPieces(game, player)
        .Where(p => movablePositions.Contains(game.GetPosition(p)))
        .OrderBy(p => p.Id)
        .ToList();

    if (movablePieces.Count == 0)
        return;

    // Automatically move only when exactly one piece is movable,
    // it is not in Base, and its square contains only one piece.
    if (movablePieces.Count == 1)
    {
        IPiece piece = movablePieces[0];
        Coordinate coordinate = game.FindCell(piece.Id);

        bool isInBase = game.GetState(piece) == PieceState.Base;
        bool isStacked = game.board.Cells[coordinate.Row, coordinate.Col]
            .Pieces.Count > 1;

        if (!isInBase && !isStacked)
        {
            MoveResult automaticResult =
                game.MovePieceById(player.Id, piece.Id);

            if (!automaticResult.Success)
                throw new InvalidOperationException(automaticResult.Error);

            return;
        }
    }

    Console.WriteLine("\nChoose a piece to move:");

    var selectedPiece = AnsiConsole.Prompt(
    new SelectionPrompt<IPiece>()
        .Title("[bold]Choose a piece to move[/]")
        .PageSize(10)
        .UseConverter(piece =>
        {
            int position = game.GetPosition(piece);
            PieceState state = game.GetState(piece);

            string color = piece.Color switch
            {
                PlayerColor.Red => "red",
                PlayerColor.Green => "green",
                PlayerColor.Yellow => "yellow",
                PlayerColor.Blue => "blue",
                _ => "white"
            };

            string location = state == PieceState.Base
                ? "In base"
                : state == PieceState.Finished
                    ? "Finished"
                    : $"Position {position}";

            return
                $"[{color}]Piece #{piece.Id}[/] " +
                $"[dim]| {location}[/]";
        })
        .AddChoices(movablePieces));

    MoveResult result =
        game.MovePieceById(player.Id, selectedPiece.Id);

    if (!result.Success)
        throw new InvalidOperationException(result.Error);
}


List<IPlayer> GetInitializePlayers()
{
    int id = 1;
    List<IPlayer> players = [];

    Dictionary<string, PlayerColor> playerColorChoices = new()
    {
        { "R", PlayerColor.Red },
        { "G", PlayerColor.Green },
        { "B", PlayerColor.Blue },
        { "Y", PlayerColor.Yellow }
    };

    while (players.Count < 4)
    {
        Console.Write($"Enter player {players.Count + 1} name");

        if (players.Count >= 2)
            Console.Write(" (press Enter to finish)");

        Console.Write(": ");

        string? name = Console.ReadLine();

        // Allow finishing once there are at least 2 players
        if (string.IsNullOrWhiteSpace(name))
        {
            if (players.Count >= 2)
                break;

            Console.WriteLine("You need at least 2 players.");
            continue;
        }

        var availableColors = playerColorChoices
            .Where(c => !players.Any(p => p.Color == c.Value))
            .Select(c => c.Value)
            .ToList();

        PlayerColor color = AnsiConsole.Prompt(
            new SelectionPrompt<PlayerColor>()
                .Title($"Choose a color for [bold]{Markup.Escape(name)}[/]")
                .PageSize(4)
                .UseConverter(color => color switch
                {
                    PlayerColor.Red => "[red]● Red[/]",
                    PlayerColor.Green => "[green]● Green[/]",
                    PlayerColor.Blue => "[blue]● Blue[/]",
                    PlayerColor.Yellow => "[yellow]● Yellow[/]",
                    _ => Markup.Escape(color.ToString())
                })
                .AddChoices(availableColors));

        players.Add(new Player(id++, name, color));

        string colorName = color switch
        {
            PlayerColor.Red => "[red]Red[/]",
            PlayerColor.Green => "[green]Green[/]",
            PlayerColor.Blue => "[blue]Blue[/]",
            PlayerColor.Yellow => "[yellow]Yellow[/]",
            _ => Markup.Escape(color.ToString())
        };

        AnsiConsole.MarkupLine(
            $"[bold]{Markup.Escape(name)}[/] chose {colorName}.");
    }

    return players;
}

// UI Draw Method
static PlayerColor? GetCellPlayerColor(Board board, int row, int col)
{
    var coordinate = new Coordinate(row, col);

    foreach (var (color, cells) in board.BaseCells)
    {
        if (cells.Contains(coordinate))
            return color;
    }

    foreach (var (color, cells) in board.HomeStretch)
    {
        if (cells.Contains(coordinate))
            return color;
    }

    foreach (var (color, startIndex) in board.StartIndex)
    {
        if (board.Track[startIndex].Row == row &&
            board.Track[startIndex].Col == col)
        {
            return color;
        }
    }

    return null;
}

static void DrawTurnPrompt(IPlayer player)
{
    string color = GetSpectreColor(player.Color);

    AnsiConsole.Write(
        new Rule(
            $"[bold {color}]{Markup.Escape(player.Name)}'s turn[/]")
            .RuleStyle("grey"));

    AnsiConsole.Markup(
        $"[{color}]Player:[/] [bold]{Markup.Escape(player.Name)}[/]\n");

    AnsiConsole.Markup("[grey]Press Enter to roll the dice...[/]");
}

static void DrawFinalRankings(GameController game, IEnumerable<IPlayer> players)
{
    AnsiConsole.Write(
        new Rule("[bold gold1]🏆 FINAL RANKINGS 🏆[/]")
            .RuleStyle("gold1"));

    var rankingsTable = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Gold1)
        .AddColumn(new TableColumn("Rank").Centered())
        .AddColumn("Player")
        .AddColumn("Color")
        .AddColumn("Result");

    for (int i = 0; i < game.Rankings.Count; i++)
    {
        IPlayer player = game.Rankings[i];

        string rank = i == 0
            ? "[bold gold1]🏆 1st[/]"
            : $"[bold]{i + 1}[/]";

        string result = i == 0
            ? "[bold green]Winner[/]"
            : "Finished";

        string color = player.Color switch
        {
            PlayerColor.Red => "[red]Red[/]",
            PlayerColor.Green => "[green]Green[/]",
            PlayerColor.Yellow => "[yellow]Yellow[/]",
            PlayerColor.Blue => "[blue]Blue[/]",
            _ => Markup.Escape(player.Color.ToString())
        };

        rankingsTable.AddRow(
            rank,
            Markup.Escape(player.Name),
            color,
            result);
    }

    AnsiConsole.Write(rankingsTable);

    IPlayer? lastPlayer = players.FirstOrDefault(
        p => !game.Rankings.Any(r => r.Id == p.Id));

    if (lastPlayer is not null)
    {
        AnsiConsole.MarkupLine(
            $"[dim]Not ranked: {Markup.Escape(lastPlayer.Name)} " +
            $"({Markup.Escape(lastPlayer.Color.ToString())})[/]");
    }
}

static string GetSpectreColor(PlayerColor color)
{
    return color switch
    {
        PlayerColor.Red => "red",
        PlayerColor.Green => "green",
        PlayerColor.Yellow => "yellow",
        PlayerColor.Blue => "blue",
        _ => "white"
    };
}

static void DrawGameBoard(
    GameController game,
    IEnumerable<IPlayer> players)
{
    DrawBoard((Board)game.board, game, players);
}

// Option to play: StandardCLIPlay(manual) or AutoPlay

void StandardCLIPlay()
{
    List<IPlayer> players = GetInitializePlayers();
    GameController game = new GameController(players);
    SubscribeEvent(game);
    game.StartGame();

    while (game.Phase != TurnPhase.Finished)
    {
        IPlayer currentPlayer = game.GetCurrentPlayer();

        DrawGameBoard(game, players);

        DrawTurnPrompt(currentPlayer);
        Console.ReadLine();

        game.RollDice(currentPlayer.Id);

        if (game.Phase == TurnPhase.WaitingForMove)
        {
            var movablePieceIds = game.GetMovablePieceIds();

            if (movablePieceIds.Count > 0)
            {
                SelectAndMovePiece(game, currentPlayer);
            }
        }
    }
    
    DrawFinalRankings(game, players);
}


void AutoPlay(int playerCount = 4)
{
    if (playerCount < 2 || playerCount > 4)
        throw new ArgumentOutOfRangeException(
            nameof(playerCount), "Player count must be between 2 and 4.");

    PlayerColor[] colors =
    [
        PlayerColor.Red,
        PlayerColor.Green,
        PlayerColor.Blue,
        PlayerColor.Yellow
    ];

    List<IPlayer> players = Enumerable.Range(1, playerCount)
        .Select(i => (IPlayer)new Player(
            i,
            $"AI-{colors[i - 1]}",
            colors[i - 1]))
        .ToList();

    GameController game = new GameController(players);

    SubscribeEvent(game);
    // game.OnPlayerFinished += (playerId, rank) =>
    // {
    //     var player = game.GetPlayer(playerId);

    //     AnsiConsole.MarkupLine(
    //         $"[bold green]✓ {Markup.Escape(player.Name)} finished in place #{rank}![/]");
    // };

    // game.OnGameEnded += playerId =>
    // {
    //     var player = game.GetPlayer(playerId);

    //     AnsiConsole.Write(
    //         new Rule($"[bold green]🏆 {Markup.Escape(player.Name)} wins![/]")
    //             .RuleStyle("green"));
    // };

    game.StartGame();

    Random random = new Random();
    int maxActions = 100_000;

    while (game.Phase != TurnPhase.Finished && maxActions-- > 0)
    {
        IPlayer currentPlayer = game.GetCurrentPlayer();

        if (game.Phase == TurnPhase.WaitingForRoll)
        {
            game.RollDice(currentPlayer.Id);
            continue;
        }

        if (game.Phase == TurnPhase.WaitingForMove)
        {
            var movablePositions = game.GetMovablePositions();

            var movablePieces = GetPlayerPieces(game, currentPlayer)
                .Where(p => movablePositions.Contains(game.GetPosition(p)))
                .ToList();

            if (movablePieces.Count == 0)
                continue;

            IPiece selectedPiece =
                movablePieces[random.Next(movablePieces.Count)];

            MoveResult result = game.MovePieceById(
                currentPlayer.Id,
                selectedPiece.Id);

            if (!result.Success)
                throw new InvalidOperationException(result.Error);
        }
    }

    if (maxActions <= 0)
        Console.WriteLine("Stopped: action limit reached.");

    DrawFinalRankings(game, players);
}


StandardCLIPlay();
// AutoPlay(2);
