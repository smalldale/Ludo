using System.Collections.Concurrent;

namespace Ludo.Api.Services;

public sealed class GameRoomManager
{
    private readonly ConcurrentDictionary<string, GameSession> _games = new();

    public GameSession Create(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!_games.TryAdd(session.Id, session))
        {
            throw new InvalidOperationException(
                $"Game '{session.Id}' already exists.");
        }

        return session;
    }

    public bool TryGet(string gameId, out GameSession? session)
    {
        return _games.TryGetValue(gameId, out session);
    }

    public GameSession GetRequired(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var session))
        {
            throw new KeyNotFoundException(
                $"Game '{gameId}' was not found.");
        }

        return session;
    }

    public bool Remove(string gameId)
    {
        return _games.TryRemove(gameId, out _);
    }
}