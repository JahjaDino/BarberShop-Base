import type { BarberOption } from './bookingTypes'

interface BarberSelectionProps {
  barbers: BarberOption[]
  selectedBarberId: string
  onSelect: (barberId: string) => void
}

function BarberSelection({
  barbers,
  selectedBarberId,
  onSelect,
}: BarberSelectionProps) {
  return (
    <section className="min-w-0 rounded-[28px] border border-amber-200/10 bg-white/[0.035] p-5 backdrop-blur">
      <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
        Korak 2
      </p>
      <h2 className="mt-3 text-2xl font-black text-stone-50">
        Odaberite frizera
      </h2>

      {barbers.length === 0 ? (
        <div className="mt-5 rounded-2xl border border-amber-200/10 bg-black/25 p-4 text-sm text-stone-300">
          Trenutno nema dostupnih frizera za ovaj salon.
        </div>
      ) : (
        <div className="mt-5 grid min-w-0 gap-3 md:grid-cols-3">
          {barbers.map((barber) => {
            const isSelected = selectedBarberId === barber.id

            return (
              <button
                key={barber.id}
                type="button"
                onClick={() => onSelect(barber.id)}
                className={`rounded-2xl border p-4 text-left transition-all duration-300 ${
                  isSelected
                    ? 'border-amber-200/50 bg-amber-100/10 text-stone-50'
                    : 'border-amber-200/10 bg-black/25 text-stone-300 hover:border-amber-200/30 hover:bg-black/35'
                }`}
              >
                <span className="grid h-11 w-11 place-items-center rounded-full border border-amber-200/35 bg-amber-100/15 text-sm font-black text-amber-100">
                  {barber.name
                    .split(' ')
                    .map((part) => part[0])
                    .join('')}
                </span>
                <span className="mt-4 block text-base font-bold">
                  {barber.name}
                </span>
                <span className="mt-2 block text-sm text-stone-400">
                  {barber.specialty}
                </span>
              </button>
            )
          })}
        </div>
      )}
    </section>
  )
}

export default BarberSelection
