import { useCallback, useState } from 'react'
import { useLudoGame } from './hooks/useLudoGame'
import LudoBoard from './components/LudoBoard'
import GameStatus from './components/GameStatus'
import GameControls from './components/GameControls'
import PlayerSetup from './components/PlayerSetup'
import { getPlayerTheme } from './helpers/playerTheme'
import type { CreatePlayerRequest } from './types/game'
import './App.css'

const INITIAL_PLAYERS: CreatePlayerRequest[] = [
  { name: '', color: 'Red' },
  { name: '', color: 'Green' },
]

function App() {
  const {
    game,
    error,
    loading,
    activeAction,
    diceRoll,
    selectedPieceId,
    handleCreateGame,
    handleResetGame,
    handleRollDice,
    handleMovePiece,
  } = useLudoGame()

  const [players, setPlayers] = useState<CreatePlayerRequest[]>(
    () => INITIAL_PLAYERS.map((player) => ({ ...player })),
  )
  const [isAnimating, setIsAnimating] = useState(false)

  const currentPlayer = game?.players.find(
    (player) => player.id === game.currentPlayerId,
  )
  const turnTheme = getPlayerTheme(currentPlayer?.color)
  const isFinished = game?.phase === 'Finished'

  const handleAnimationChange = useCallback((animating: boolean) => {
    setIsAnimating(animating)
  }, [])

  function handleStartGame() {
    void handleCreateGame(players)
  }

  function handleNewGame() {
    setIsAnimating(false)

    if (game) {
      setPlayers(
        game.players.map((player) => ({
          name: player.name,
          color: player.color,
        })),
      )
    }

    handleResetGame()
  }

  return (
    <main className="min-h-screen bg-base-100 text-base-content">
      <div className="mx-auto max-w-7xl p-4 sm:p-6 lg:p-8">
        <header className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <h1 className="text-3xl font-bold sm:text-4xl">
              Ludo
            </h1>
            <p className="mt-1 text-base-content/70">
              Roll the dice and move your pieces to victory.
            </p>

            {game && currentPlayer && (
              <div
                className={`mt-3 inline-flex flex-wrap items-center gap-2 rounded-full border px-3 py-1.5 text-sm shadow-sm ${turnTheme.soft} ${turnTheme.border}`}
              >
                <span
                  className={`h-2.5 w-2.5 rounded-full ${turnTheme.dot} ${
                    isFinished ? '' : 'animate-pulse'
                  }`}
                  aria-hidden="true"
                />
                <span className="text-base-content/60">
                  {isFinished ? 'Game finished' : 'Current turn'}
                </span>
                <span className="font-bold">{currentPlayer.name}</span>
                <span
                  className={`badge badge-sm ${
                    isFinished ? 'badge-neutral' : turnTheme.badge
                  }`}
                >
                  {isFinished ? 'Finished' : currentPlayer.color}
                </span>
              </div>
            )}
          </div>

          {game && (
            <GameControls
              loading={loading || isAnimating}
              activeAction={activeAction}
              canRoll={game.phase === 'WaitingForRoll'}
              phase={game.phase}
              onNewGame={handleNewGame}
              onRollDice={handleRollDice}
            />
          )}
        </header>

        {error && (
          <div className="alert alert-error mb-6" role="alert">
            <span>{error}</span>
          </div>
        )}

        {!game && (
          <div className="mx-auto max-w-3xl">
            <PlayerSetup
              players={players}
              onPlayersChange={setPlayers}
              onStartGame={handleStartGame}
              loading={loading}
              creating={activeAction === 'create'}
            />
          </div>
        )}

        {game && (
          <div className="grid grid-cols-1 items-start gap-6 lg:grid-cols-[minmax(0,1fr)_20rem]">
            <section className="min-w-0">
              <LudoBoard
                game={game}
                selectedPieceId={selectedPieceId}
                onPieceSelect={handleMovePiece}
                onAnimationChange={handleAnimationChange}
                loading={loading}
              />
            </section>

            <aside className="flex min-w-0 flex-col gap-4">
              <GameStatus
                game={game}
                diceRoll={diceRoll}
                animating={isAnimating}
              />

              <details className="collapse collapse-arrow border border-base-300 bg-base-200">
                <summary className="collapse-title font-medium">
                  Debug: Game State
                </summary>

                <div className="collapse-content">
                  <pre className="overflow-x-auto whitespace-pre-wrap break-words text-xs">
                    {JSON.stringify(game, null, 2)}
                  </pre>
                </div>
              </details>
            </aside>
          </div>
        )}
      </div>
    </main>
  )
}

export default App
