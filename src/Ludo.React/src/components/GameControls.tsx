// Start a fresh game and roll the dice for the current turn.

type GameAction = 'create' | 'roll' | 'move'

type Props = {
  loading: boolean
  activeAction: GameAction | null
  canRoll: boolean
  phase: string
  onNewGame: () => void
  onRollDice: () => void
}

function DiceIcon({ className = 'h-5 w-5' }: { className?: string }) {
  return (
    <svg
      className={className}
      viewBox="0 0 24 24"
      fill="none"
      aria-hidden="true"
    >
      <rect
        x="3"
        y="3"
        width="18"
        height="18"
        rx="4"
        stroke="currentColor"
        strokeWidth="2"
      />
      <circle cx="8" cy="8" r="1.5" fill="currentColor" />
      <circle cx="16" cy="8" r="1.5" fill="currentColor" />
      <circle cx="12" cy="12" r="1.5" fill="currentColor" />
      <circle cx="8" cy="16" r="1.5" fill="currentColor" />
      <circle cx="16" cy="16" r="1.5" fill="currentColor" />
    </svg>
  )
}

function RefreshIcon({ className = 'h-4 w-4' }: { className?: string }) {
  return (
    <svg
      className={className}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M21 12a9 9 0 1 1-2.64-6.36" />
      <path d="M21 3v6h-6" />
    </svg>
  )
}

export default function GameControls({
  loading,
  activeAction,
  canRoll,
  phase,
  onNewGame,
  onRollDice,
}: Props) {
  const isRolling = activeAction === 'roll'
  const waitingForMove = phase === 'WaitingForMove'
  const finished = phase === 'Finished'

  const rollLabel = isRolling
    ? 'Rolling dice...'
    : finished
      ? 'Game over'
      : waitingForMove
        ? 'Move a piece'
        : 'Roll Dice'

  const rollTitle = waitingForMove
    ? 'Select a highlighted piece on the board.'
    : finished
      ? 'Start a new game to play again.'
      : undefined

  return (
    <div className="flex flex-wrap items-center gap-2">
      <button
        type="button"
        className="btn btn-outline btn-lg gap-2"
        onClick={onNewGame}
        disabled={loading}
      >
        <RefreshIcon />
        New Game
      </button>

      <button
        type="button"
        className="btn btn-primary btn-lg gap-2 shadow-md"
        onClick={onRollDice}
        disabled={loading || !canRoll}
        title={rollTitle}
        aria-busy={isRolling}
      >
        {isRolling ? (
          <span className="loading loading-spinner loading-sm" />
        ) : (
          <DiceIcon />
        )}
        {rollLabel}
      </button>
    </div>
  )
}
