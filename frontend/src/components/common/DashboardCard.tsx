interface DashboardCardProps {
  eyebrow?: string
  title: string
  value?: string
  description: string
  actionLabel?: string
}

function DashboardCard({
  eyebrow,
  title,
  value,
  description,
  actionLabel,
}: DashboardCardProps) {
  return (
    <article className="rounded-2xl border border-amber-200/10 bg-white/[0.035] p-5 shadow-[0_0_35px_rgba(0,0,0,0.22)] backdrop-blur transition-all duration-300 hover:border-amber-200/20 hover:bg-white/[0.05]">
      {eyebrow && (
        <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/65">
          {eyebrow}
        </p>
      )}
      <div className="mt-3 flex items-start justify-between gap-4">
        <h3 className="text-lg font-bold text-stone-50">{title}</h3>
        {value && (
          <span className="rounded-full border border-blue-200/20 bg-blue-950/40 px-3 py-1 text-xs font-semibold text-blue-100">
            {value}
          </span>
        )}
      </div>
      <p className="mt-3 text-sm leading-6 text-stone-400">{description}</p>
      {actionLabel && (
        <button
          type="button"
          className="mt-5 text-sm font-semibold text-amber-100 transition hover:text-[#d6b56c]"
        >
          {actionLabel}
        </button>
      )}
    </article>
  )
}

export default DashboardCard
