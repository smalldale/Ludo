namespace Ludo.Api.Contracts;

public sealed record RankingDto(
    int Rank,
    int PlayerId,
    string Name,
    string Color,
    bool Finished
);
