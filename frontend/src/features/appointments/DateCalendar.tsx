import { useEffect, useMemo, useState } from 'react'
import type { DateOption } from './bookingTypes'

interface DateCalendarProps {
  selectedDate: string
  onSelect: (date: DateOption) => void
}

const monthNames = [
  'Januar',
  'Februar',
  'Mart',
  'April',
  'Maj',
  'Juni',
  'Juli',
  'August',
  'Septembar',
  'Oktobar',
  'Novembar',
  'Decembar',
]

const dayNames = ['Pon', 'Uto', 'Sri', 'Čet', 'Pet', 'Sub', 'Ned']

function startOfDay(date: Date) {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate())
}

function toDateValue(date: Date) {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')

  return `${year}-${month}-${day}`
}

function formatDateOption(date: Date): DateOption {
  return {
    value: toDateValue(date),
    day: dayNames[(date.getDay() + 6) % 7],
    label: `${String(date.getDate()).padStart(2, '0')}. ${monthNames[date.getMonth()]}`,
  }
}

function getMonthGridDays(monthDate: Date) {
  const firstDay = new Date(monthDate.getFullYear(), monthDate.getMonth(), 1)
  const firstGridOffset = (firstDay.getDay() + 6) % 7
  const gridStart = new Date(firstDay)
  gridStart.setDate(firstDay.getDate() - firstGridOffset)

  return Array.from({ length: 42 }, (_, index) => {
    const date = new Date(gridStart)
    date.setDate(gridStart.getDate() + index)
    return date
  })
}

function DateCalendar({ selectedDate, onSelect }: DateCalendarProps) {
  const today = useMemo(() => startOfDay(new Date()), [])
  const maxDate = useMemo(() => {
    const date = new Date(today)
    date.setDate(today.getDate() + 30)
    return date
  }, [today])

  const selectedDateObject = useMemo(
    () =>
      selectedDate
        ? startOfDay(new Date(`${selectedDate}T00:00:00`))
        : today,
    [selectedDate, today],
  )

  const [displayMonth, setDisplayMonth] = useState(
    new Date(selectedDateObject.getFullYear(), selectedDateObject.getMonth(), 1),
  )

  useEffect(() => {
    setDisplayMonth(
      new Date(selectedDateObject.getFullYear(), selectedDateObject.getMonth(), 1),
    )
  }, [selectedDateObject])

  const previousMonth = new Date(displayMonth)
  previousMonth.setMonth(displayMonth.getMonth() - 1)
  const nextMonth = new Date(displayMonth)
  nextMonth.setMonth(displayMonth.getMonth() + 1)

  const canGoPrevious =
    new Date(displayMonth.getFullYear(), displayMonth.getMonth(), 0) >= today
  const canGoNext =
    new Date(nextMonth.getFullYear(), nextMonth.getMonth(), 1) <= maxDate

  const gridDays = getMonthGridDays(displayMonth)

  return (
    <section className="min-w-0 rounded-[24px] border border-amber-200/10 bg-white/[0.035] p-4 backdrop-blur">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.24em] text-amber-200/70">
            Korak 3
          </p>
          <h2 className="mt-2 text-xl font-black text-stone-50">
            Odaberite datum
          </h2>
        </div>

        <div className="flex items-center gap-2">
          <button
            type="button"
            disabled={!canGoPrevious}
            onClick={() => setDisplayMonth(previousMonth)}
            className="inline-flex h-8 w-8 items-center justify-center rounded-full border border-amber-200/15 bg-black/25 text-base font-black text-amber-100 transition hover:border-amber-200/30 hover:bg-amber-100/10 disabled:cursor-not-allowed disabled:opacity-35"
            aria-label="Prethodni mjesec"
          >
            ‹
          </button>
          <div className="min-w-32 text-center">
            <p className="text-sm font-black text-stone-50">
              {monthNames[displayMonth.getMonth()]} {displayMonth.getFullYear()}
            </p>
          </div>
          <button
            type="button"
            disabled={!canGoNext}
            onClick={() => setDisplayMonth(nextMonth)}
            className="inline-flex h-8 w-8 items-center justify-center rounded-full border border-amber-200/15 bg-black/25 text-base font-black text-amber-100 transition hover:border-amber-200/30 hover:bg-amber-100/10 disabled:cursor-not-allowed disabled:opacity-35"
            aria-label="Sljedeći mjesec"
          >
            ›
          </button>
        </div>
      </div>

      <div className="mt-4 grid grid-cols-7 gap-1.5 sm:gap-2">
        {dayNames.map((day) => (
          <div
            key={day}
            className="py-1 text-center text-[10px] font-bold uppercase tracking-[0.12em] text-stone-500"
          >
            {day}
          </div>
        ))}

        {gridDays.map((date) => {
          const dateValue = toDateValue(date)
          const isCurrentMonth = date.getMonth() === displayMonth.getMonth()
          const isToday = dateValue === toDateValue(today)
          const isSelected = selectedDate === dateValue
          const isInRange = date >= today && date <= maxDate
          const isDisabled = !isCurrentMonth || !isInRange

          return (
            <button
              key={dateValue}
              type="button"
              disabled={isDisabled}
              onClick={() => onSelect(formatDateOption(date))}
              className={`flex h-8 items-center justify-center rounded-xl border text-xs font-bold transition-all duration-200 sm:h-9 ${
                isSelected
                  ? 'border-amber-200/55 bg-amber-100/15 text-amber-50 shadow-[0_0_18px_rgba(214,181,108,0.10)]'
                  : isDisabled
                    ? 'cursor-not-allowed border-transparent bg-transparent text-stone-700'
                    : isToday
                      ? 'border-amber-200/35 bg-black/25 text-amber-100 hover:bg-amber-100/10'
                      : 'border-amber-200/10 bg-black/20 text-stone-300 hover:border-amber-200/25 hover:bg-amber-100/5 hover:text-amber-100'
              }`}
            >
              {date.getDate()}
            </button>
          )
        })}
      </div>

      <p className="mt-3 text-xs leading-5 text-stone-500">
        Termine je moguće birati od danas do 30 dana unaprijed.
      </p>
    </section>
  )
}

export default DateCalendar
