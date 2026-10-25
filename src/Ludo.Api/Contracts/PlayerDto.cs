namespace Ludo.Api.Contracts;

public sealed record PlayerDto(
    int Id,
    string Name,
    string Color
);