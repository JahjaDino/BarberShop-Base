interface StatCardProps {
  label: string
  value: string
  detail?: string
}

function StatCard({ label, value, detail }: StatCardProps) {
  return (
    <article className="flex min-h-[130px] min-w-0 flex-col justify-between rounded-2xl border border-amber-200/10 bg-black/25 p-4 shadow-[0_0_28px_rgba(0,0,0,0.18)] sm:min-h-[150px] sm:p-5">
      <p className="break-words text-xs font-semibold uppercase tracking-[0.18em] text-amber-200/65 sm:tracking-[0.22em]">
        {label}
      </p>
      <div className="mt-4">
        <p className="break-words text-3xl font-black leading-tight text-stone-50">
          {value}
        </p>
        {detail && (
          <p className="mt-2 break-words text-sm leading-5 text-stone-400">{detail}</p>
        )}
      </div>
    </article>
  )
}

export default StatCard
