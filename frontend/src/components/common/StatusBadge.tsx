type StatusTone = 'success' | 'warning' | 'neutral' | 'danger'

interface StatusBadgeProps {
  label: string
  tone?: StatusTone
}

const toneClasses: Record<StatusTone, string> = {
  success: 'border-emerald-300/20 bg-emerald-300/10 text-emerald-100',
  warning: 'border-amber-200/25 bg-amber-100/10 text-amber-100',
  neutral: 'border-stone-400/15 bg-stone-400/10 text-stone-200',
  danger: 'border-red-300/20 bg-red-400/10 text-red-200',
}

function StatusBadge({ label, tone = 'neutral' }: StatusBadgeProps) {
  return (
    <span
      className={`inline-flex max-w-full break-words rounded-full border px-3 py-1 text-xs font-semibold leading-snug ${toneClasses[tone]}`}
    >
      {label}
    </span>
  )
}

export default StatusBadge
