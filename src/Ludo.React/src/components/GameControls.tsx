// Create a game and roll the dice

type GameAction = 'create' | 'roll' | 'move'

type Props = {
  loading: boolean
  activeAction: GameAction | null
  hasGame: boolean
  onCreateGame: () => void
  onRollDice: () => void
}

export default function GameControls({
  loading,
  activeAction,
  hasGame,
  onCreateGame,
  onRollDice,
}: Props) {
  return (
    <div className="flex flex-wrap gap-3">
      <button
        className="btn btn-primary"
        onClick={onCreateGame}
        disabled={loading}
      >
        {activeAction === 'create' ? 'Creating game...' : 'Create Game'}
      </button>

      {hasGame && (
        <button
          className="btn btn-secondary"
          onClick={onRollDice}
          disabled={loading}
        >
          {activeAction === 'roll' ? 'Rolling dice...' : 'Roll Dice'}
        </button>
      )}
    </div>
  )
}
