using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Domain.Enums;
using Serilog;
using Serilog.Events;
using Spectre.Console;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        restrictedToMinimumLevel: LogEventLevel.Debug)
    .WriteTo.File(
        formatter: new JsonFormatter(),
        path: "logs/ludo-.json",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Information)
    .CreateLogger();

try
{
    Log.Information("Ludo console starting");

    List<IPlayer> players = PlayerSetup.CreatePlayers();
    IBoard board = new Board();
    IDice dice = new Dice(6);
    Dictionary<PlayerColor, List<IPiece>> piecesByColor = PieceSetup.CreatePieces(players);

    Log.Information(
        "Players configured: {@Players}",
        players.Select(p => new { p.Id, p.Name, p.Color }));

    GameController game = new(players, board, dice, piecesByColor);
    LudoConsole ludoConsole = new(game, Log.ForContext<LudoConsole>());
    ludoConsole.Play();

    Log.Information(
        "Ludo console finished. Rankings: {@Rankings}",
        game.Rankings.Select(p => new { p.Id, p.Name, p.Color }));
}
catch (InvalidOperationException ex)
{
    Log.Error(ex, "Game operation failed");
    AnsiConsole.MarkupLine(
        $"[red]Game operation failed: " +
        $"{Markup.Escape(ex.Message)}[/]");
}
catch (ArgumentOutOfRangeException ex)
{
    Log.Error(ex, "Argument out of range");
    AnsiConsole.MarkupLine(
        $"[red]Argument out of range: " +
        $"{Markup.Escape(ex.Message)}[/]");
}
catch (ArgumentException ex)
{
    Log.Error(ex, "Invalid game argument");
    AnsiConsole.MarkupLine(
        $"[red]Invalid game argument: " +
        $"{Markup.Escape(ex.Message)}[/]");
}
finally
{
    Log.CloseAndFlush();
}
