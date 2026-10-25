namespace Ludo.Api.Contracts;

/// <summary>
/// Request to roll the dice for a player.
/// </summary>
/// <param name="PlayerId">Identifier of the player taking the action.</param>
public sealed record PlayerActionRequest(int PlayerId);

public record MovePieceRequest(int PlayerId, int PieceId);