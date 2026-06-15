import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  cancelAppointment,
  getClientAppointments,
  type AppointmentStatus,
  type ClientAppointmentCardDto,
} from '../../api/appointmentsApi'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatusBadge from '../../components/common/StatusBadge'
import { buttonStyles } from '../../components/common/buttonStyles'

type AppointmentCategory = 'upcoming' | 'completed' | 'cancelled'

const filters: Array<{ label: string; value: AppointmentCategory }> = [
  { label: 'Nadolazeći', value: 'upcoming' },
  { label: 'Završeni', value: 'completed' },
  { label: 'Otkazani', value: 'cancelled' },
]

function formatPrice(price: number) {
  return `${price.toFixed(price % 1 === 0 ? 0 : 2)} KM`
}

function formatDate(date: string) {
  const [year, month, day] = date.split('-')

  if (!year || !month || !day) {
    return date
  }

  return `${day}.${month}.${year}`
}

function formatTime(time: string) {
  return time.slice(0, 5)
}

function translateStatus(status: AppointmentStatus) {
  const labels: Record<AppointmentStatus, string> = {
    PENDING: 'Na čekanju',
    CONFIRMED: 'Potvrđen',
    COMPLETED: 'Završen',
    CANCELLED: 'Otkazan',
    NO_SHOW: 'Nije se pojavio',
  }

  return labels[status]
}

function getStatusTone(status: AppointmentStatus) {
  if (status === 'CONFIRMED' || status === 'COMPLETED') {
    return 'success'
  }

  if (status === 'PENDING') {
    return 'warning'
  }

  if (status === 'CANCELLED' || status === 'NO_SHOW') {
    return 'danger'
  }

  return 'neutral'
}

function translatePaymentMethod(paymentMethod?: string | null) {
  return paymentMethod ? 'Plaćanje u salonu' : 'Nije dostupno'
}

interface AppointmentCardProps {
  appointment: ClientAppointmentCardDto
  onCancel: (appointmentId: number) => void
  isCancelling: boolean
}

function AppointmentCard({
  appointment,
  onCancel,
  isCancelling,
}: AppointmentCardProps) {
  return (
    <article className="rounded-2xl border border-amber-200/10 bg-black/25 p-5 transition hover:border-amber-200/25 hover:bg-black/35">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h3 className="text-xl font-black text-stone-50">
            {appointment.serviceName || 'Usluga'}
          </h3>
          <p className="mt-2 text-sm text-stone-400">
            Frizer: {appointment.employeeName || 'Nije dostupno'}
          </p>
        </div>
        <StatusBadge
          label={translateStatus(appointment.status)}
          tone={getStatusTone(appointment.status)}
        />
      </div>

      <dl className="mt-5 grid gap-3 text-sm sm:grid-cols-2 xl:grid-cols-4">
        {[
          ['Datum', formatDate(appointment.date)],
          ['Vrijeme', formatTime(appointment.time)],
          ['Trajanje', `${appointment.durationMinutes} min`],
          ['Cijena', formatPrice(appointment.price)],
          ['Način plaćanja', translatePaymentMethod(appointment.paymentMethod)],
        ].map(([label, value]) => (
          <div
            key={label}
            className="rounded-2xl border border-amber-200/10 bg-white/[0.035] p-3"
          >
            <dt className="text-xs uppercase tracking-[0.16em] text-stone-500">
              {label}
            </dt>
            <dd className="mt-1 font-semibold text-stone-100">{value}</dd>
          </div>
        ))}
      </dl>

      {appointment.status === 'PENDING' || appointment.status === 'CONFIRMED' ? (
        <div className="mt-5 flex flex-col gap-3 sm:flex-row">
          <button
            type="button"
            disabled={isCancelling}
            onClick={() => onCancel(appointment.appointmentId)}
            className="inline-flex items-center justify-center rounded-2xl border border-red-300/20 bg-red-400/10 px-5 py-3 text-sm font-bold uppercase tracking-[0.16em] text-red-200 transition hover:border-red-300/30 hover:bg-red-400/15 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isCancelling ? 'Otkazivanje...' : 'Otkaži termin'}
          </button>
        </div>
      ) : null}
    </article>
  )
}

function MyAppointmentsPage() {
  const [activeFilter, setActiveFilter] =
    useState<AppointmentCategory>('upcoming')
  const [appointments, setAppointments] = useState<ClientAppointmentCardDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [cancellingAppointmentId, setCancellingAppointmentId] =
    useState<number | null>(null)

  useEffect(() => {
    let isMounted = true

    async function loadAppointments() {
      setIsLoading(true)
      setError('')

      try {
        const response = await getClientAppointments(activeFilter)

        if (isMounted) {
          setAppointments(response.items)
        }
      } catch (requestError) {
        if (isMounted) {
          setAppointments([])
          setError(
            requestError instanceof Error
              ? requestError.message
              : 'Termini trenutno nisu dostupni.',
          )
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadAppointments()

    return () => {
      isMounted = false
    }
  }, [activeFilter])

  const handleCancelAppointment = async (appointmentId: number) => {
    setCancellingAppointmentId(appointmentId)
    setError('')

    try {
      await cancelAppointment(appointmentId, 'Termin je otkazan od klijenta.')
      const response = await getClientAppointments(activeFilter)
      setAppointments(response.items)
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Termin nije moguće otkazati.',
      )
    } finally {
      setCancellingAppointmentId(null)
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader
        title="Moji termini"
        subtitle="Pregledajte nadolazeće, završene i otkazane termine."
      />

      <SectionCard>
        <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
              Filter termina
            </p>
            <h2 className="mt-3 text-2xl font-black text-stone-50">
              {filters.find((filter) => filter.value === activeFilter)?.label}{' '}
              termini
            </h2>
          </div>
          <div className="flex flex-wrap gap-2">
            {filters.map((filter) => (
              <button
                key={filter.value}
                type="button"
                onClick={() => setActiveFilter(filter.value)}
                className={`rounded-2xl border px-4 py-2.5 text-sm font-semibold transition ${
                  activeFilter === filter.value
                    ? 'border-amber-200/40 bg-amber-100/10 text-amber-100'
                    : 'border-amber-200/10 bg-black/25 text-stone-300 hover:border-amber-200/25 hover:text-amber-100'
                }`}
              >
                {filter.label}
              </button>
            ))}
          </div>
        </div>

        {error && (
          <div className="mt-5 rounded-2xl border border-red-300/20 bg-red-400/10 p-4 text-sm font-semibold text-red-100">
            {error}
          </div>
        )}

        <div className="mt-6 grid gap-4">
          {isLoading ? (
            <div className="rounded-2xl border border-amber-200/10 bg-white/[0.035] p-5 text-sm text-stone-300">
              Učitavanje termina...
            </div>
          ) : appointments.length > 0 ? (
            appointments.map((appointment) => (
              <AppointmentCard
                key={appointment.appointmentId}
                appointment={appointment}
                onCancel={handleCancelAppointment}
                isCancelling={
                  cancellingAppointmentId === appointment.appointmentId
                }
              />
            ))
          ) : (
            <EmptyState
              title="Nemate termina u ovoj kategoriji."
              description="Kada se pojave termini sa ovim statusom, bit će prikazani ovdje."
              actionLabel="Zakaži termin"
              actionTo="/app/book-appointment"
            />
          )}
        </div>
      </SectionCard>

      <SectionCard>
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
              Brzi pristup
            </p>
            <h2 className="mt-3 text-2xl font-black text-stone-50">
              Želite novi termin?
            </h2>
          </div>
          <Link to="/app/book-appointment" className={buttonStyles.primary}>
            Zakaži termin
          </Link>
        </div>
      </SectionCard>
    </div>
  )
}

export default MyAppointmentsPage
