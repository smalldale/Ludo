
import type {
  CreateGameRequest,
  CreateGameResponse,
  GameStateDto,
  RollDiceResponse,
  MoveRequest,
  MoveResponse,
} from '../types/game'

export async function createGame(
  request: CreateGameRequest,
): Promise<CreateGameResponse> {
  const response = await fetch('/api/games', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    const message = await response.text()
    throw new Error(
      `Failed to create game (${response.status}): ${message}`,
    )
  }
  return (await response.json()) as CreateGameResponse
}

export async function getGame(
  gameId: string,
): Promise<GameStateDto> {
  const response = await fetch(
    `/api/games/${encodeURIComponent(gameId)}`,
  )

  if (!response.ok) {
    const message = await response.text()
    throw new Error(
      `Failed to get game (${response.status}): ${message}`,
    )
  }

  return (await response.json()) as GameStateDto
}


export async function joinGame(
  gameId: string,
): Promise<GameStateDto> {
  return getGame(gameId)
}


export async function rollDice(
  gameId: string,
  playerId: number,
): Promise<RollDiceResponse> {
  const response = await fetch(
    `/api/games/${encodeURIComponent(gameId)}/roll`,
    {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ playerId }),
    },
  )

  if (!response.ok) {
    const message = await response.text()
    throw new Error(
      `Failed to roll dice (${response.status}): ${message}`,
    )
  }

  return (await response.json()) as RollDiceResponse
}

export async function movePiece(
  gameId: string,
  request: MoveRequest,
): Promise<MoveResponse> {
  const response = await fetch(
    `/api/games/${encodeURIComponent(gameId)}/move`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        playerId: request.playerId,
        pieceId: request.pieceId,
      }),
    },
  )

  if (!response.ok) {
    const message = await response.text()
    throw new Error(
      `Failed to move piece (${response.status}): ${message}`,
    )
  }

  return (await response.json()) as MoveResponse
}
