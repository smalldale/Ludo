namespace Ludo.Api.Contracts;

/// <summary>
/// 
/// </summary>
/// <param name="BoardSize">Grid size 15 × 15</param>
/// <param name="Cells"></param>
/// <param name="StartIndexes"></param>
/// <param name="HomeStretch"></param>
/// <param name="BaseCells"></param>
public sealed record BoardDto(
    int BoardSize,
    IReadOnlyList<BoardCellDto> Cells,
    IReadOnlyDictionary<string, int> StartIndexes,
    IReadOnlyDictionary<string, IReadOnlyList<BoardCoordinateDto>> HomeStretch,
    IReadOnlyDictionary<string, IReadOnlyList<BoardCoordinateDto>> BaseCells
);


/// <summary>
/// 
/// </summary>
/// <param name="Row">Row cell position</param>
/// <param name="Column">Column cell position</param>
/// <param name="Type">Identifies Normal, Start, Safe, Base, HomeColumn, or HomeTriangle.</param>
/// <param name="TrackPosition">Gives the logical track index from Board.Track, when applicable.</param>
/// <param name="Color">Identifies a color-specific home or base cell when applicable.</param>
public sealed record BoardCellDto(
    int Row,
    int Column,
    string Type,
    int? TrackPosition,
    string? Color
);

public sealed record BoardCoordinateDto(
    int Row,
    int Column
);