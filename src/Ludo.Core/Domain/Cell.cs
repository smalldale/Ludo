using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;

namespace Ludo.Core.Domain;

public class Cell: ICell
{
    public CellType Type { get; }
    public List<IPiece> Pieces { get; } = new();

    public Cell(CellType type)
    {
        Type = type;
    }
}