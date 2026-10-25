import type { GameStateDto } from '../types/game'
import { getPlayerTheme } from '../helpers/playerTheme'

type Props = {
  game: GameStateDto
}

function TrophyIcon({ className = 'h-5 w-5' }: { className?: string }) {
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
      <path d="M8 21h8" />
      <path d="M12 17v4" />
      <path d="M7 4h10v5a5 5 0 0 1-10 0V4Z" />
      <path d="M17 5h3v2a3 3 0 0 1-3 3" />
      <path d="M7 5H4v2a3 3 0 0 0 3 3" />
    </svg>
  )
}

function getInitial(name: string) {
  return name.trim().charAt(0).toUpperCase() || '?'
}

export default function GameResult({ game }: Props) {
  const rankings = game.rankings ?? []
  const winner = rankings[0]

  if (!winner) return null

  const winnerTheme = getPlayerTheme(winner.color)

  return (
    <section className="card overflow-hidden border border-warning/40 bg-base-100 shadow-xl">
      <div className="h-2 bg-gradient-to-r from-warning via-error to-warning" />

      <div className="card-body gap-5">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex items-center gap-4">
            <div className="grid h-14 w-14 shrink-0 place-items-center rounded-full bg-warning text-warning-content shadow-lg">
              <TrophyIcon className="h-8 w-8" />
            </div>

            <div>
              <p className="text-xs font-bold uppercase tracking-[0.2em] text-warning">
                Game over
              </p>
              <h2 className="text-2xl font-black sm:text-3xl">
                {winner.name} wins!
              </h2>
              <p className="text-sm text-base-content/70">
                {winner.color} player finished all four pieces first.
              </p>
            </div>
          </div>

          <div
            className={`badge badge-lg gap-2 self-start sm:self-center ${winnerTheme.badge}`}
          >
            <span
              className="h-2.5 w-2.5 rounded-full bg-current"
              aria-hidden="true"
            />
            Winner
          </div>
        </div>

        <div>
          <div className="mb-2 flex items-end justify-between gap-3">
            <h3 className="font-bold">Final ranking</h3>
            <span className="text-sm text-base-content/60">
              {rankings.length} players
            </span>
          </div>

          <div className="overflow-x-auto rounded-box border border-base-300 bg-base-100">
            <table className="table table-zebra">
              <thead>
                <tr>
                  <th className="w-20">Rank</th>
                  <th>Player</th>
                  <th>Color</th>
                  <th className="text-right">Result</th>
                </tr>
              </thead>
              <tbody>
                {rankings.map((ranking) => {
                  const theme = getPlayerTheme(ranking.color)
                  const isWinner = ranking.rank === 1

                  return (
                    <tr
                      key={ranking.playerId}
                      className={isWinner ? 'bg-warning/10' : undefined}
                    >
                      <td>
                        <span
                          className={`badge badge-lg font-black ${
                            isWinner ? 'badge-warning' : 'badge-outline'
                          }`}
                        >
                          {ranking.rank}
                        </span>
                      </td>

                      <td>
                        <div className="flex items-center gap-2">
                          <span
                            className={`grid h-8 w-8 shrink-0 place-items-center rounded-full text-xs font-black ${theme.avatar}`}
                          >
                            {getInitial(ranking.name)}
                          </span>
                          <span className="font-bold">{ranking.name}</span>
                          {isWinner && (
                            <span className="badge badge-sm badge-warning">
                              Champion
                            </span>
                          )}
                        </div>
                      </td>

                      <td>
                        <span
                          className={`badge badge-sm gap-1.5 ${theme.badge}`}
                        >
                          <span
                            className="h-1.5 w-1.5 rounded-full bg-current"
                            aria-hidden="true"
                          />
                          {ranking.color}
                        </span>
                      </td>

                      <td className="text-right">
                        {isWinner ? (
                          <span className="badge badge-warning">
                            Winner
                          </span>
                        ) : ranking.finished ? (
                          <span className="badge badge-success">
                            Finished
                          </span>
                        ) : (
                          <span className="badge badge-outline">
                            Not finished
                          </span>
                        )}
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </section>
  )
}
