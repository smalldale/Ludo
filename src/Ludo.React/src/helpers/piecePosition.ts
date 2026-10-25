import type {
  BoardDto,
  BoardCoordinateDto,
  PieceDto,
  BoardCellDto,
} from '../types/game'

type PiecePosition = Pick<PieceDto, 'id' | 'color' | 'position'>


export function getPieceCoordinate(
  piece: PiecePosition,
  board: BoardDto,
  cellsByTrackPosition?: Map<number, BoardCellDto>,
  trackLength?: number,
): BoardCoordinateDto | null {
  const { color, position, id } = piece

  // Piece is inside its base.
  if (position === -1) {
    const baseCells = board.baseCells[color]
    const pieceIndex = id % 4

    return baseCells?.[pieceIndex] ?? null
  }

  // Piece is on the main track.
  if (position >= 0 && position <= 50) {
    const length =
      trackLength ??
      board.cells.reduce(
        (max, cell) =>
          cell.trackPosition === null
            ? max
            : Math.max(max, cell.trackPosition + 1),
        0,
      )

    if (length === 0) {
      return null
    }

    const trackIndex =
      (board.startIndexes[color] + position) % length

    const cell = cellsByTrackPosition
      ? cellsByTrackPosition.get(trackIndex)
      : board.cells.find(
          (item) => item.trackPosition === trackIndex,
        )

    return cell
      ? { row: cell.row, column: cell.column }
      : null
  }

  // Piece is in the home stretch or at the finish.
  if (position >= 51 && position <= 56) {
    const homeCells = board.homeStretch[color]

    return homeCells?.[position - 51] ?? null
  }

  return null
}
