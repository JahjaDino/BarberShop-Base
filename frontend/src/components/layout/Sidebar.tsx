import { useEffect, useState } from 'react'

type SectionId = 'home' | 'about' | 'gallery' | 'contact'

const links: Array<{ label: string; section: SectionId }> = [
  { label: 'Početna', section: 'home' },
  { label: 'O nama', section: 'about' },
  { label: 'Galerija', section: 'gallery' },
  { label: 'Kontakt', section: 'contact' },
]

function Sidebar() {
  const [activeSection, setActiveSection] = useState<SectionId>('home')
  const [isMenuOpen, setIsMenuOpen] = useState(false)

  const updateActiveSection = () => {
    const scrollPosition = window.scrollY + 160
    let currentSection: SectionId = 'home'

    links.forEach((link) => {
      const section = document.getElementById(link.section)

      if (section && section.offsetTop <= scrollPosition) {
        currentSection = link.section
      }
    })

    setActiveSection(currentSection)
  }

  const scrollToSection = (sectionId: SectionId) => {
    setActiveSection(sectionId)
    setIsMenuOpen(false)
    document.getElementById(sectionId)?.scrollIntoView({
      behavior: 'smooth',
      block: 'start',
    })
    window.history.pushState(null, '', `#${sectionId}`)
  }

  useEffect(() => {
    const frameId = window.requestAnimationFrame(updateActiveSection)
    window.addEventListener('scroll', updateActiveSection, { passive: true })
    window.addEventListener('resize', updateActiveSection)

    return () => {
      window.cancelAnimationFrame(frameId)
      window.removeEventListener('scroll', updateActiveSection)
      window.removeEventListener('resize', updateActiveSection)
    }
  }, [])

  return (
    <aside className="sticky top-0 z-40 w-full border-b border-amber-200/10 bg-black/70 backdrop-blur-xl lg:top-0 lg:flex lg:h-screen lg:w-72 lg:flex-col lg:justify-between lg:border-b-0 lg:border-r lg:bg-black/25 lg:p-5">
      <div className="lg:hidden">
        <div className="flex items-center justify-between gap-4 px-4 py-3">
          <button
            type="button"
            onClick={() => scrollToSection('home')}
            className="flex min-w-0 items-center gap-3 text-left"
            aria-label="Hair Studio MIMI početna"
          >
            <span className="grid h-11 w-11 shrink-0 place-items-center rounded-2xl border border-amber-200/25 bg-amber-100/10 text-lg font-black text-amber-100 shadow-[0_0_30px_rgba(245,213,145,0.12)]">
              MM
            </span>
            <span className="min-w-0">
              <span className="block truncate text-base font-semibold tracking-wide text-stone-50">
                Hair Studio MIMI
              </span>
              <span className="block truncate text-[11px] uppercase tracking-[0.18em] text-amber-200/70">
                Hair Studio
              </span>
            </span>
          </button>

          <button
            type="button"
            onClick={() => setIsMenuOpen((isOpen) => !isOpen)}
            className="grid h-11 w-11 shrink-0 place-items-center rounded-2xl border border-amber-200/20 bg-black/25 text-amber-100 transition hover:border-amber-200/35 hover:bg-amber-100/10"
            aria-label={isMenuOpen ? 'Zatvori navigaciju' : 'Otvori navigaciju'}
            aria-expanded={isMenuOpen}
          >
            <span className="grid gap-1.5">
              <span
                className={`block h-0.5 w-5 rounded-full bg-current transition ${
                  isMenuOpen ? 'translate-y-2 rotate-45' : ''
                }`}
              />
              <span
                className={`block h-0.5 w-5 rounded-full bg-current transition ${
                  isMenuOpen ? 'opacity-0' : ''
                }`}
              />
              <span
                className={`block h-0.5 w-5 rounded-full bg-current transition ${
                  isMenuOpen ? '-translate-y-2 -rotate-45' : ''
                }`}
              />
            </span>
          </button>
        </div>

        {isMenuOpen && (
          <nav className="grid gap-2 border-t border-amber-200/10 px-4 py-3">
            {links.map((link) => {
              const isActive = activeSection === link.section

              return (
                <button
                  key={link.section}
                  type="button"
                  onClick={() => scrollToSection(link.section)}
                  className={`rounded-2xl border px-4 py-3 text-left text-sm font-medium transition hover:border-amber-200/40 hover:bg-amber-100/10 hover:text-amber-100 ${
                    isActive
                      ? 'border-amber-200/30 bg-amber-100/10 text-amber-100'
                      : 'border-transparent text-stone-300'
                  }`}
                >
                  {link.label}
                </button>
              )
            })}
          </nav>
        )}
      </div>

      <div className="hidden lg:block">
        <button
          type="button"
          onClick={() => scrollToSection('home')}
          className="flex items-center gap-3 text-left"
        >
          <span className="grid h-12 w-12 place-items-center rounded-2xl border border-amber-200/25 bg-amber-100/10 text-xl font-black text-amber-100 shadow-[0_0_30px_rgba(245,213,145,0.12)]">
            MM
          </span>
          <span>
            <span className="block text-lg font-semibold tracking-wide text-stone-50">
              Hair Studio MIMI
            </span>
            <span className="block text-xs uppercase tracking-[0.28em] text-amber-200/70">
              Hair Studio
            </span>
          </span>
        </button>

        <nav className="mt-8 grid gap-2">
          {links.map((link) => {
            const isActive = activeSection === link.section

            return (
              <button
                key={link.section}
                type="button"
                onClick={() => scrollToSection(link.section)}
                className={`rounded-2xl border px-4 py-3 text-left text-sm font-medium transition hover:border-amber-200/40 hover:bg-amber-100/10 hover:text-amber-100 ${
                  isActive
                    ? 'border-amber-200/30 bg-amber-100/10 text-amber-100'
                    : 'border-transparent text-stone-300'
                }`}
              >
                {link.label}
              </button>
            )
          })}
        </nav>
      </div>

      <div className="mt-8 hidden rounded-2xl border border-amber-200/20 bg-blue-950/60 p-4 shadow-[0_0_35px_rgba(30,64,175,0.20)] lg:block">
        <p className="text-xs uppercase tracking-[0.24em] text-amber-200/70">
          Radno vrijeme
        </p>

        <div className="mt-3 space-y-2 text-sm text-stone-300">
          <div className="flex justify-between gap-4">
            <span>Pon - Pet</span>
            <span>07:00 - 14:00</span>
          </div>
          <div className="flex justify-between gap-4">
            <span>Subota</span>
            <span>08:00 - 12:00</span>
          </div>
          <div className="flex justify-between gap-4">
            <span>Nedjelja</span>
            <span>Zatvoreno</span>
          </div>
        </div>
      </div>
    </aside>
  )
}

export default Sidebar
