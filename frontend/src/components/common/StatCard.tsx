interface StatCardProps {
  label: string
  value: string
  detail?: string
}

function StatCard({ label, value, detail }: StatCardProps) {
  return (
    <article className="flex min-h-[150px] flex-col justify-between rounded-2xl border border-amber-200/10 bg-black/25 p-5 shadow-[0_0_28px_rgba(0,0,0,0.18)]">
      <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/65">
        {label}
      </p>
      <div className="mt-4">
        <p className="break-words text-3xl font-black leading-tight text-stone-50">
          {value}
        </p>
        {detail && (
          <p className="mt-2 text-sm leading-5 text-stone-400">{detail}</p>
        )}
      </div>
    </article>
  )
}

export default StatCard
