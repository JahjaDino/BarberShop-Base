import { buttonStyles } from '../../components/common/buttonStyles'
import type {
  BarberOption,
  BookingServiceOption,
  DateOption,
  PaymentMethod,
} from './bookingTypes'

interface BookingSummaryProps {
  service?: BookingServiceOption
  barber?: BarberOption
  date?: DateOption
  time: string
  paymentMethod?: PaymentMethod
  isPrepared: boolean
  isSubmitting: boolean
  message: string
  error: string
  onConfirm: () => void
}

function BookingSummary({
  service,
  barber,
  date,
  time,
  paymentMethod,
  isPrepared,
  isSubmitting,
  message,
  error,
  onConfirm,
}: BookingSummaryProps) {
  const isComplete = Boolean(service && barber && date && time && paymentMethod)
  const paymentMethodLabel = paymentMethod ? 'Plaćanje u salonu' : 'Nije odabrano'

  return (
    <section className="min-w-0 rounded-[24px] border border-amber-200/15 bg-black/30 p-4 shadow-[0_0_36px_rgba(0,0,0,0.24)] backdrop-blur-xl sm:rounded-[28px] sm:p-5 lg:p-6">
      <div className="flex min-w-0 flex-col gap-3 md:flex-row md:items-end md:justify-between">
        <div className="min-w-0">
          <p className="break-words text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/70 sm:tracking-[0.28em]">
            Korak 5
          </p>
          <h2 className="mt-3 break-words text-2xl font-black text-stone-50">
            Sažetak rezervacije
          </h2>
        </div>
        <button
          type="button"
          disabled={!isComplete || isSubmitting}
          onClick={onConfirm}
          className={`${buttonStyles.primary} disabled:cursor-not-allowed disabled:opacity-50`}
        >
          {isSubmitting ? 'Slanje zahtjeva...' : 'Potvrdi termin'}
        </button>
      </div>

      <dl className="mt-6 grid gap-3 sm:grid-cols-2 xl:grid-cols-6">
        {[
          ['Usluga', service?.name ?? 'Nije odabrano'],
          ['Frizer', barber?.name ?? 'Nije odabrano'],
          ['Datum', date ? `${date.day}, ${date.label}` : 'Nije odabrano'],
          ['Vrijeme', time || 'Nije odabrano'],
          ['Cijena', service?.price ?? 'Nije odabrano'],
          ['Način plaćanja', paymentMethodLabel],
        ].map(([label, value]) => (
          <div
            key={label}
            className="min-w-0 rounded-2xl border border-amber-200/10 bg-white/[0.035] p-4"
          >
            <dt className="break-words text-xs font-semibold uppercase tracking-[0.16em] text-stone-500 sm:tracking-[0.18em]">
              {label}
            </dt>
            <dd className="mt-2 break-words font-semibold text-stone-100">{value}</dd>
          </div>
        ))}
      </dl>

      {error && (
        <div className="mt-5 rounded-2xl border border-red-300/20 bg-red-400/10 p-4">
          <p className="font-bold text-red-100">{error}</p>
        </div>
      )}

      {isPrepared && message && (
        <div className="mt-5 rounded-2xl border border-emerald-300/20 bg-emerald-300/10 p-4">
          <p className="font-bold text-emerald-100">{message}</p>
        </div>
      )}
    </section>
  )
}

export default BookingSummary
