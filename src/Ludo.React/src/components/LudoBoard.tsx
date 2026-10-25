//Render the board and pieces

import './LudoBoard.css'
import type {
  BoardCellDto,
  GameStateDto
} from '../types/game'
import { getPieceCoordinate } from '../helpers/piecePosition'

type Props = {
  game: GameStateDto
  selectedPieceId: number | null
  onPieceSelect: (pieceId: number) => void
  loading: boolean
}


function getBaseAreaStyle(
  row: number,
  column: number,
  baseCells: BoardCellDto[],
): string | null {
  for (const cell of baseCells) {
    if (!cell.color) continue

    // The base cell is part of a 2×2 block.
    // Find its top-left corner from the color's base-cell coordinates.
    const sameColorCells = baseCells.filter(
      (candidate) => candidate.color === cell.color,
    )

    const minRow = Math.min(...sameColorCells.map((c) => c.row))
    const maxRow = Math.max(...sameColorCells.map((c) => c.row))
    const minCol = Math.min(...sameColorCells.map((c) => c.column))
    const maxCol = Math.max(...sameColorCells.map((c) => c.column))

    if (
      row >= minRow - 1 &&
      row <= maxRow + 1 &&
      column >= minCol - 1 &&
      column <= maxCol + 1
    ) {
      const isBaseCell = sameColorCells.some(
        (c) => c.row === row && c.column === column,
      )

      if (isBaseCell) return null

      const isBorder =
        row === minRow - 1 ||
        row === maxRow + 1 ||
        column === minCol - 1 ||
        column === maxCol + 1

      return isBorder
        ? 'cell-base-border'
        : `cell-base-area cell-${cell.color.toLowerCase()}`
    }
  }

  return null
}


export default function LudoBoard({
  game,
  selectedPieceId,
  onPieceSelect,
  loading,
}: Props) {
  const { board, pieces, movablePieces } = game
  const baseCells = board.cells.filter(
    (cell) => cell.type === 'Base',
  )
  const cellsByCoordinate = new Map<string, BoardCellDto>(
    board.cells.map((cell) => [
      `${cell.row}-${cell.column}`,
      cell,
    ]),
  )

  const cellsByTrackPosition = new Map(
    board.cells
      .filter(
        (cell) => cell.trackPosition !== null,
      )
      .map((cell) => [cell.trackPosition!, cell]),
  )

  const trackLength = cellsByTrackPosition.size

  return (
    <div className="relative">
    {loading && (
      // <div className="alert alert-info mb-3">
      //   <span className="loading loading-spinner loading-sm" />
      //   <span>
      //     {selectedPieceId !== null
      //       ? 'Moving piece...'
      //       : 'Processing action...'}
      //   </span>
      // </div>
      <div></div>
    )}

    <div
      className="ludo-board"
      style={{
        gridTemplateColumns: `repeat(${board.boardSize}, minmax(0, 1fr))`,
        gridTemplateRows: `repeat(${board.boardSize}, minmax(0, 1fr))`,
      }}
    >
      {Array.from(
        { length: board.boardSize * board.boardSize },
        (_, index) => {
          const row = Math.floor(index / board.boardSize)
          const column = index % board.boardSize

          const cellPieces = pieces.filter((piece) => {
            const coordinate = getPieceCoordinate(
              piece,
              board,
              cellsByTrackPosition,
              trackLength,
            )

            return (
              coordinate?.row === row &&
              coordinate?.column === column
            )
          })


          const cell = cellsByCoordinate.get(`${row}-${column}`)
          const type = cell?.type ?? 'Empty'
          const color = cell?.color?.toLowerCase()

          const baseAreaClass = getBaseAreaStyle(
            row,
            column,
            baseCells,
            )

          const className = [
          'board-cell',
          `cell-${type.toLowerCase()}`,
          color ? `cell-${color.toLowerCase()}` : '',
          baseAreaClass ?? '',
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
              {cellPieces.map((piece) => {
                const isMovable = movablePieces.some(
                  (movable) => movable.pieceId === piece.id,
                )

                const isSelected = selectedPieceId === piece.id

                return (
                  <button
                    key={piece.id}
                    type="button"
                    disabled={!isMovable}
                    className={[
                      'piece',
                      `piece-${piece.color.toLowerCase()}`,
                      isMovable ? 'piece-movable' : '',
                      isSelected ? 'piece-selected' : '',
                    ]
                      .filter(Boolean)
                      .join(' ')}
                    title={`${piece.color} piece ${piece.id}`}
                    onClick={() => onPieceSelect(piece.id)}
                  >
                    {(piece.id % 4) + 1}
                  </button>
                )
              })}
            </div>
          )
        },
      )}
    </div>
    </div>
  )
}
