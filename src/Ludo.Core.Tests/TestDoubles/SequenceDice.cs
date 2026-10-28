
using Ludo.Core.Domain.Interfaces;

namespace Ludo.Core.Tests.TestDoubles;

public sealed class SequenceDice : IDice
{
    public int Sides { get; }
    public Random Random { get; }

    public SequenceDice(params int[] rolls)
    {
        Sides = 6;
        Random = new SequenceRandom(rolls);
    }

    private sealed class SequenceRandom : Random
    {
        private readonly Queue<int> _rolls;

        public SequenceRandom(IEnumerable<int> rolls)
        {
            _rolls = new Queue<int>(rolls);
        }

        public override int Next(int minValue, int maxValue)
        {
            if (_rolls.Count == 0)
            {
                throw new InvalidOperationException(
                    "No scripted dice rolls remain.");
            }

            int roll = _rolls.Dequeue();

            if (roll < minValue || roll >= maxValue)
            {
                throw new InvalidOperationException(
                    $"Scripted roll {roll} is outside " +
                    $"the range [{minValue}, {maxValue}).");
            }

            return roll;
        }
    }
}
