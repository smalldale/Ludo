
import {
  HubConnectionBuilder,
  HubConnectionState,
  type HubConnection,
} from '@microsoft/signalr'
import type { GameStateDto } from '../types/game'

let connection: HubConnection | null = null

export async function connectToGameHub(
  gameId: string,
  onGameUpdated: (state: GameStateDto) => void,
): Promise<void> {
  if (!connection) {
    connection = new HubConnectionBuilder()
      .withUrl('/hubs/game')
      .withAutomaticReconnect()
      .build()
  }

  // Register the handler before connecting so we don't miss updates.
  connection.off('GameUpdated')
  connection.on('GameUpdated', onGameUpdated)

  if (connection.state === HubConnectionState.Disconnected) {
    await connection.start()
  }

  await connection.invoke('JoinGame', gameId)
}

export async function disconnectFromGameHub(
  gameId: string,
): Promise<void> {
  if (!connection) return

  if (connection.state === HubConnectionState.Connected) {
    await connection.invoke('LeaveGame', gameId)
  }

  connection.off('GameUpdated')
  await connection.stop()
  connection = null
}
