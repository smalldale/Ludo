using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;

namespace Ludo.Core.Application;

public class GameController
{
    private const int FinishPosition = 56; // addons, index finished relative position
    private const int HomeColumnStart = 51; // addons, index home column/scretch start relative position.
    private const int LastTrackPosition = 50; // addons, index last outerRing track relative position.
    public List<IPlayer> players = [];
    public IBoard board;
    public IDice dice;
    public Dictionary<int, PlayerColor> playerColors = new Dictionary<int, PlayerColor>();
    private Dictionary<PlayerColor, List<IPiece>> piecesByColor;
    private int currentTurnIndex;
    private int pendingRoll; // Pending roll dice, 0 = no roll.
    public int Turn { get; private set; }  // 0 = not started, then 1, 2, 3...
    public TurnPhase Phase { get; private set; } = TurnPhase.WaitingForRoll; // Current Phase Move (WaitingForRoll, WaitingForMove, Finished)

    public int ConsecutiveSixes { get; private set; } // Count consecutive sixes of current player turn.
    public List<IPlayer> Rankings { get; } = [];

    public event Action<int, int>? OnDiceRolled; // (playerid, roll)
    public event Action<int, int>? OnNoMovablePieces; // (playerid, roll) addons
    public event Action<int, int>? OnPieceMoved; // (pieceId, position)
    public event Action<int>? OnPieceCaptured; // victimPieceId
    public event Action<int>? OnTurnChanged; // currentPlayerId
    public event Action<int, int>? OnPlayerFinished; // (playerId, rank)
    public event Action<int>? OnGameEnded; // winnerPlayerId

    // addons
    public GameController(List<IPlayer> players)
        : this(players, new Board(), new Dice(6), new Dictionary<PlayerColor, List<IPiece>>())
    {
        
    }
    
    public GameController(List<IPlayer> players, IBoard board, IDice dice,
        Dictionary<PlayerColor, List<IPiece>> piecesByColor)
    {
        if (players.Count < 2 || players.Count > 4)
        {
            throw new ArgumentException($"Need 2-4 player to play this game, you give {players.Count} players.", nameof(players));
        }
        if (players.Select(p => p.Color).Distinct().Count() != players.Count)
        {
            throw new ArgumentException("Player colors must unique.", nameof(playerColors));
        }
        if (players.Select(p => p.Id).Distinct().Count() != players.Count)
        {
            throw new ArgumentException("Player ids must be unique.", nameof(players));
        }
        this.players = new List<IPlayer>(players);
        this.board = board;
        this.dice = dice;
        this.piecesByColor = piecesByColor;

        if (this.piecesByColor.Count != this.players.Count * 4)
        {
            this.piecesByColor.Clear();
            foreach (IPlayer player in this.players)
            {
                playerColors[player.Id] = player.Color;
                List<IPiece> pieces = [];
                for (int i = 0; i < 4; i++)
                {
                    IPiece piece = new Piece((player.Id - 1) * 4 + i + 1, player.Color);
                    pieces.Add(piece);
                    GetCell(board.BaseCells[player.Color][i]).Pieces.Add(piece);
                }
                this.piecesByColor[player.Color] = pieces;
            }
        }
    }

    /// <summary>First player's turn, waiting for a roll.</summary>
    public void StartGame()
    {
        Turn = 1;
        currentTurnIndex = 0;
        pendingRoll = 0;
        ConsecutiveSixes = 0;
        Phase = TurnPhase.WaitingForRoll;
        OnTurnChanged?.Invoke(GetCurrentPlayer().Id);
    }

    /// <summary>
    /// Rolls for the current player and stores the roll internally (the caller can't pick it).
    /// A third six in a row is not allowed: the turn is passed. When no piece can move, the turn
    /// passes too, except after a 6, which still grants its bonus roll.
    /// </summary>
    /// <param name="playerId">Player.Id of player that current play turn.</param>
    /// <returns>random number of a dice roll</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public int RollDice(int playerId)
    {
        if (Turn == 0)
        {
            throw new InvalidOperationException("Game not started.");
        }
        if (Phase != TurnPhase.WaitingForRoll)
        {
            throw new InvalidOperationException($"Can't roll now (phase: {Phase}).");
        }
        if (playerId != GetCurrentPlayer().Id)
        {
            throw new InvalidOperationException("Not your turn.");
        }
        int roll = NextRoll();
        pendingRoll = roll;
        OnDiceRolled?.Invoke(playerId, pendingRoll);

        if (TrackRoll(roll))
        {
            ForfeitTurn(); // third six in a row, not allowed.
            return roll;
        }

        Phase = TurnPhase.WaitingForMove;
        if (GetMovablePieces().Count == 0)
        {
            OnNoMovablePieces?.Invoke(playerId, roll);
            EndTurn(GrantsExtraTurn(roll, false, false)); // if nothing to move, end turn, unless dice = 6
        }
        return roll;

    }

    /// <summary>
    /// Cells (the current player's relative positions) the UI can offer for the pending roll.
    /// Each cell appears once, however many pieces are stacked on it. -1 = Base (offered once even
    /// though the 4 Base pieces sit in 4 different Base cells).
    /// 
    /// Relative position piece that can be moved base on dice roll
    /// 
    /// </summary>
    /// <returns></returns>
    public List<int> GetMovablePositions() =>
        GetMovablePieces()
            .Select(GetPosition)
            .Distinct()
            .OrderBy(pos => pos)
            .ToList();
    
    /// <summary>
    /// Get movable piece by PieceId
    /// </summary>
    /// <returns>List of movable PieceId</returns>
    public IReadOnlyList<int> GetMovablePieceIds() => // adddons
        GetMovablePieces()
            .Select(piece => piece.Id)
            .ToList();

    private MoveResult MovePieceCore(int playerId, IPiece? piece) // addons
    {
        if (Phase != TurnPhase.WaitingForMove)
        {
            return MoveResult.Fail("Roll the dice first");
        }
        if (playerId != GetCurrentPlayer().Id)
        {
            return MoveResult.Fail("Not your turn.");
        }
        if (piece is null)
        {
            return MoveResult.Fail("This piece cannot move with the current roll.");
        }
        int roll = pendingRoll;
        bool isFinished = false;

        Coordinate? destination = null;
        if (GetState(piece) == PieceState.Base)
        {
            destination = GetCoordinate(piece.Color, 0); // own base cell -> start cell
        }
        else
        {
            isFinished = TryFinish(piece.Id); // exact roll -> moves the piece into the finished cell.
            if (!isFinished)
            {
                int next = GetNextPosition(GetPosition(piece), roll);
                destination = GetCoordinate(piece.Color, next);
            }
        }

        if (destination != null)
        {
            // Remove selected piece that want to move
            // Place piece to destination coordinate
            GetCell(FindCell(piece.Id)).Pieces.Remove(piece);
            GetCell(destination.Value).Pieces.Add(piece);
        }

        OnPieceMoved?.Invoke(piece.Id, GetPosition(piece));

        bool isCaptured = GetState(piece) == PieceState.OnTrack && ResolveLanding(piece.Id);

        if (isFinished && GetFinishedPlayer()?.Id == playerId)
        {
            RegisterFinishedPlayer(playerId);

            // Stop when all but one player have finished.
            if (Rankings.Count >= players.Count - 1)
            {
                Phase = TurnPhase.Finished;
                pendingRoll = 0;

                // The first-place player remains the overall winner.
                OnGameEnded?.Invoke(Rankings[0].Id);
                return new MoveResult(
                    true, piece.Id, isCaptured, true, false, null);
            }
        }
        bool extraTurn = GrantsExtraTurn(roll, isCaptured, isFinished);
        EndTurn(extraTurn);
        return new MoveResult(true, piece.Id, isCaptured, true, extraTurn, null);
    }

    /// <summary>
    /// Move a piece from piece position
    /// </summary>
    /// <param name="playerId">current player id turn</param>
    /// <param name="position">piece relative position that want to move</param>
    /// <returns></returns>
    public MoveResult MovePiece(int playerId, int position)
    {
        IPiece? piece = GetMovablePieces()
            .Where(p => GetPosition(p) == position)
            .OrderBy(p => p.Id)
            .FirstOrDefault();
        if (piece is null)
        {
            return MovePieceCore(playerId, null);
        }
        return MovePieceCore(playerId, piece);
    }

    public MoveResult MovePieceById(int playerId, int pieceId) // addons
    {
        IPiece? piece = GetMovablePieces().FirstOrDefault(p => p.Id == pieceId);
        return MovePieceCore(playerId, piece);
    }

    public IPlayer GetPlayer(int playerId) =>
        players.Find(player => player.Id == playerId)
        ?? throw new ArgumentException($"Player with id {playerId} not found", nameof(playerId));

    public IPiece GetPiece(int pieceId)
    {
        return piecesByColor.Values.SelectMany(list => list)
            .FirstOrDefault(piece => piece.Id == pieceId)
        ?? throw new ArgumentException($"Piece with id {pieceId} not found", nameof(pieceId));
    }
    
    public IPlayer GetCurrentPlayer() => players[currentTurnIndex];
    
    public IPlayer? GetFinishedPlayer() => // before: GetWinner();
        players.FirstOrDefault(
            p => GetCell(board.HomeStretch[p.Color][^1]).Pieces.Count == 4
        );

    /// <summary>
    /// Roll the dice.
    /// </summary>
    /// <returns>Random number based on dice.Sides (1..6)</returns>
    private int NextRoll() => dice.Random.Next(1, dice.Sides + 1); // addons

    /// <summary>
    /// First roll to leave base must 6.
    /// </summary>
    /// <param name="roll">dice roll number</param>
    /// <returns>true if 6</returns>
    private bool CanLeaveBase(int roll) => roll == 6;

    /// <summary>
    /// Only capture if type is Normal (CellType.Normal). Not captured in Start and Safe.
    /// </summary>
    /// <param name="type">Type of Cell that current player on.</param>
    /// <param name="isOwn">Is other Piece() is it own current player.</param>
    /// <returns>true: can capture other piece. false: can't capture other piece.</returns>
    private bool CanCapture(CellType type, bool isOwn) => type == CellType.Normal && !isOwn;
    
    /// <summary>
    /// Conditional for granting extra turn to current player.
    /// There's 3 possible condition for this:
    ///  Roll a 6, Capture an opponent's Piece(), or Current player Piece() move to Finished position 
    /// </summary>
    /// <param name="roll">current roll dice number</param>
    /// <param name="captured">is current player captured an opponent's Piece()</param>
    /// <param name="finished">is current player Piece() move to Finished position</param>
    /// <returns>boolean</returns>
    private bool GrantsExtraTurn(int roll, bool captured, bool finished) => roll == 6 || captured || finished;
    
    private bool IsTripleSix(int count) => count >= 3;
    
    private bool CanFinish(int stepLeft, int roll) => stepLeft == roll;

    /// <summary>
    /// Counts Consecutive Sixes for current player.
    /// Unallowed player to play again if player get six 3 times in a row.
    /// </summary>
    /// <param name="roll">current roll dice number</param>
    /// <returns>true if triple six</returns>
    private bool TrackRoll(int roll) {
        ConsecutiveSixes = roll == 6 ? ConsecutiveSixes + 1 : 0;
        return IsTripleSix(ConsecutiveSixes);
    }

    /// <summary>
    /// Third six: No move for current player, reset all counter 
    /// (ConsecutiveSixes and PendingRoll) pass the turn to next player.
    /// </summary>
    private void ForfeitTurn() => EndTurn(false);

   /// <summary>
   /// Passes the turn to next player, or keeps it for a bonus roll.
   /// </summary>
   /// <param name="extraTurn">true if player got consecutive sixes < 3, otherwise false.</param>
   private void EndTurn(bool extraTurn)
    {
        pendingRoll = 0;
        if (!extraTurn)
        {
            ConsecutiveSixes = 0;
            currentTurnIndex = (currentTurnIndex + 1) % players.Count;
            Turn++;
        }
        Phase = TurnPhase.WaitingForRoll;
        OnTurnChanged?.Invoke(GetCurrentPlayer().Id);
    }

    /// <summary>
    /// Find Piece() by pieceId in Board.Cells.
    /// </summary>
    /// <param name="pieceId">Piece.Id</param>
    /// <returns>Coordinate of Piece in board Cell.</returns>
    /// <exception cref="ArgumentException">If pieceId not on Board.</exception>
    public Coordinate FindCell(int pieceId)
    {
        for (int row = 0; row < Board.Size; row++)
        {
            for (int col = 0; col < Board.Size; col++)
            {
                ICell cell = board.Cells[row, col];
                if (cell?.Pieces.Any(p => p.Id == pieceId) == true)
                    return new Coordinate(row, col);
            }
        }
        throw new ArgumentException(
            $"Piece {pieceId} is not on the board.",
            nameof(pieceId));
    }


    /// <summary>
    /// Derived relative position: -1 Base, 0..50 track, 51..55 home column, 56 finish.
    /// </summary>
    /// <param name="piece">Piece.Id</param>
    /// <returns>
    /// if piece in Base return -1,
    /// else return relative index (0..50 track, 51..55 home column, 56 finish)
    /// </returns>
    public int GetPosition(IPiece piece)
    {
        Coordinate pieceCoordinate = FindCell(piece.Id);
        if (GetCell(pieceCoordinate).Type == CellType.Base) // still on base
        {
            return -1;
        }
        int stretchIndex = board.HomeStretch[piece.Color].IndexOf(pieceCoordinate); // index player on home stretch
        if (stretchIndex >= 0) // if on home stretch
        {
            return HomeColumnStart + stretchIndex;
        }
        int outerRingIndex = board.Track.IndexOf(pieceCoordinate); // index player on outer ring
        return (outerRingIndex - board.StartIndex[piece.Color] + board.Track.Count) % board.Track.Count;
    }

    /// <summary>Derived state, from the Type of the containing cell.</summary>
    public PieceState GetState(IPiece piece) 
        => GetCell(FindCell(piece.Id)).Type switch
        {
            CellType.Base => PieceState.Base,
            CellType.HomeColumn => PieceState.HomeColumn,
            CellType.HomeTriangle => PieceState.Finished,
            _ => PieceState.OnTrack // Normal, Start, Safe CellType
        };

    /// <summary>
    /// Returns the current player's pieces that can legally move
    /// based on the current dice roll.
    /// Returns an empty list if the game is not waiting for a move.
    /// </summary>
    /// <returns>List of Movable Piece</returns>
    private List<IPiece> GetMovablePieces()
    {
        if (Phase != TurnPhase.WaitingForMove)
        {
            return [];
        }
        PlayerColor currentPlayerColor = playerColors[GetCurrentPlayer().Id];
        return piecesByColor[currentPlayerColor]
            .Where(p => IsLegalMove(p, pendingRoll))
            .ToList();
    }

    private bool IsLegalMove(IPiece piece, int roll)
    {
        switch (GetState(piece))
        {
            case PieceState.Finished:
                return false;
            case PieceState.Base:
                return CanLeaveBase(roll); // can leave if roll = 6
            default:
                int position = GetPosition(piece);
                int next = GetNextPosition(position, roll);
                if (next == -1)
                {
                    return false;
                }
                if (next == FinishPosition)
                {
                    return CanFinish(GetStepLeft(position), roll);
                }
                return true;
        }
    }

    private List<IPiece> GetPiecesAt(int row, int col) => board.Cells[row, col].Pieces.ToList();

    /// <summary>
    /// Relative position -> board cell. -1 = the first empty Base cell of the color (used when sending
    /// a piece home); 0..50 = Track[(StartIndex + position) % 52]; 51..56 = HomeStretch[position - 51].
    /// </summary>
    private Coordinate GetCoordinate(PlayerColor color, int position)
    {
        if (position == -1)
        {
            foreach (Coordinate coordinate in board.BaseCells[color])
            {
                if (GetCell(coordinate).Pieces.Count == 0)
                {
                    return coordinate;
                }
            }
            throw new InvalidOperationException($"No empty Base cell for {color}.");
        }
        if (position >= 0 && position <= LastTrackPosition)
        {
            return board.Track[(board.StartIndex[color] + position) % board.Track.Count];
        }
        if (position >= HomeColumnStart && position <= FinishPosition)
        {
            return board.HomeStretch[color][position - HomeColumnStart];
        }
        throw new ArgumentOutOfRangeException(nameof(position), $"Position {position} is outside -1..{FinishPosition}.");
    }
    
    private ICell GetCell(Coordinate coordinate) => 
        board.Cells[coordinate.Row, coordinate.Col]
        ?? throw new InvalidOperationException($"No Cell at ({coordinate.Row},{coordinate.Col}).");
    
    /// <summary>
    /// Next position
    /// </summary>
    /// <param name="position">current position index</param>
    /// <param name="steps">step added</param>
    /// <returns>-1 if position+ steps > Finished position (no move), else next Position</returns>
    private int GetNextPosition(int position, int steps)
    { 
        int nextPosition = position + steps;
        return nextPosition > FinishPosition ? -1 : nextPosition;
        
    }

   private int GetStepLeft(int position) => FinishPosition - position;
    
    /// <summary>
    /// After a move: capture opponents in the landing cell if the rules allow. Returns true if any.
    /// </summary>
    private bool ResolveLanding(int pieceId)
    {
        IPiece moverPiece = GetPiece(pieceId);
        Coordinate moverPieceAt = FindCell(pieceId);
        CellType type = GetCell(moverPieceAt).Type;
        bool isCaptured = false;

        foreach (IPiece other in GetPiecesAt(moverPieceAt.Row, moverPieceAt.Col))
        {
            // if (other.Id == moverPiece.Id) // exclude self piece
            // {
            //     continue;
            // }
            bool isOwn = other.Color == moverPiece.Color; // own piece in same cell
            if (CanCapture(type, isOwn))
            {
                CapturePiece(other.Id);
                isCaptured = true;
            }
        }
        return isCaptured;
    }

    /// <summary>Victim goes to the first empty Base cell of its own color (needs a 6 to leave again).</summary>
    private void CapturePiece(int victimId)
    {
        IPiece victim = GetPiece(victimId);
        GetCell(FindCell(victimId)).Pieces.Remove(victim); // remove victim from cell
        GetCell(GetCoordinate(victim.Color, -1)).Pieces.Add(victim); // Place victim to Base
        OnPieceCaptured?.Invoke(victimId);
    }

    /// <summary>
    /// Exact roll into the finish cell -> the piece is now contained there (= Finished). False if not exact.
    /// </summary>
    private bool TryFinish(int pieceId)
    {
        IPiece piece = GetPiece(pieceId);
        if (!CanFinish(GetStepLeft(GetPosition(piece)), pendingRoll))
        {
            return false;
        }
        // Take the piece out of its home-stretch cell and put it in the color's finish cell (the last
        // HomeStretch entry). Being in that cell is what "Finished" means; nothing is set on the piece.
        GetCell(FindCell(piece.Id)).Pieces.Remove(piece);
        GetCell(board.HomeStretch[piece.Color][^1]).Pieces.Add(piece);
        return true;
    }

    private void RegisterFinishedPlayer(int playerId) // addons
    {
        if (Rankings.Any(p => p.Id == playerId))
            return;

        IPlayer player = GetPlayer(playerId);
        Rankings.Add(player);

        int rank = Rankings.Count;
        OnPlayerFinished?.Invoke(playerId, rank);
    }
}