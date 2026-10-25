// manages game state, API calls, loading, errors, dice results, and piece selection.
// Display debugging information, such as the game-state JSON

import { useEffect, useState } from 'react'
import {
  createGame,
  getGame,
  joinGame,
  rollDice,
  movePiece,
} from '../api/games'
import type {
  CreatePlayerRequest,
  GameStateDto,
} from '../types/game'

export function useLudoGame() {
  const [game, setGame] = useState<GameStateDto | null>(null)
  const [error, setError] = useState<string | null>(null)
  type GameAction = 'create' | 'roll' | 'move'
  const [activeAction, setActiveAction] = useState<GameAction | null>(null)
  const loading = activeAction !== null
  const [diceRoll, setDiceRoll] = useState<number | null>(null)
  const [selectedPieceId, setSelectedPieceId] = useState<number | null>(null)

  useEffect(() => {
    async function restoreGame() {
      const savedGameId = localStorage.getItem('ludoGameId')

      if (!savedGameId) return

      setActiveAction('create')
      setError(null)

      try {
        const savedGame = await getGame(savedGameId)
        setGame(savedGame)
      } catch (err) {
        // A deleted game should not keep failing on every reload.
        if (err instanceof Error && err.message.includes('(404)')) {
          localStorage.removeItem('ludoGameId')
        }

        setError(
          err instanceof Error
            ? err.message
            : 'Failed to restore the saved game',
        )
      } finally {
        setActiveAction(null)
      }
    }

    void restoreGame()
  }, [])

  async function handleCreateGame(players: CreatePlayerRequest[]) {
    if (loading) return

    setActiveAction('create')
    setError(null)
    setDiceRoll(null)
    setSelectedPieceId(null)

    try {
      const response = await createGame({
        players: players.map((player) => ({
          name: player.name.trim(),
          color: player.color,
        })),
      })

      localStorage.setItem('ludoGameId', response.state.gameId)
      setGame(response.state)
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'An unknown error occurred',
      )
    } finally {
      setActiveAction(null)
    }
  }

  function handleResetGame() {
    if (loading) return

    localStorage.removeItem('ludoGameId')
    setGame(null)
    setError(null)
    setDiceRoll(null)
    setSelectedPieceId(null)
  }

  async function handleJoinGame(gameId: string) {
    const trimmedGameId = gameId.trim()

    if (!trimmedGameId || loading) return

    setActiveAction('create')
    setError(null)
    setSelectedPieceId(null)
    setDiceRoll(null)

    try {
      const joinedGame = await joinGame(trimmedGameId)

      localStorage.setItem('ludoGameId', joinedGame.gameId)
      setGame(joinedGame)
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Failed to join game',
      )
    } finally {
      setActiveAction(null)
    }
  }
  
  async function handleRollDice() {
    if (!game || loading) return

    setActiveAction('roll')
    setError(null)
    setDiceRoll(null)
    setSelectedPieceId(null)

    try {
      
      const rollResponse = await rollDice(
        game.gameId,
        game.currentPlayerId,
      )

      setDiceRoll(rollResponse.roll)
      setGame(rollResponse.state)

      const movablePieces = rollResponse.state.movablePieces

      if (movablePieces.length === 1) {
        const pieceId = movablePieces[0].pieceId

        setActiveAction('move')
        setSelectedPieceId(pieceId)

        const moveResponse = await movePiece(
          rollResponse.state.gameId,
          {
            playerId: rollResponse.state.currentPlayerId,
            pieceId,
          },
        )

        setGame(moveResponse.state)
        setSelectedPieceId(null)
        setDiceRoll(null)
      }
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'An unknown error occurred',
      )
    } finally {
      setActiveAction(null)
    }
  }



  async function handleMovePiece(pieceId: number) {
    if (!game || loading) return

    const isMovable = game.movablePieces.some(
      (piece) => piece.pieceId === pieceId,
    )

    if (!isMovable) return

    setActiveAction('move')
    setError(null)
    setSelectedPieceId(pieceId)

    try {
      const response = await movePiece(game.gameId, {
        playerId: game.currentPlayerId,
        pieceId,
      })

      setGame(response.state)
      setSelectedPieceId(null)
      setDiceRoll(null)
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'An unknown error occurred',
      )
    } finally {
      setActiveAction(null)
    }
  }

  return {
    game,
    error,
    loading,
    activeAction,
    diceRoll,
    selectedPieceId,
    handleCreateGame,
    handleJoinGame,
    handleResetGame,
    handleRollDice,
    handleMovePiece,
  }
}
