using Ludo.Core.Domain.Enums;

namespace Ludo.Api.Contracts;

public sealed record CreateGameRequest(
    IReadOnlyList<CreatePlayerRequest> Players
);

public sealed record CreatePlayerRequest(
    string Name,
    PlayerColor Color
);