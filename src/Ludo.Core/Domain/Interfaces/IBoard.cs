using Ludo.Core.Domain.Enums;

namespace Ludo.Core.Domain.Interfaces;

public interface IBoard
{
    // public int Size { get; }
    int Size { get; }
    public ICell[,] Cells { get; }
    public List<Coordinate> Track { get; }
    public Dictionary<PlayerColor, int> StartIndex { get; }
    public Dictionary<PlayerColor, List<Coordinate>> HomeStretch { get; }
    public Dictionary<PlayerColor, List<Coordinate>> BaseCells { get; }
}