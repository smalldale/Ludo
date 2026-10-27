using Ludo.Core.Application;
using Ludo.Core.Domain;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Domain.Enums;
using Spectre.Console;

List<IPlayer> players = PlayerSetup.CreatePlayers();
IBoard board = new Board();
IDice dice = new Dice(6);
Dictionary<PlayerColor, List<IPiece>> piecesByColor = PieceSetup.CreatePieces(players);
try
{
    GameController game = new(players, board, dice, piecesByColor);
    LudoConsole ludoConsole = new(game);
    ludoConsole.Play();
}
catch (ArgumentOutOfRangeException ex)
{
    AnsiConsole.MarkupLine(
        $"[red]Argument out of range: " +
        $"{Markup.Escape(ex.Message)}[/]");
}
catch (InvalidOperationException ex)
{
    AnsiConsole.MarkupLine(
        $"[red]Game operation failed: " +
        $"{Markup.Escape(ex.Message)}[/]");
}
catch (ArgumentException ex)
{
    AnsiConsole.MarkupLine(
        $"[red]Invalid game argument: " +
        $"{Markup.Escape(ex.Message)}[/]");
}
