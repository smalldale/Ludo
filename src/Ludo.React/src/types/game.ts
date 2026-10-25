
export type PlayerColor = 'Red' | 'Green' | 'Yellow' | 'Blue'

export interface CreatePlayerRequest {
  name: string
  color: PlayerColor
}

export interface CreateGameRequest {
  players: CreatePlayerRequest[]
}

export interface PlayerDto {
  id: number
  name: string
  color: PlayerColor
}

export interface PieceDto {
  id: number
  color: PlayerColor
  position: number
  state: string
}

export interface MovablePieceDto {
  pieceId: number
  position: number
}

export interface BoardCellDto {
  row: number
  column: number
  type: string
  trackPosition: number | null
  color: PlayerColor | null
}

export interface BoardCoordinateDto {
  row: number
  column: number
}

export interface BoardDto {
  boardSize: number
  cells: BoardCellDto[]
  startIndexes: Record<PlayerColor, number>
  homeStretch: Record<PlayerColor, BoardCoordinateDto[]>
  baseCells: Record<PlayerColor, BoardCoordinateDto[]>
}

export interface GameStateDto {
  gameId: string
  turn: number
  phase: string
  currentPlayerId: number
  players: PlayerDto[]
  movablePieces: MovablePieceDto[]
  pieces: PieceDto[]
  board: BoardDto
}

export interface RollDiceResponse {
  roll: number
  state: GameStateDto
}

// movePiece
export interface MoveRequest {
  playerId: number
  pieceId: number
}

export interface MoveResultDto {
  success: boolean
  pieceId: number
  captured: boolean
  pieceFinished: boolean
  extraTurn: boolean
  error: string | null
}

export interface MoveResponse {
  result: MoveResultDto
  state: GameStateDto
}

export interface PieceDto {
  id: number
  color: PlayerColor
  position: number
  state: string
}

export interface PlayerAssignmentDto {
  playerId: number
  playerToken: string
}

export interface CreateGameResponse {
  state: GameStateDto
  assignments: PlayerAssignmentDto[]
}
