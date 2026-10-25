namespace Ludo.Core.Domain.Interfaces;

public interface IDice
{
    public int Sides { get; }
    public Random Random { get; }
}