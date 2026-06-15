import type { DateOption } from './bookingTypes'

interface DateSelectionProps {
  dates: DateOption[]
  selectedDate: string
  onSelect: (date: string) => void
}

function DateSelection({ dates, selectedDate, onSelect }: DateSelectionProps) {
  return (
    <section className="min-w-0 rounded-[28px] border border-amber-200/10 bg-white/[0.035] p-5 backdrop-blur">
      <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
        Korak 3
      </p>
      <h2 className="mt-3 text-2xl font-black text-stone-50">
        Odaberite datum
      </h2>

      <div className="app-scrollbar mt-5 flex min-w-0 gap-3 overflow-x-auto pb-2">
        {dates.map((date) => {
          const isSelected = selectedDate === date.value

          return (
            <button
              key={date.value}
              type="button"
              onClick={() => onSelect(date.value)}
              className={`min-w-28 rounded-2xl border px-4 py-4 text-center transition-all duration-300 ${
                isSelected
                  ? 'border-amber-200/50 bg-amber-100/10 text-amber-100'
                  : 'border-amber-200/10 bg-black/25 text-stone-300 hover:border-amber-200/30'
              }`}
            >
              <span className="block text-xs font-semibold uppercase tracking-[0.18em]">
                {date.day}
              </span>
              <span className="mt-2 block text-lg font-black">
                {date.label}
              </span>
            </button>
          )
        })}
      </div>
    </section>
  )
}

export default DateSelection
