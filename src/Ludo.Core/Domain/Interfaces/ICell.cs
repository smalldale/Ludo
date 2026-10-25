using Ludo.Core.Domain.Enums;

namespace Ludo.Core.Domain.Interfaces;

public interface ICell
{
    public CellType Type { get; }
    public List<IPiece> Pieces { get; }
}