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
    <section className="rounded-[32px] border border-amber-200/15 bg-black/25 p-6 shadow-[0_0_45px_rgba(0,0,0,0.25)] backdrop-blur-xl lg:p-8">
      <p className="text-xs font-semibold uppercase tracking-[0.32em] text-amber-200/75">
        {label}
      </p>
      <h2 className="mt-4 max-w-4xl text-4xl font-black leading-tight text-stone-50 sm:text-5xl">
        {title}
      </h2>
      <p className="mt-5 max-w-3xl text-base leading-8 text-stone-300">
        {description}
      </p>

      {items.length > 0 && (
        <div className="mt-8 grid gap-3 md:grid-cols-2 xl:grid-cols-3">
          {items.map((item) => (
            <div
              key={item}
              className="rounded-2xl border border-amber-200/10 bg-white/[0.035] p-4 text-sm text-stone-300"
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
