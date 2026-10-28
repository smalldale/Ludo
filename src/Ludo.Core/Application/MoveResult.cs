namespace Ludo.Core.Application;

public class MoveResult
{
    public bool Success { get; } // piece is move succesfully
    public int PieceId { get; } // the piece the controller picked, -1 on failure
    public bool Captured { get; } // is pieces captured victim pices
    public bool PieceFinished { get; } // is pieces finished in home triangle
    public bool ExtraTurn { get; } // is current phase gained extra turn
    public string? Error { get; } // error message if piece fail to move

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