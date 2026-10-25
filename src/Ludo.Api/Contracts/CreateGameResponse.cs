
namespace Ludo.Api.Contracts;

public sealed record CreateGameResponse(
    GameStateDto State,
    IReadOnlyList<PlayerAssignmentDto> Assignments
);

public sealed record PlayerAssignmentDto(
    int PlayerId,
    string PlayerToken
);
