interface PageHeaderProps {
  eyebrow?: string
  title: string
  subtitle: string
}

function PageHeader({ eyebrow = 'Classic Cuts', title, subtitle }: PageHeaderProps) {
  return (
    <section className="rounded-[32px] border border-amber-200/15 bg-black/25 p-6 shadow-[0_0_45px_rgba(0,0,0,0.28)] backdrop-blur-xl lg:p-8">
      <p className="text-xs font-semibold uppercase tracking-[0.32em] text-amber-200/75">
        {eyebrow}
      </p>
      <h2 className="mt-4 max-w-4xl text-4xl font-black leading-tight text-stone-50 sm:text-5xl">
        {title}
      </h2>
      <p className="mt-5 max-w-3xl text-base leading-8 text-stone-300">
        {subtitle}
      </p>
    </section>
  )
}

export default PageHeader
