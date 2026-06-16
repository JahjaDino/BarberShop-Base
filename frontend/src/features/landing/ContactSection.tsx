const contacts = [
  { icon: 'T', label: 'Telefon', value: '+387 61 123 456' },
  { icon: '@', label: 'Email', value: 'info@hairstudiomimi.ba' },
  { icon: 'L', label: 'Lokacija', value: 'Bihac, Bosna i Hercegovina' },
  {
    icon: 'H',
    label: 'Radno vrijeme',
    value: 'Pon - Pet 08:00 - 20:00, Subota 09:00 - 18:00',
  },
]

function ContactSection() {
  const handleLoginClick = () => {
    document.getElementById('home')?.scrollIntoView({
      behavior: 'smooth',
      block: 'start',
    })

    window.setTimeout(() => {
      window.dispatchEvent(new Event('highlight-login-card'))
    }, 450)
  }

  return (
    <section id="contact" className="border-t border-amber-200/10 py-16">
      <div className="mx-auto max-w-[1500px] px-5 sm:px-8 lg:px-14 xl:px-20">
        <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
          Kontakt
        </p>

        <h2 className="mt-4 max-w-3xl text-4xl font-black leading-tight text-stone-50 sm:text-5xl">
          Rezervacija pocinje jednim klikom.
        </h2>

        <p className="mt-4 max-w-2xl text-base leading-7 text-stone-300">
          Javite se Hair Studio MIMI timu za informacije o uslugama ili se
          prijavite u aplikaciju i upravljajte terminima na jednom mjestu.
        </p>

        <div className="mt-10 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {contacts
            .filter((contact) => contact.label !== 'Radno vrijeme')
            .map((contact) => (
              <article
                key={contact.label}
                className="flex min-h-48 flex-col justify-between rounded-2xl border border-amber-200/10 bg-white/[0.03] p-5 backdrop-blur transition-all duration-300 hover:-translate-y-1 hover:border-amber-200/25 hover:bg-white/[0.05]"
              >
                <div className="flex h-12 w-12 items-center justify-center rounded-2xl border border-amber-200/15 bg-amber-200/10 text-xl font-black text-amber-200">
                  {contact.icon}
                </div>

                <div className="mt-6">
                  <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-200/70">
                    {contact.label}
                  </p>
                  <p className="mt-3 text-lg font-semibold leading-7 text-white">
                    {contact.value}
                  </p>
                </div>
              </article>
            ))}
        </div>

        <div className="mt-4 rounded-[28px] border border-amber-200/10 bg-blue-950/35 p-6 backdrop-blur">
          <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-200/70">
            Mapa lokacije
          </p>
          <div className="mt-5 grid h-56 place-items-center rounded-2xl border border-amber-200/10 bg-black/35 text-sm font-semibold uppercase tracking-[0.24em] text-stone-400">
            Bihac centar
          </div>
        </div>

        <div className="mt-4 flex flex-col gap-5 rounded-[28px] border border-amber-200/10 bg-white/[0.03] p-6 backdrop-blur-xl md:flex-row md:items-center md:justify-between">
          <div>
            <h3 className="text-2xl font-black leading-tight text-stone-50">
              Spremni za novi termin?
            </h3>
            <p className="mt-2 max-w-2xl text-sm leading-7 text-stone-300">
              Prijavite se ili kreirajte nalog i upravljajte svojim terminima
              kroz aplikaciju.
            </p>
          </div>

          <button
            type="button"
            onClick={handleLoginClick}
            className="inline-flex shrink-0 justify-center rounded-2xl bg-amber-200 px-6 py-4 text-sm font-bold uppercase tracking-[0.2em] text-blue-950 transition-all duration-300 hover:-translate-y-0.5 hover:bg-[#d6b56c]"
          >
            Prijavi se
          </button>
        </div>

        <div className="mt-4 rounded-[28px] border border-amber-200/20 bg-blue-950/60 p-6 shadow-[0_0_35px_rgba(30,64,175,0.20)] backdrop-blur-xl">
          <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-200/70">
            Radno vrijeme
          </p>

          <div className="mt-4 grid gap-3 text-sm text-stone-300 sm:grid-cols-3">
            <div className="flex justify-between gap-4 rounded-2xl border border-amber-200/10 bg-black/20 px-4 py-3 sm:block">
              <span className="block">Pon - Pet</span>
              <span className="block font-semibold text-stone-100 sm:mt-2">
                08:00 - 20:00
              </span>
            </div>
            <div className="flex justify-between gap-4 rounded-2xl border border-amber-200/10 bg-black/20 px-4 py-3 sm:block">
              <span className="block">Subota</span>
              <span className="block font-semibold text-stone-100 sm:mt-2">
                09:00 - 18:00
              </span>
            </div>
            <div className="flex justify-between gap-4 rounded-2xl border border-amber-200/10 bg-black/20 px-4 py-3 sm:block">
              <span className="block">Nedjelja</span>
              <span className="block font-semibold text-stone-100 sm:mt-2">
                Zatvoreno
              </span>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}

export default ContactSection
