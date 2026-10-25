
using Ludo.Core.Application;
using System.Security.Cryptography;

namespace Ludo.Api.Services;

public sealed class GameSession
{
    private readonly HashSet<int> _playerIds;
    private readonly Dictionary<int, string> _playerTokens;

    public string Id { get; }
    public GameController Controller { get; }

    // Protects mutations of this game's controller.
    public SemaphoreSlim StateLock { get; } = new(1, 1);

    public IReadOnlySet<int> PlayerIds => _playerIds;

    public GameSession(string id, GameController controller)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(controller);

        Id = id;
        Controller = controller;

        _playerIds = controller.players
            .Select(player => player.Id)
            .ToHashSet();

        _playerTokens = _playerIds.ToDictionary(
            playerId => playerId,
            _ => Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32)));
    }

    public bool HasPlayer(int playerId)
    {
        return _playerIds.Contains(playerId);
    }

    public string GetPlayerToken(int playerId)
    {
        if (!_playerTokens.TryGetValue(playerId, out var token))
        {
            throw new UnauthorizedAccessException(
                $"Player '{playerId}' does not belong to game '{Id}'.");
        }

        return token;
    }

    public bool IsPlayerTokenValid(int playerId, string token)
    {
        return _playerTokens.TryGetValue(playerId, out var expectedToken)
            && !string.IsNullOrWhiteSpace(token)
            && CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(expectedToken),
                System.Text.Encoding.UTF8.GetBytes(token));
    }
}
