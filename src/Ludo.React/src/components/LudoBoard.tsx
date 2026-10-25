// Render the board, the four bases, the home area, and the pieces.

import './LudoBoard.css'
import type {
  BoardCellDto,
  GameStateDto,
  PieceDto,
  PlayerColor,
} from '../types/game'
import { getPieceCoordinate } from '../helpers/piecePosition'
import { usePieceAnimation } from '../hooks/usePieceAnimation'

type Props = {
  game: GameStateDto
  selectedPieceId: number | null
  onPieceSelect: (pieceId: number) => void
  onAnimationChange?: (animating: boolean) => void
  loading: boolean
}

const BASE_COLORS: PlayerColor[] = ['Red', 'Green', 'Yellow', 'Blue']

const BASE_CORNER: Record<PlayerColor, string> = {
  Red: 'top-left',
  Green: 'top-right',
  Yellow: 'bottom-right',
  Blue: 'bottom-left',
}

function isBaseCorner(
  row: number,
  column: number,
  cornerSize: number,
  boardSize: number,
) {
  const top = row < cornerSize
  const bottom = row >= boardSize - cornerSize
  const left = column < cornerSize
  const right = column >= boardSize - cornerSize

  return (top || bottom) && (left || right)
}

function isCenter(
  row: number,
  column: number,
  centerStart: number,
  centerEnd: number,
) {
  return (
    row >= centerStart &&
    row <= centerEnd &&
    column >= centerStart &&
    column <= centerEnd
  )
}

function pieceNumber(piece: PieceDto) {
  return ((piece.id - 1) % 4) + 1
}

export default function LudoBoard({
  game,
  selectedPieceId,
  onPieceSelect,
  onAnimationChange,
  loading,
}: Props) {
  const { board, pieces, movablePieces } = game
  const { positions: displayPositions, animatingIds } =
    usePieceAnimation(pieces, game.gameId, onAnimationChange)
  const animatingIdSet = new Set(animatingIds)
  const isAnimating = animatingIds.length > 0
  const boardSize = board.boardSize
  const cornerSize = Math.floor(boardSize * 0.4)
  const centerStart = cornerSize
  const centerEnd = boardSize - cornerSize - 1

  const cellsByCoordinate = new Map<string, BoardCellDto>(
    board.cells.map((cell) => [
      `${cell.row}-${cell.column}`,
      cell,
    ]),
  )

  const cellsByTrackPosition = new Map(
    board.cells
      .filter((cell) => cell.trackPosition !== null)
      .map((cell) => [cell.trackPosition!, cell]),
  )

  const trackLength = cellsByTrackPosition.size
  const movableIds = new Set(
    movablePieces.map((movable) => movable.pieceId),
  )

  // Group pieces once instead of scanning the whole piece list for every cell.
  const piecesByCell = new Map<string, PieceDto[]>()

  for (const piece of pieces) {
    const coordinate = getPieceCoordinate(
      {
        ...piece,
        position: displayPositions[piece.id] ?? piece.position,
      },
      board,
      cellsByTrackPosition,
      trackLength,
    )

    if (!coordinate) continue

    const key = `${coordinate.row}-${coordinate.column}`
    const cellPieces = piecesByCell.get(key)

    if (cellPieces) {
      cellPieces.push(piece)
    } else {
      piecesByCell.set(key, [piece])
    }
  }

  // Draw the selected piece on top, then movable pieces, so a piece that can
  // be clicked is never buried under a stack of idle pieces.
  for (const cellPieces of piecesByCell.values()) {
    cellPieces.sort((left, right) => {
      const leftRank =
        left.id === selectedPieceId ? 2 : movableIds.has(left.id) ? 1 : 0
      const rightRank =
        right.id === selectedPieceId ? 2 : movableIds.has(right.id) ? 1 : 0

      return leftRank - rightRank || left.id - right.id
    })
  }

  const playerByColor = new Map<PlayerColor, string>()

  for (const player of game.players) {
    playerByColor.set(player.color, player.name)
  }

  const currentPlayer = game.players.find(
    (player) => player.id === game.currentPlayerId,
  )
  const turnColor = currentPlayer?.color.toLowerCase()

  return (
    <div
      className={[
        'ludo-board-shell',
        turnColor ? `turn-${turnColor}` : '',
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <div
        className="ludo-board"
        role="group"
        aria-label="Ludo board"
        style={{
          gridTemplateColumns: `repeat(${boardSize}, minmax(0, 1fr))`,
          gridTemplateRows: `repeat(${boardSize}, minmax(0, 1fr))`,
        }}
      >
        {BASE_COLORS.map((color) => (
          <div
            key={color}
            className={[
              'board-base-panel',
              `board-base-${color.toLowerCase()}`,
              `board-base-${BASE_CORNER[color]}`,
            ].join(' ')}
            aria-hidden="true"
          >
            <span className="board-base-inner" />
            <span className="board-base-label">
              {playerByColor.get(color) ?? color}
            </span>
          </div>
        ))}

        <div className="board-center" aria-hidden="true">
          <span className="board-center-triangle board-center-top" />
          <span className="board-center-triangle board-center-right" />
          <span className="board-center-triangle board-center-bottom" />
          <span className="board-center-triangle board-center-left" />
          <span className="board-center-pin" />
        </div>

        {Array.from(
          { length: boardSize * boardSize },
          (_, index) => {
            const row = Math.floor(index / boardSize)
            const column = index % boardSize
            const cellPieces =
              piecesByCell.get(`${row}-${column}`) ?? []

            const cell = cellsByCoordinate.get(`${row}-${column}`)
            const type = cell?.type ?? 'Empty'
            const color = cell?.color?.toLowerCase()

            const className = [
              'board-cell',
              `cell-${type.toLowerCase()}`,
              color ? `cell-${color}` : '',
              isBaseCorner(row, column, cornerSize, boardSize)
                ? 'in-base'
                : '',
              isCenter(row, column, centerStart, centerEnd)
                ? 'in-center'
                : '',
            ]
              .filter(Boolean)
              .join(' ')

            return (
              <div
                key={`${row}-${column}`}
                className={className}
                aria-label={`Row ${row + 1}, column ${column + 1}: ${type}`}
                title={`${type}${color ? ` (${color})` : ''}`}
              >
                {cellPieces.length > 0 && (
                  <div
                    className="cell-pieces"
                    data-count={cellPieces.length}
                  >
                    {cellPieces.map((piece) => {
                      const isMovable = movableIds.has(piece.id)
                      const canSelect = isMovable && !isAnimating
                      const isSelected = selectedPieceId === piece.id
                      const isAnimatingPiece = animatingIdSet.has(
                        piece.id,
                      )
                      const number = pieceNumber(piece)
                      const position =
                        displayPositions[piece.id] ?? piece.position

                      return (
                        <button
                          key={piece.id}
                          type="button"
                          disabled={!canSelect}
                          className={[
                            'piece',
                            `piece-${piece.color.toLowerCase()}`,
                            canSelect ? 'piece-movable' : '',
                            isSelected ? 'piece-selected' : '',
                            isAnimatingPiece ? 'piece-animating' : '',
                          ]
                            .filter(Boolean)
                            .join(' ')}
                          aria-label={`${piece.color} piece ${number}${
                            canSelect ? ', movable' : ''
                          }`}
                          aria-pressed={isSelected}
                          title={`${piece.color} piece ${number}${
                            canSelect ? ' - click to move' : ''
                          }`}
                          data-state={piece.state}
                          data-piece-id={piece.id}
                          data-position={position}
                          onClick={() => onPieceSelect(piece.id)}
                        >
                          <span className="piece-number">
                            {number}
                          </span>
                        </button>
                      )
                    })}
                  </div>
                )}

                {cellPieces.length > 1 && (
                  <span className="cell-piece-count" aria-hidden="true">
                    {cellPieces.length}
                  </span>
                )}
              </div>
            )
          },
        )}

        {loading && (
          <div className="board-loading" role="status" aria-live="polite">
            <span className="board-loading-pill">
              <span className="board-spinner" aria-hidden="true" />
              {selectedPieceId !== null
                ? 'Moving piece...'
                : 'Updating board...'}
            </span>
          </div>
        )}

        {isAnimating && !loading && (
          <div
            className="board-animation-status"
            role="status"
            aria-live="polite"
          >
            <span className="board-spinner" aria-hidden="true" />
            Moving piece...
          </div>
        )}
      </div>
    </div>
  )
}
