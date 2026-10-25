import type { PlayerColor } from '../types/game'

type PlayerTheme = {
  dot: string
  soft: string
  border: string
  badge: string
  avatar: string
}

const PLAYER_THEMES: Record<PlayerColor, PlayerTheme> = {
  Red: {
    dot: 'bg-red-500',
    soft: 'bg-red-500/10',
    border: 'border-red-400/40',
    badge: 'badge-error',
    avatar: 'bg-red-500 text-white',
  },
  Green: {
    dot: 'bg-green-500',
    soft: 'bg-green-500/10',
    border: 'border-green-400/40',
    badge: 'badge-success',
    avatar: 'bg-green-500 text-white',
  },
  Yellow: {
    dot: 'bg-yellow-400',
    soft: 'bg-yellow-400/10',
    border: 'border-yellow-400/50',
    badge: 'badge-warning',
    avatar: 'bg-yellow-400 text-yellow-950',
  },
  Blue: {
    dot: 'bg-blue-500',
    soft: 'bg-blue-500/10',
    border: 'border-blue-400/40',
    badge: 'badge-info',
    avatar: 'bg-blue-500 text-white',
  },
}

const FALLBACK_THEME: PlayerTheme = {
  dot: 'bg-base-300',
  soft: 'bg-base-200',
  border: 'border-base-300',
  badge: 'badge-neutral',
  avatar: 'bg-base-300 text-base-content',
}

export function getPlayerTheme(
  color: PlayerColor | null | undefined,
): PlayerTheme {
  return color ? PLAYER_THEMES[color] : FALLBACK_THEME
}
