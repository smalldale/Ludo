using Ludo.Core.Application;
using Ludo.Core.Domain.Interfaces;

namespace Ludo.Web.Services;

public sealed class GameSessionService : IDisposable
{
    // public GameSessionService()
    // {
    //     Console.WriteLine($"GameSessionService created: {GetHashCode()}");
    // }
    private readonly List<string> _history = new();
    private const int MaxHistoryEntries = 48;
    private List<IPlayer> _players = new();
    public string StatusMessage { get; private set; } =
        "Start a game to begin playing.";
    public GameController? Game { get; private set; }
    public int? LastRoll { get; private set; }
    public int? LastRollPlayerId { get; private set; }
    
    public event Action? StateChanged;

    public void StartGame(List<IPlayer> players)
    {
        // Console.WriteLine($"Starting game in service: {GetHashCode()}");
        _players = players.ToList();
        _history.Clear();
        StatusMessage = "Game started. Roll the dice!";
        LastRoll = null;
        LastRollPlayerId = null;
        if (Game is not null)
        {
            Game.OnDiceRolled -= HandleDiceRolled;
            Game.OnPieceMoved -= HandlePieceMoved;
            Game.OnPieceCaptured -= HandlePieceCaptured;
            Game.OnTurnChanged -= HandleTurnChanged;
            Game.OnPlayerFinished -= HandlePlayerFinished;
            Game.OnGameEnded -= HandleGameEnded;
        }

        LastRoll = null;
        LastRollPlayerId = null;

        Game = new GameController(players);

        Game.OnDiceRolled += HandleDiceRolled;
        Game.OnPieceMoved += HandlePieceMoved;
        Game.OnPieceCaptured += HandlePieceCaptured;
        Game.OnTurnChanged += HandleTurnChanged;
        Game.OnPlayerFinished += HandlePlayerFinished;
        Game.OnGameEnded += HandleGameEnded;

        Game.StartGame();
        NotifyStateChanged();
    }

    private string GetPlayerName(int playerId) =>
    _players.FirstOrDefault(p => p.Id == playerId)?.Name
    ?? $"Player {playerId + 1}";

    public IReadOnlyList<string> History => _history;

    private void AddHistory(string message)
    {
        StatusMessage = message;
        _history.Insert(0, message);

        if (_history.Count > MaxHistoryEntries)
            _history.RemoveAt(_history.Count - 1);

        NotifyStateChanged();
    }

    private static string Ordinal(int rank) => rank switch
    {
        1 => "1st",
        2 => "2nd",
        3 => "3rd",
        _ => $"{rank}th"
    };

    public int RollDice(int playerId)
    {
        if (Game is null)
        {
            throw new InvalidOperationException("No game started.");
        }

        int roll = Game.RollDice(playerId);

        LastRoll = roll;
        LastRollPlayerId = playerId;

        NotifyStateChanged();

        return roll;
    }

    public MoveResult MovePiece(int playerId, int pieceId)
    {
        if (Game is null)
        {
            return MoveResult.Fail("Game has not started.");
        }
        MoveResult result = Game.MovePieceById(playerId, pieceId);
        NotifyStateChanged();
        return result;
    }

    private void HandleDiceRolled(int playerId, int roll)
    {
        LastRoll = roll;
        LastRollPlayerId = playerId;

        var message = $"{GetPlayerName(playerId)} rolled {roll}.";

        if (roll == 6)
            message += " Roll a 6!";

        AddHistory(message);
    }

    private void HandleGameEvent(int playerId, int roll) =>
        NotifyStateChanged();
    
    private void HandlePieceMoved(int pieceId, int position) =>
        AddHistory($"Piece {pieceId} moved to position {position}.");

    private void HandlePieceCaptured(int victimPieceId) =>
        AddHistory($"Piece {victimPieceId} was captured and sent home.");

    private void HandleTurnChanged(int playerId) =>
        AddHistory($"It's {GetPlayerName(playerId)}'s turn.");

    private void HandlePlayerFinished(int playerId, int rank) =>
        AddHistory(
        $"{GetPlayerName(playerId)} finished in {Ordinal(rank)} place!");
    
    private void HandleGameEnded(int winnerPlayerId) =>
        AddHistory($"{GetPlayerName(winnerPlayerId)} wins the game!");

    private void NotifyStateChanged() =>
        StateChanged?.Invoke();

    public void Dispose()
    {
        if (Game is null)
        {
            return;
        }
        
        Game.OnDiceRolled -= HandleDiceRolled;
        Game.OnPieceMoved -= HandlePieceMoved;
        Game.OnPieceCaptured -= HandlePieceCaptured;
        Game.OnTurnChanged -= HandleTurnChanged;
        Game.OnPlayerFinished -= HandlePlayerFinished;
        Game.OnGameEnded -= HandleGameEnded;
    }
}