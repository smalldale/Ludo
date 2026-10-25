// Start a fresh game and roll the dice for the current turn.

type GameAction = 'create' | 'roll' | 'move'

type Props = {
  loading: boolean
  activeAction: GameAction | null
  canRoll: boolean
  onNewGame: () => void
  onRollDice: () => void
}

export default function GameControls({
  loading,
  activeAction,
  canRoll,
  onNewGame,
  onRollDice,
}: Props) {
  return (
    <div className="flex flex-wrap gap-3">
      <button
        type="button"
        className="btn btn-outline"
        onClick={onNewGame}
        disabled={loading}
      >
        New Game
      </button>

      <button
        type="button"
        className="btn btn-primary"
        onClick={onRollDice}
        disabled={loading || !canRoll}
      >
        {activeAction === 'roll' ? 'Rolling dice...' : 'Roll Dice'}
      </button>
    </div>
  )
}
