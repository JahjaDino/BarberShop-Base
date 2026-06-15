import { Link } from 'react-router-dom'
import { buttonStyles } from './buttonStyles'

interface EmptyStateProps {
  title: string
  description?: string
  actionLabel?: string
  actionTo?: string
}

function EmptyState({
  title,
  description,
  actionLabel,
  actionTo,
}: EmptyStateProps) {
  return (
    <div className="rounded-2xl border border-amber-200/10 bg-black/25 p-6 text-center">
      <h3 className="text-lg font-bold text-stone-50">{title}</h3>
      {description && (
        <p className="mx-auto mt-2 max-w-xl text-sm leading-6 text-stone-400">
          {description}
        </p>
      )}
      {actionLabel && actionTo && (
        <Link to={actionTo} className={`mt-5 ${buttonStyles.secondary}`}>
          {actionLabel}
        </Link>
      )}
    </div>
  )
}

export default EmptyState
