interface PageHeaderProps {
  eyebrow?: string
  title: string
  subtitle: string
}

function PageHeader({ eyebrow = 'Classic Cuts', title, subtitle }: PageHeaderProps) {
  return (
    <section className="min-w-0 rounded-[28px] border border-amber-200/15 bg-black/25 p-4 shadow-[0_0_45px_rgba(0,0,0,0.28)] backdrop-blur-xl sm:rounded-[32px] sm:p-6 lg:p-8">
      <p className="break-words text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/75 sm:tracking-[0.32em]">
        {eyebrow}
      </p>
      <h2 className="mt-4 max-w-4xl break-words text-3xl font-black leading-tight text-stone-50 sm:text-4xl lg:text-5xl">
        {title}
      </h2>
      <p className="mt-5 max-w-3xl break-words text-sm leading-7 text-stone-300 sm:text-base sm:leading-8">
        {subtitle}
      </p>
    </section>
  )
}

export default PageHeader
