import type { ReactNode } from 'react'

export type AppBadgeTone =
  | 'pending'
  | 'confirmed'
  | 'cancelled'
  | 'completed'
  | 'noShow'
  | 'active'
  | 'inactive'
  | 'neutral'

interface AppBadgeProps {
  children: ReactNode
  className?: string
  tone?: AppBadgeTone
}

const toneClasses: Record<AppBadgeTone, string> = {
  pending: 'border-amber-200/25 bg-amber-100/10 text-amber-100',
  confirmed: 'border-emerald-300/20 bg-emerald-300/10 text-emerald-100',
  cancelled: 'border-red-300/20 bg-red-400/10 text-red-200',
  completed: 'border-emerald-300/20 bg-emerald-300/10 text-emerald-100',
  noShow: 'border-red-300/20 bg-red-400/10 text-red-200',
  active: 'border-emerald-300/20 bg-emerald-300/10 text-emerald-100',
  inactive: 'border-stone-400/15 bg-stone-400/10 text-stone-200',
  neutral: 'border-stone-400/15 bg-stone-400/10 text-stone-200',
}

function AppBadge({ children, className = '', tone = 'neutral' }: AppBadgeProps) {
  return (
    <span
      className={`inline-flex w-fit items-center rounded-full border px-3 py-1 text-xs font-semibold ${toneClasses[tone]} ${className}`}
    >
      {children}
    </span>
  )
}

export default AppBadge
