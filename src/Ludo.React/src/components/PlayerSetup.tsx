// Let 2-4 players choose a name and a color before starting a game.

import type {
  CreatePlayerRequest,
  PlayerColor,
} from '../types/game'

type Props = {
  players: CreatePlayerRequest[]
  onPlayersChange: (players: CreatePlayerRequest[]) => void
  onStartGame: () => void
  loading: boolean
  creating: boolean
}

const MIN_PLAYERS = 2
const MAX_PLAYERS = 4

const COLOR_OPTIONS: Array<{
  value: PlayerColor
  label: string
  radioClass: string
  badgeClass: string
}> = [
  {
    value: 'Red',
    label: 'Red',
    radioClass: 'radio-error',
    badgeClass: 'bg-red-500 text-white',
  },
  {
    value: 'Green',
    label: 'Green',
    radioClass: 'radio-success',
    badgeClass: 'bg-green-500 text-white',
  },
  {
    value: 'Yellow',
    label: 'Yellow',
    radioClass: 'radio-warning',
    badgeClass: 'bg-yellow-400 text-yellow-950',
  },
  {
    value: 'Blue',
    label: 'Blue',
    radioClass: 'radio-info',
    badgeClass: 'bg-blue-500 text-white',
  },
]

function getColorOption(color: PlayerColor) {
  return COLOR_OPTIONS.find((option) => option.value === color)
}

export default function PlayerSetup({
  players,
  onPlayersChange,
  onStartGame,
  loading,
  creating,
}: Props) {
  const allNamesFilled = players.every(
    (player) => player.name.trim().length > 0,
  )
  const uniqueColors =
    new Set(players.map((player) => player.color)).size ===
    players.length
  const playerCountValid =
    players.length >= MIN_PLAYERS && players.length <= MAX_PLAYERS
  const canStart =
    playerCountValid && allNamesFilled && uniqueColors

  function handleNameChange(index: number, name: string) {
    onPlayersChange(
      players.map((player, playerIndex) =>
        playerIndex === index ? { ...player, name } : player,
      ),
    )
  }

  // Keep colors unique. Picking a color that another player already owns
  // swaps the two colors instead of creating an invalid duplicate.
  function handleColorChange(index: number, color: PlayerColor) {
    const previousColor = players[index].color

    onPlayersChange(
      players.map((player, playerIndex) => {
        if (playerIndex === index) {
          return { ...player, color }
        }

        if (player.color === color) {
          return { ...player, color: previousColor }
        }

        return player
      }),
    )
  }

  function handleAddPlayer() {
    if (players.length >= MAX_PLAYERS) return

    const usedColors = new Set(players.map((player) => player.color))
    const availableColor =
      COLOR_OPTIONS.find((option) => !usedColors.has(option.value))
        ?.value ?? 'Red'

    onPlayersChange([
      ...players,
      { name: '', color: availableColor },
    ])
  }

  function handleRemovePlayer(index: number) {
    if (players.length <= MIN_PLAYERS) return

    onPlayersChange(
      players.filter((_, playerIndex) => playerIndex !== index),
    )
  }

  return (
    <section className="card border border-base-300 bg-base-200 shadow-xl">
      <div className="card-body gap-5">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="card-title text-2xl">Set up players</h2>
            <p className="text-sm text-base-content/70">
              Choose 2 to 4 players, give each a name, and pick a
              color.
            </p>
          </div>

          <span className="badge badge-lg badge-neutral">
            {players.length} / {MAX_PLAYERS} players
          </span>
        </div>

        <div className="flex flex-col gap-3">
          {players.map((player, index) => {
            const colorOption = getColorOption(player.color)

            return (
              <div
                key={index}
                className="rounded-box border border-base-300 bg-base-100 p-3"
              >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
                  <div className="flex min-w-0 flex-1 items-center gap-3">
                    <span
                      className={`grid h-9 w-9 shrink-0 place-items-center rounded-full text-sm font-bold ${
                        colorOption?.badgeClass ??
                        'bg-base-300 text-base-content'
                      }`}
                      aria-hidden="true"
                    >
                      {index + 1}
                    </span>

                    <input
                      type="text"
                      className="input w-full"
                      placeholder={`Player ${index + 1} name`}
                      value={player.name}
                      maxLength={20}
                      autoComplete="off"
                      disabled={loading}
                      aria-label={`Player ${index + 1} name`}
                      onChange={(event) =>
                        handleNameChange(index, event.target.value)
                      }
                    />
                  </div>

                  <div
                    className="flex flex-wrap items-center gap-x-4 gap-y-2"
                    role="radiogroup"
                    aria-label={`Player ${index + 1} color`}
                  >
                    {COLOR_OPTIONS.map((option) => (
                      <label
                        key={option.value}
                        className="flex cursor-pointer items-center gap-2"
                        title={option.label}
                      >
                        <input
                          type="radio"
                          name={`player-color-${index}`}
                          value={option.value}
                          className={`radio radio-sm ${option.radioClass}`}
                          checked={player.color === option.value}
                          disabled={loading}
                          onChange={() =>
                            handleColorChange(index, option.value)
                          }
                        />
                        <span className="text-sm">{option.label}</span>
                      </label>
                    ))}
                  </div>

                  <button
                    type="button"
                    className="btn btn-ghost btn-sm sm:ml-auto"
                    onClick={() => handleRemovePlayer(index)}
                    disabled={loading || players.length <= MIN_PLAYERS}
                    aria-label={`Remove player ${index + 1}`}
                  >
                    Remove
                  </button>
                </div>
              </div>
            )
          })}
        </div>

        <div className="flex flex-wrap items-center gap-3">
          <button
            type="button"
            className="btn btn-outline"
            onClick={handleAddPlayer}
            disabled={players.length >= MAX_PLAYERS || loading}
          >
            + Add player
          </button>

          <button
            type="button"
            className="btn btn-primary"
            onClick={onStartGame}
            disabled={!canStart || loading}
          >
            {creating ? 'Starting game...' : 'Start game'}
          </button>
        </div>

        <div className="flex flex-col gap-1">
          {!allNamesFilled && (
            <p className="text-sm text-warning">
              Enter a name for every player.
            </p>
          )}

          {!uniqueColors && (
            <p className="text-sm text-error">
              Each player needs a different color.
            </p>
          )}

          {!playerCountValid && (
            <p className="text-sm text-error">
              A game needs between {MIN_PLAYERS} and {MAX_PLAYERS}{' '}
              players.
            </p>
          )}

          <p className="text-xs text-base-content/50">
            Choosing a color that is already in use swaps the two
            players' colors.
          </p>
        </div>
      </div>
    </section>
  )
}
