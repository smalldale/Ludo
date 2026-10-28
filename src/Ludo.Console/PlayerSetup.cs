using Ludo.Core.Domain;
using Ludo.Core.Domain.Interfaces;
using Ludo.Core.Domain.Enums;
using Spectre.Console;

public static class PlayerSetup
{
    public static List<IPlayer> CreatePlayers()
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
}