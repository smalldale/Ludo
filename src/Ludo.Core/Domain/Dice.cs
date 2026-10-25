using Ludo.Core.Domain.Interfaces;

namespace Ludo.Core.Domain;

// Data only, no Roll().
// Use GameController.NextRoll() to roll,
// return Random.Next(1, sides + 1) => normal dice random 1-6
//
public class Dice: IDice
{
    public int Sides { get; }
    public Random Random { get; } = new();
    // Scripted Dice?

    public Dice(int sides)
    {
        Sides = sides;
    }

    // kenapa harus ada preset Rolls? Scripted? For test?
    // public Dice(int sides, IEnumerable<int> presetRolls)
    // {
    //     Sides = sides;
    // }
}