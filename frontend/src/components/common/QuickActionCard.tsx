import { Link } from 'react-router-dom'
import { buttonStyles } from './buttonStyles'

interface QuickActionCardProps {
  title: string
  description: string
  to: string
  label: string
}

function QuickActionCard({ title, description, to, label }: QuickActionCardProps) {
  return (
    <section className="rounded-[28px] border border-amber-200/15 bg-amber-100/10 p-6 shadow-[0_0_45px_rgba(245,213,145,0.08)]">
      <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/75">
        Quick action
      </p>
      <h2 className="mt-4 text-2xl font-black leading-tight text-stone-50">
        {title}
      </h2>
      <p className="mt-3 max-w-2xl text-sm leading-7 text-stone-300">
        {description}
      </p>
      <Link
        to={to}
        className={`mt-6 ${buttonStyles.primary}`}
      >
        {label}
      </Link>
    </section>
  )
}

export default QuickActionCard
