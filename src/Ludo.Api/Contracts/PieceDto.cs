namespace Ludo.Api.Contracts;

public sealed record PieceDto(
    int Id,
    string Color,
    int Position,
    string State
);