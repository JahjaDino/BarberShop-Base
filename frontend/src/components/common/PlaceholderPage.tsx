interface PlaceholderPageProps {
  label: string
  title: string
  description: string
  items?: string[]
}

function PlaceholderPage({
  label,
  title,
  description,
  items = [],
}: PlaceholderPageProps) {
  return (
    <section className="min-w-0 rounded-[28px] border border-amber-200/15 bg-black/25 p-4 shadow-[0_0_45px_rgba(0,0,0,0.25)] backdrop-blur-xl sm:rounded-[32px] sm:p-6 lg:p-8">
      <p className="break-words text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/75 sm:tracking-[0.32em]">
        {label}
      </p>
      <h2 className="mt-4 max-w-4xl break-words text-3xl font-black leading-tight text-stone-50 sm:text-4xl lg:text-5xl">
        {title}
      </h2>
      <p className="mt-5 max-w-3xl break-words text-sm leading-7 text-stone-300 sm:text-base sm:leading-8">
        {description}
      </p>

      {items.length > 0 && (
        <div className="mt-8 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
          {items.map((item) => (
            <div
              key={item}
              className="min-w-0 break-words rounded-2xl border border-amber-200/10 bg-white/[0.035] p-4 text-sm text-stone-300"
            >
              {item}
            </div>
          ))}
        </div>
      )}
    </section>
  )
}

export default PlaceholderPage
