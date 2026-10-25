import { useCallback, useEffect, useRef, useState } from 'react'
import type { PieceDto } from '../types/game'

type PiecePositions = Record<number, number>

const STEP_DELAY_MS = 150

function toPositionMap(pieces: PieceDto[]): PiecePositions {
  const positions: PiecePositions = {}

  for (const piece of pieces) {
    positions[piece.id] = piece.position
  }

  return positions
}

// A move only ever moves forward one relative position at a time. A piece
// leaving the base goes straight to position 0; captured pieces move backward
// and are snapped home at the end instead of being animated backward.
function getForwardPath(from: number, to: number): number[] {
  if (to <= from) return []

  if (from === -1) {
    return Array.from({ length: to + 1 }, (_, index) => index)
  }

  return Array.from(
    { length: to - from },
    (_, index) => from + index + 1,
  )
}

function prefersReducedMotion() {
  return (
    typeof window !== 'undefined' &&
    window.matchMedia('(prefers-reduced-motion: reduce)').matches
  )
}

export function usePieceAnimation(
  pieces: PieceDto[],
  gameId: string,
  onAnimationChange?: (animating: boolean) => void,
) {
  const [positions, setPositions] = useState<PiecePositions>(() =>
    toPositionMap(pieces),
  )
  const [animatingIds, setAnimatingIds] = useState<number[]>([])

  const positionsRef = useRef(positions)
  const animatingRef = useRef(false)
  const onAnimationChangeRef = useRef(onAnimationChange)
  const timeoutRef = useRef<number | null>(null)
  const gameIdRef = useRef(gameId)

  useEffect(() => {
    onAnimationChangeRef.current = onAnimationChange
  }, [onAnimationChange])

  const updatePositions = useCallback((next: PiecePositions) => {
    positionsRef.current = next
    setPositions(next)
  }, [])

  const reportAnimation = useCallback((animating: boolean) => {
    if (animatingRef.current === animating) return

    animatingRef.current = animating
    onAnimationChangeRef.current?.(animating)
  }, [])

  useEffect(() => {
    const finalPositions = toPositionMap(pieces)
    const gameChanged = gameIdRef.current !== gameId
    gameIdRef.current = gameId

    let cancelled = false

    const cancel = () => {
      cancelled = true

      if (timeoutRef.current !== null) {
        window.clearTimeout(timeoutRef.current)
        timeoutRef.current = null
      }

      reportAnimation(false)
    }

    const snapToFinal = () => {
      if (cancelled) return

      updatePositions(finalPositions)
      setAnimatingIds([])
      reportAnimation(false)
      timeoutRef.current = null
    }

    if (gameChanged || prefersReducedMotion()) {
      timeoutRef.current = window.setTimeout(snapToFinal, 0)
      return cancel
    }

    const currentPositions = positionsRef.current
    const heldPositions: PiecePositions = { ...finalPositions }
    const animations: Array<{ id: number; path: number[] }> = []

    for (const piece of pieces) {
      const from = currentPositions[piece.id]

      if (from === undefined) continue

      // Keep every existing piece where it is on screen while the moving
      // pieces step forward. This also holds captured pieces in place until
      // the mover reaches its destination.
      heldPositions[piece.id] = from

      const path = getForwardPath(from, piece.position)

      if (path.length > 0) {
        animations.push({ id: piece.id, path })
      }
    }

    if (animations.length === 0) {
      timeoutRef.current = window.setTimeout(snapToFinal, 0)
      return cancel
    }

    let step = 0

    const runStep = () => {
      if (cancelled) return

      step += 1

      const stepPositions: PiecePositions = { ...heldPositions }
      let finished = true

      for (const animation of animations) {
        const stepIndex = Math.min(
          step - 1,
          animation.path.length - 1,
        )

        stepPositions[animation.id] = animation.path[stepIndex]

        if (step < animation.path.length) {
          finished = false
        }
      }

      updatePositions(stepPositions)

      if (finished) {
        updatePositions(finalPositions)
        setAnimatingIds([])
        reportAnimation(false)
        timeoutRef.current = null
        return
      }

      timeoutRef.current = window.setTimeout(
        runStep,
        STEP_DELAY_MS,
      )
    }

    const startAnimation = () => {
      if (cancelled) return

      setAnimatingIds(animations.map((animation) => animation.id))
      reportAnimation(true)
      timeoutRef.current = window.setTimeout(
        runStep,
        STEP_DELAY_MS,
      )
    }

    timeoutRef.current = window.setTimeout(startAnimation, 0)

    return cancel
  }, [gameId, pieces, reportAnimation, updatePositions])

  return {
    positions,
    animatingIds,
  }
}
