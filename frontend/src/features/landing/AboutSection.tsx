const highlights = [
  {
    icon: '*',
    title: 'Iskusan tim frizerki',
    description:
      'Cetiri zaposlenice posvecene detaljima, njezi i stilu svake klijentice.',
  },
  {
    icon: '+',
    title: 'Brzo zakazivanje',
    description:
      'Rezervisite termin u nekoliko klikova nakon prijave u aplikaciju.',
  },
  {
    icon: '!',
    title: 'Podsjetnici na termin',
    description:
      'Aplikacija vas podsjeca na rezervaciju kako biste lakse organizovali dan.',
  },
  {
    icon: 'M',
    title: 'Moderne usluge za kosu',
    description:
      'Zenske i muske frizure, farbanje, pramenovi i profesionalna njega kose.',
  },
  {
    icon: '#',
    title: 'Kvalitetni proizvodi',
    description:
      'Radimo sa pazljivo odabranim proizvodima za njegovan i postojan rezultat.',
  },
  {
    icon: '~',
    title: 'Prijatna atmosfera',
    description:
      'Ugodan ambijent, pazljiva usluga i osjecaj da ste na pravom mjestu.',
  },
]

function AboutSection() {
  return (
    <section
      id="about"
      className="border-t border-amber-200/10 bg-transparent pt-12 pb-16 sm:pb-20"
    >
      <div className="mx-auto max-w-[1500px] px-5 sm:px-8 lg:px-14 xl:px-20">
        <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_390px]">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
              O nama
            </p>

            <h2 className="mt-3 max-w-4xl text-3xl font-black leading-tight text-stone-50 sm:text-4xl">
              Hair Studio MIMI je mjesto gdje njega kose postaje iskustvo.
            </h2>

            <p className="mt-4 max-w-3xl text-base leading-7 text-stone-300">
              Hair Studio MIMI je moderan hair studio za klijentice i klijente
              koji zele profesionalnu njegu, kvalitetne proizvode i jednostavan
              sistem rezervacije. MIMI je vlasnica i glavna frizerka, uz tim
              od 4 frizerke koje svakom terminu pristupaju pazljivo i licno.
            </p>
          </div>

          <aside className="rounded-[32px] border border-amber-200/10 bg-white/[0.03] p-5 shadow-[0_0_35px_rgba(0,0,0,0.24)] backdrop-blur">
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
              Nas cilj
            </p>
            <h3 className="mt-3 text-xl font-black leading-tight text-stone-50">
              Individualan pristup. Profesionalna njega. Vrhunski izgled.
            </h3>
            <p className="mt-3 text-sm leading-6 text-stone-300">
              Zelimo da svaka klijentica zna kada dolazi, kome dolazi i da iz
              salona izlazi sa frizurom koja izgleda njegovano, moderno i
              prirodno.
            </p>
          </aside>
        </div>

        <div className="mt-8 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {highlights.map((item) => (
            <article
              key={item.title}
              className="rounded-2xl border border-amber-200/10 bg-white/[0.03] p-5 backdrop-blur transition-all duration-300 hover:border-amber-200/20 hover:bg-white/[0.04]"
            >
              <div className="grid h-10 w-10 place-items-center rounded-full bg-amber-200 text-lg font-black text-blue-950">
                {item.icon}
              </div>

              <h4 className="mt-4 text-lg font-bold text-stone-50">
                {item.title}
              </h4>

              <p className="mt-2 text-sm leading-6 text-stone-400">
                {item.description}
              </p>
            </article>
          ))}
        </div>
      </div>
    </section>
  )
}

export default AboutSection
