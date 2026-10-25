import type { GameStateDto } from '../types/game'
import { getPlayerTheme } from '../helpers/playerTheme'
import DiceRoll from './DiceRoll'

type Props = {
  game: GameStateDto
  diceRoll: number | null
  animating?: boolean
}

export default function GameStatus({
  game,
  diceRoll,
  animating = false,
}: Props) {
  const currentPlayer = game.players.find(
    (player) => player.id === game.currentPlayerId,
  )
  const isFinished = game.phase === 'Finished'
  const winner = game.rankings?.find((ranking) => ranking.rank === 1)
  const winnerPlayer = winner
    ? game.players.find((player) => player.id === winner.playerId)
    : undefined
  const displayPlayer = (isFinished ? winnerPlayer : undefined) ?? currentPlayer
  const theme = getPlayerTheme(displayPlayer?.color)

  const instruction = animating
    ? 'Moving the piece...'
    : game.phase === 'WaitingForRoll'
      ? 'Roll the dice to continue your turn.'
      : game.phase === 'WaitingForMove'
        ? game.movablePieces.length > 0
          ? 'Select a highlighted piece to move.'
          : 'No movable pieces available.'
        : game.phase === 'Finished'
          ? 'Game finished. Start a new game to play again.'
          : 'Waiting for the next game action.'

  const instructionAlert =
    animating || game.phase === 'WaitingForRoll'
      ? 'alert-info'
      : game.phase === 'WaitingForMove'
        ? 'alert-warning'
        : game.phase === 'Finished'
          ? 'alert-success'
          : 'alert-info'

  const phaseBadge =
    game.phase === 'WaitingForRoll'
      ? 'badge-info'
      : game.phase === 'WaitingForMove'
        ? 'badge-warning'
        : game.phase === 'Finished'
          ? 'badge-success'
          : 'badge-neutral'

  const phaseLabel =
    game.phase === 'WaitingForRoll'
      ? 'Roll dice'
      : game.phase === 'WaitingForMove'
        ? 'Move a piece'
        : game.phase === 'Finished'
          ? 'Finished'
          : game.phase

  const initial = displayPlayer?.name.trim().charAt(0).toUpperCase() || '?'

  return (
    <section className="card border border-base-300 bg-base-200 shadow-lg">
      <div className="card-body gap-4">
        <div className="flex items-center justify-between gap-3">
          <h2 className="card-title">Game Status</h2>
          <span className={`badge badge-sm ${phaseBadge}`}>{phaseLabel}</span>
        </div>

        {displayPlayer ? (
          <div
            className={`rounded-box border p-4 ${theme.soft} ${theme.border}`}
            aria-live="polite"
          >
            <div className="flex items-center justify-between gap-2">
              <p className="whitespace-nowrap text-xs font-semibold uppercase tracking-[0.16em] text-base-content/60">
                {isFinished ? 'Winner' : 'Current turn'}
              </p>

              <span
                className={`badge badge-sm gap-1.5 ${
                  isFinished ? 'badge-warning' : theme.badge
                }`}
              >
                {!isFinished && (
                  <span
                    className="h-1.5 w-1.5 animate-pulse rounded-full bg-current"
                    aria-hidden="true"
                  />
                )}
                {isFinished ? 'Champion' : 'Playing'}
              </span>
            </div>

            <div className="mt-2 flex items-center gap-3">
              <div
                className={`grid h-12 w-12 shrink-0 place-items-center rounded-full text-lg font-black shadow-inner ${theme.avatar}`}
              >
                {initial}
              </div>

              <div className="min-w-0 flex-1">
                <p className="truncate text-2xl font-black leading-tight">
                  {displayPlayer.name}
                </p>
                <p className="text-sm text-base-content/70">
                  {displayPlayer.color} player
                </p>
              </div>
            </div>
          </div>
        ) : (
          <div className="alert alert-warning">
            <span>No active player.</span>
          </div>
        )}

        <div className="grid grid-cols-2 gap-3">
          <div className="rounded-box border border-base-300 bg-base-100 p-3">
            <p className="text-xs font-semibold uppercase tracking-wide text-base-content/50">
              Color
            </p>
            <p className="mt-1 flex items-center gap-2 font-semibold">
              <span
                className={`h-3 w-3 rounded-full ${theme.dot}`}
                aria-hidden="true"
              />
              {displayPlayer?.color ?? 'Unknown'}
            </p>
          </div>

          <div className="rounded-box border border-base-300 bg-base-100 p-3">
            <p className="text-xs font-semibold uppercase tracking-wide text-base-content/50">
              Phase
            </p>
            <p className="mt-1 truncate font-semibold">{phaseLabel}</p>
          </div>
        </div>

        <div className={`alert ${instructionAlert}`} role="status">
          {animating && (
            <span className="loading loading-spinner loading-sm" />
          )}
          <span>{instruction}</span>
        </div>

        {diceRoll !== null && <DiceRoll value={diceRoll} />}
      </div>
    </section>
  )
}
