namespace Ludo.Core.Application;

public class MoveResult
{
    public bool Success { get; }
    public int PieceId { get; } // the piece the controller picked, -1 on failure
    public bool Captured { get; }
    public bool PieceFinished { get; }
    public bool ExtraTurn { get; }
    public string? Error { get; }

    public MoveResult(bool success, int pieceId, bool captured, bool pieceFinished, bool extraTurn, string? error)
    {
        Success = success;
        PieceId = pieceId;
        Captured = captured;
        PieceFinished = pieceFinished;
        ExtraTurn = extraTurn;
        Error = error;
    }

    public static MoveResult Fail(string error) => new(false, -1, false, false, false, error);

    public override string ToString() =>
        Success
            ? $"OK (piece {PieceId}, captured={Captured}, finished={PieceFinished}, extraTurn={ExtraTurn})"
            : $"REJECTED: {Error}";

}