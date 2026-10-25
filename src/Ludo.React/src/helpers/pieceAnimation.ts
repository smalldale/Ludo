import type {
  BoardDto,
  BoardCellDto,
  BoardCoordinateDto,
  PieceDto,
} from '../types/game'
import { getPieceCoordinate } from './piecePosition'

type PiecePosition = Pick<PieceDto, 'id' | 'color' | 'position'>

export function getPieceAnimationPath(
  piece: PiecePosition,
  destinationPosition: number,
  board: BoardDto,
): BoardCoordinateDto[] {
  const { position } = piece

  // A piece leaving its base enters its color's starting cell.
  // A piece already on the board advances one position at a time.
  if (position === -1 && destinationPosition < 0) {
    return []
  }

  const trackCells = new Map<number, BoardCellDto>(
    board.cells
      .filter((cell) => cell.trackPosition !== null)
      .map((cell) => [cell.trackPosition!, cell]),
  )

  const trackLength = trackCells.size

  const path: BoardCoordinateDto[] = []

  for (
    let nextPosition = position + 1;
    nextPosition <= destinationPosition;
    nextPosition++
  ) {
    const coordinate = getPieceCoordinate(
      { ...piece, position: nextPosition },
      board,
      trackCells,
      trackLength,
    )

    if (coordinate) {
      path.push(coordinate)
    }
  }

  return path
}
