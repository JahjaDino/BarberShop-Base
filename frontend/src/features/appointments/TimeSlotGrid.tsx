import type { TimeSlot } from './bookingTypes'

interface TimeSlotGridProps {
  slots: TimeSlot[]
  selectedTime: string
  onSelect: (time: string) => void
}

function TimeSlotGrid({ slots, selectedTime, onSelect }: TimeSlotGridProps) {
  return (
    <section className="min-w-0 rounded-[28px] border border-amber-200/10 bg-white/[0.035] p-5 backdrop-blur">
      <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
        Korak 4
      </p>
      <h2 className="mt-3 text-2xl font-black text-stone-50">
        Odaberite termin
      </h2>

      {slots.length === 0 && (
        <div className="mt-5 rounded-2xl border border-amber-200/10 bg-black/20 p-4 text-sm leading-6 text-stone-400">
          Odaberite uslugu, frizera i datum kako biste vidjeli slobodne termine.
        </div>
      )}

      <div className="mt-5 grid min-w-0 grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4 2xl:grid-cols-5">
        {slots.map((slot) => {
          const isBusy = slot.status === 'busy'
          const isSelected = selectedTime === slot.value

          return (
            <button
              key={slot.value}
              type="button"
              disabled={isBusy}
              onClick={() => onSelect(slot.value)}
              className={`rounded-2xl border px-4 py-3 text-sm font-bold transition-all duration-300 ${
                isBusy
                  ? 'cursor-not-allowed border-stone-700 bg-stone-900/60 text-stone-600'
                  : isSelected
                    ? 'border-amber-200/50 bg-[linear-gradient(135deg,rgba(120,84,28,0.92),rgba(214,181,108,0.72))] text-amber-50'
                    : 'border-emerald-300/15 bg-emerald-950/30 text-emerald-100 hover:border-amber-200/40 hover:bg-amber-100/10 hover:text-amber-100'
              }`}
            >
              <span className="block">{slot.value}</span>
              <span className="mt-1 block text-xs font-medium">
                {isBusy ? 'Zauzeto' : 'Slobodno'}
              </span>
            </button>
          )
        })}
      </div>
    </section>
  )
}

export default TimeSlotGrid
