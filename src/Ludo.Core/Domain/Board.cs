using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;

namespace Ludo.Core.Domain;

public class Board: IBoard
{
    public const int Size = 15;
    int IBoard.Size => Size;
    public ICell[,] Cells { get; } = new Cell[Size, Size];
    public List<Coordinate> Track { get; } = new();
    public Dictionary<PlayerColor, int> StartIndex { get; } = new();
    public Dictionary<PlayerColor, List<Coordinate>> HomeStretch { get; } = new();
    public Dictionary<PlayerColor, List<Coordinate>> BaseCells { get; } = new();


    public Board()
    {
        Coordinate[] outerRing =
        {
            // idx 0..12
            new(6,0), new(6,1), new(6,2), new(6,3), new(6,4), new(6,5), new(5,6), new(4,6), new(3,6), new(2,6), new(1,6), new(0,6), new(0,7),
            // idx 13..25
            new(0,8), new(1,8), new(2,8), new(3,8), new(4,8), new(5,8), new(6,9), new(6,10), new(6,11), new(6,12), new(6,13), new(6,14), new(7,14),
            // idx 26..38
            new(8,14), new(8,13), new(8,12), new(8,11), new(8,10), new(8,9), new(9,8), new(10,8), new(11,8), new(12,8), new(13,8), new(14,8), new(14,7),
            // idx 39..51
            new(14,6), new(13,6), new(12,6), new(11,6), new(10,6), new(9,6), new(8,5), new(8,4), new(8,3), new(8,2), new(8,1), new(8,0), new(7,0)
        }; // 52 cell coordinate for outerring.
        Track.AddRange(outerRing);

        StartIndex[PlayerColor.Red] = 1; // (6,1)
        StartIndex[PlayerColor.Green] = 14; // (1,8)
        StartIndex[PlayerColor.Yellow] = 27; // (8,13)
        StartIndex[PlayerColor.Blue] = 40; // (13,6)

        // Need 51 steps before go to HomeScretch (including start index)
        // Last index of HomeStretch is Finished Position.
        HomeStretch[PlayerColor.Red] = Line((7, 1), (7, 2), (7, 3), (7, 4), (7, 5), (7, 6));
        HomeStretch[PlayerColor.Green] = Line((1, 7), (2, 7), (3, 7), (4, 7), (5, 7), (6, 7));
        HomeStretch[PlayerColor.Yellow] = Line((7, 13), (7, 12), (7, 11), (7, 10), (7, 9), (7, 8));
        HomeStretch[PlayerColor.Blue] = Line((13, 7), (12, 7), (11, 7), (10, 7), (9, 7), (8, 7));

        // Base Cell coordinate, this is all Piece() are positioned before the dice are rolled to move it. 
        BaseCells[PlayerColor.Red] = Line((2, 2), (2, 3), (3, 2), (3, 3));
        BaseCells[PlayerColor.Green] = Line((2, 11), (2, 12), (3, 11), (3, 12));
        BaseCells[PlayerColor.Yellow] = Line((11, 11), (11, 12), (12, 11), (12, 12));
        BaseCells[PlayerColor.Blue] = Line((11, 2), (11, 3), (12, 2), (12, 3));

        BuildCells();
    }

    private static List<Coordinate> Line(params (int Row, int Col)[] cells) =>
        cells.Select(c => new Coordinate(c.Row, c.Col)).ToList();

    private void BuildCells()
    {
        // track: Start at each start index, Safe star at start + 8, Normal elsewhere
        var starts = new HashSet<int>(StartIndex.Values); // CellType.Start
        var stars = new HashSet<int>(
            StartIndex.Values.Select(s => (s + 8) % Track.Count)); // CellType.Safe
        for (int i = 0; i < Track.Count; i++)
        {
            CellType type = starts.Contains(i)
                ? CellType.Start
                : stars.Contains(i) 
                    ? CellType.Safe
                    : CellType.Normal;
            Cells[Track[i].Row, Track[i].Col] = new Cell(type);
        }

        foreach (List<Coordinate> stretch in HomeStretch.Values)
            for (int i = 0; i < stretch.Count; i++)
                Cells[stretch[i].Row, stretch[i].Col] = new Cell(i == stretch.Count - 1 ? CellType.HomeTriangle : CellType.HomeColumn);

        foreach (List<Coordinate> cells in BaseCells.Values)
            foreach (Coordinate c in cells)
                Cells[c.Row, c.Col] = new Cell(CellType.Base);
    }
}