using Microsoft.AspNetCore.SignalR;

namespace Ludo.Api.Hubs;

public class GameHub : Hub
{
    /// <summary>
    /// subscribes a player's connection to a particular game room
    /// </summary>
    /// <param name="gameId"></param>
    /// <returns></returns>
    /// <exception cref="HubException"></exception>
    public async Task JoinGame(string gameId)
    {
        if (string.IsNullOrWhiteSpace(gameId))
        {
            throw new HubException("Game ID is required.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            gameId);
    }

    /// <summary>
    /// removes that connection from the room
    /// </summary>
    /// <param name="gameId"></param>
    /// <returns></returns>
    public async Task LeaveGame(string gameId)
    {
        if (string.IsNullOrWhiteSpace(gameId))
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            gameId);
    }
}
