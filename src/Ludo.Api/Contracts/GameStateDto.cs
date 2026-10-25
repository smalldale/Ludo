namespace Ludo.Api.Contracts;

public sealed record GameStateDto(
    string GameId,
    int Turn,
    string Phase,
    int CurrentPlayerId,
    IReadOnlyList<PlayerDto> Players,
    IReadOnlyList<MovablePieceDto> MovablePieces,
    IReadOnlyList<PieceDto> Pieces,
    IReadOnlyList<RankingDto> Rankings,
    BoardDto Board
);
