import './DiceRoll.css'

type Props = {
  value: number
}

const PIP_LAYOUTS: Record<number, number[]> = {
  1: [4],
  2: [0, 8],
  3: [0, 4, 8],
  4: [0, 2, 6, 8],
  5: [0, 2, 4, 6, 8],
  6: [0, 2, 3, 5, 6, 8],
}

export default function DiceRoll({ value }: Props) {
  const visiblePips = new Set(PIP_LAYOUTS[value] ?? [])

  return (
    <div className="card border border-success/30 bg-success/10 shadow-sm">
      <div className="card-body flex-row items-center gap-4 p-4">
        <div
          key={value}
          className="dice-face"
          role="img"
          aria-label={`Dice showing ${value}`}
        >
          {Array.from({ length: 9 }, (_, index) => (
            <span
              key={index}
              className={
                visiblePips.has(index)
                  ? 'dice-pip dice-pip-visible'
                  : 'dice-pip'
              }
            />
          ))}
        </div>

        <div className="min-w-0">
          <p className="text-xs font-bold uppercase tracking-[0.18em] text-success">
            Last roll
          </p>
          <p className="text-3xl font-black leading-none">{value}</p>
          <p className="mt-1 text-sm text-base-content/60">
            The dice shows {value}.
          </p>
        </div>
      </div>
    </div>
  )
}
