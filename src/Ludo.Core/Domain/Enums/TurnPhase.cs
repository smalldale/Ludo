namespace Ludo.Core.Domain.Enums;

// waiting current player to...(roll dice, choose piece to move) and current player ended.
public enum TurnPhase
{
    WaitingForRoll,
    WaitingForMove,
    Finished
}