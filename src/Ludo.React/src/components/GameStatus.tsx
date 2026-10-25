// Show the current player, color, phase, and dice result


import type { GameStateDto } from '../types/game'

type Props = {
  game: GameStateDto
  diceRoll: number | null
}

const playerColorClasses: Record<string, string> = {
  Red: 'bg-red-600',
  Green: 'bg-green-600',
  Yellow: 'bg-yellow-400',
  Blue: 'bg-blue-600',
}

export default function GameStatus({ game, diceRoll }: Props) {
  const currentPlayer = game.players.find(
    (player) => player.id === game.currentPlayerId,
  )

  const colorClass = currentPlayer
    ? playerColorClasses[currentPlayer.color] ?? 'bg-base-300'
    : 'bg-base-300'

  const instruction =
    game.phase === 'WaitingForRoll'
        ? 'Roll the dice to continue your turn.'
        : game.phase === 'WaitingForMove'
        ? game.movablePieces.length > 0
            ? 'Select a highlighted piece to move.'
            : 'No movable pieces available.'
        : 'Waiting for the next game action.'

  return (
    <section className="card bg-base-200 shadow-md">
      <div className="card-body gap-3">
        <h2 className="card-title">Game Status</h2>

        <div className="flex items-center gap-3">
          <span
            className={`h-4 w-4 shrink-0 rounded-full ${colorClass}`}
            aria-hidden="true"
          />

          <div className="min-w-0">
            <p className="text-sm text-base-content/70">
              Current turn
            </p>
            <p className="text-lg font-bold">
              {currentPlayer?.name ?? 'Unknown'}
            </p>
          </div>

          <span className="badge badge-primary ml-auto">
            Active
          </span>
        </div>

        <div className="divider my-0" />

        <p>
          Color:{' '}
          <strong>{currentPlayer?.color ?? 'Unknown'}</strong>
        </p>

        <p>
          Phase: <span className="badge badge-outline">{game.phase}</span>
        </p>

        <div className="alert alert-info">
            <span>{instruction}</span>
        </div>

        {diceRoll !== null && (
          <div className="alert alert-success">
            <span>
              Dice result: <strong>{diceRoll}</strong>
            </span>
          </div>
        )}
      </div>
    </section>
  )
}
