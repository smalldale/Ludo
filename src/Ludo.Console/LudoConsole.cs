using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Domain.Enums;
using Serilog;
using Spectre.Console;


public class LudoConsole
{
    private GameController _game;
    private IBoard _board;
    private readonly ILogger _logger;

    public LudoConsole(GameController gameController, ILogger? logger = null)
    {

        _game = gameController;
        _board = _game.board;
        _logger = logger ?? Log.ForContext<LudoConsole>();
        SubscribeEvent();
    }

    public void Play()
    {
        // main normal game
        _logger.Information("Starting console game");
        _game.StartGame();

        while (_game.Phase != TurnPhase.Finished)
        {
            IPlayer currentPlayer = _game.GetCurrentPlayer();

            DrawBoard();

            DrawTurnPrompt(currentPlayer); // player x turn, player x roll dice

            Console.ReadLine();
            try
            {
                _game.RollDice(currentPlayer.Id);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning(
                    ex,
                    "Cannot roll dice for player {PlayerId}",
                    currentPlayer.Id);

                AnsiConsole.MarkupLine(
                    $"[yellow]Cannot roll dice: " +
                    $"{Markup.Escape(ex.Message)}[/]");

                continue;
            }

            if (_game.Phase == TurnPhase.WaitingForMove)
            {
                var movablePieceIds = _game.GetMovablePieceIds();

                if (movablePieceIds.Count > 0)
                {
                    SelectAndMovePiece(currentPlayer);
                }
            }
        }

        _logger.Information("Console game finished");
        DrawFinalRankings();
    }
    
    void SubscribeEvent()
    {
        _game.OnDiceRolled += (playerId, roll) =>
        {
            var player = _game.GetPlayer(playerId);

            _logger.Information(
                "Player {PlayerId} ({PlayerName}) rolled {Roll}",
                playerId, player.Name, roll);

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

        _game.OnNoMovablePieces += (playerId, roll) =>
        {
            IPlayer player = _game.players
                .First(p => p.Id == playerId);

            _logger.Debug(
                "Player {PlayerId} ({PlayerName}) has no movable pieces for roll {Roll}",
                playerId, player.Name, roll);

            AnsiConsole.MarkupLine(
                $"[yellow]{Markup.Escape(player.Name)} rolled a {roll}, " +
                "but has no movable pieces for this roll.[/]");
        };

        _game.OnPieceCaptured += victimPieceId =>
        {
            var piece = _game.GetPiece(victimPieceId);

            _logger.Information(
                "Piece {PieceId} ({Color}) was captured and returned to base",
                victimPieceId, piece.Color);

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

        _game.OnPieceMoved += (pieceId, position) =>
        {
            var piece = _game.GetPiece(pieceId);

            _logger.Debug(
                "Piece {PieceId} ({Color}) moved to position {Position}",
                pieceId, piece.Color, position);

            string color = GetSpectreColor(piece.Color);

            AnsiConsole.MarkupLine(
                $"[{color}]● {piece.Color} piece #{pieceId}[/] " +
                $"[bold]moved to position {position}[/].");
        };

        _game.OnTurnChanged += currentPlayerId =>
        {
            var player = _game.GetPlayer(currentPlayerId);

            _logger.Debug(
                "Turn {Turn} begins for player {PlayerId} ({PlayerName})",
                _game.Turn, currentPlayerId, player.Name);

            AnsiConsole.MarkupLine(
                $"[bold cyan]▶ {Markup.Escape(player.Name)}'s turn[/] " +
                $"[dim](Turn {_game.Turn}, consecutive sixes: {_game.ConsecutiveSixes})[/]");
        };

        _game.OnGameEnded += playerId =>
        {
            var player = _game.GetPlayer(playerId);

            _logger.Information(
                "Game ended. Winner is player {PlayerId} ({PlayerName})",
                playerId, player.Name);

            AnsiConsole.Write(
                new Rule($"[bold green]🏆 {Markup.Escape(player.Name)} wins![/]")
                    .RuleStyle("green"));
        };

        _game.OnPlayerFinished += (playerId, rank) =>
        {
            var player = _game.GetPlayer(playerId);

            _logger.Information(
                "Player {PlayerId} ({PlayerName}) finished in rank {Rank}",
                playerId, player.Name, rank);

            AnsiConsole.Write(
                new Rule(
                    $"[bold green]🏁 {Markup.Escape(player.Name)} " +
                    $"finished in place #{rank}![/]")
                    .RuleStyle("green"));
        };
    }

    /// <summary>
    /// Spectre coloring based on PlayerColor
    /// </summary>
    /// <param name="color">PlayerColor</param>
    /// <returns></returns>
    private string GetSpectreColor(PlayerColor color)
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

    /// <summary>
    /// Format board cell padding
    /// </summary>
    /// <param name="value">text value</param>
    /// <param name="style">color styling</param>
    /// <param name="width">padding width</param>
    /// <returns></returns>
    private string FormatBoardCell(
    string value,
    string? style = null,
    int width = 3)
    {
        if (value.Length > width)
        {
            value = value[..width]; // slice only first to width index
        }

        string padding = new(' ', width - value.Length);
        string content = Markup.Escape(value);

        if (!string.IsNullOrWhiteSpace(style))
        {
            content = $"[{style}]{content}[/]";
        }

        return padding + content;
    }

    /// <summary>
    /// Get cell color based on PlayerColor key in BaseCells,
    /// HomeStretch, and StartIndex Board property dictionaries.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns>PlayerColor</returns>
    private PlayerColor? GetCellPlayerColor(int row, int col)
    {
        var coordinate = new Coordinate(row, col);

        foreach (var (color, cells) in _board.BaseCells)
        {
            if (cells.Contains(coordinate))
            {
                return color;
            }
        }

        foreach (var (color, cells) in _board.HomeStretch)
        {
            if (cells.Contains(coordinate))
            {
                return color;
            }
        }

        foreach (var (color, startIndex) in _board.StartIndex)
        {
            if (_board.Track[startIndex].Row == row &&
                _board.Track[startIndex].Col == col)
            {
                return color;
            }
        }

        return null;
    }

    private void DrawBoard()
    {
        var table = new Table()
            .Border(TableBorder.None)
            .HideHeaders();

        for (int col = 0; col < _board.Size; col++)
        {
            table.AddColumn(
                new TableColumn(" ")
                    .Centered()
                    .NoWrap());
        }

        for (int row = 0; row < Board.Size; row++)
        {
            string[] cells = new string[Board.Size];

            for (int col = 0; col < Board.Size; col++)
            {
                ICell? cell = _board.Cells[row, col];

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
                        GetCellPlayerColor(row, col);

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

    private void DrawTurnPrompt(IPlayer player)
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

    void SelectAndMovePiece(IPlayer player)
    {
        IReadOnlyList<int> movablePieceIds = _game.GetMovablePieceIds();
        
        // Automatically move only when exactly one piece is movable,
        if (movablePieceIds.Count == 1)
        {
            IPiece piece = _game.GetPiece(movablePieceIds[0]);
            MoveResult automaticResult = _game.MovePieceById(player.Id, piece.Id);

            if (!automaticResult.Success)
            {
                _logger.Warning(
                    "Automatic move failed for player {PlayerId} piece {PieceId}: {Error}",
                    player.Id, piece.Id, automaticResult.Error);

                throw new InvalidOperationException(automaticResult.Error);
            }

            return;
        }

        Console.WriteLine("\nChoose a piece to move:");

        var selectedPiece = AnsiConsole.Prompt(
        new SelectionPrompt<int>()
            .Title("[bold]Choose a piece to move[/]")
            .PageSize(10)
            .UseConverter(pieceId =>
            {
                IPiece piece = _game.GetPiece(pieceId);
                int position = _game.GetPosition(piece);
                PieceState state = _game.GetPieceState(piece);

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
            .AddChoices(movablePieceIds));

        MoveResult result =
            _game.MovePieceById(player.Id, selectedPiece);

        if (!result.Success)
        {
            _logger.Warning(
                "Move failed for player {PlayerId} piece {PieceId}: {Error}",
                player.Id, selectedPiece, result.Error);

            throw new InvalidOperationException(result.Error);
        }
    }

    private void DrawFinalRankings()
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

        for (int i = 0; i < _game.Rankings.Count; i++)
        {
            IPlayer player = _game.Rankings[i];

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

        IPlayer? lastPlayer = _game.players.FirstOrDefault(
            p => !_game.Rankings.Any(r => r.Id == p.Id));

        if (lastPlayer is not null)
        {
            AnsiConsole.MarkupLine(
                $"[dim]Not ranked: {Markup.Escape(lastPlayer.Name)} " +
                $"({Markup.Escape(lastPlayer.Color.ToString())})[/]");
        }
    }
}