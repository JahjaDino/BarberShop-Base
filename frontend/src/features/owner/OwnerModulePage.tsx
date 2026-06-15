import { useEffect, useState } from 'react'
import {
  getOwnerPortalAppointments,
  type AppointmentStatus,
  type OwnerAppointmentListItemDto,
} from '../../api/appointmentsApi'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatusBadge from '../../components/common/StatusBadge'

type OwnerModule = 'appointments'

interface OwnerModulePageProps {
  module: OwnerModule
}

const moduleCopy: Record<OwnerModule, { title: string; subtitle: string }> = {
  appointments: {
    title: 'Termini',
    subtitle: 'Pregled i organizacija termina u salonu.',
  },
}

function translateAppointmentStatus(status: AppointmentStatus) {
  const labels: Record<AppointmentStatus, string> = {
    PENDING: 'Na čekanju',
    CONFIRMED: 'Potvrđen',
    COMPLETED: 'Završen',
    CANCELLED: 'Otkazan',
    NO_SHOW: 'Nije se pojavio',
  }

  return labels[status]
}

function getAppointmentTone(status: AppointmentStatus) {
  if (status === 'CONFIRMED' || status === 'COMPLETED') return 'success'
  if (status === 'PENDING') return 'warning'
  if (status === 'CANCELLED' || status === 'NO_SHOW') return 'danger'
  return 'neutral'
}

function formatDate(value: string) {
  const [year, month, day] = value.split('-')

  return year && month && day ? `${day}.${month}.${year}` : value
}

function formatTime(value: string) {
  return value.slice(0, 5)
}

function AppointmentsContent() {
  const [appointments, setAppointments] = useState<OwnerAppointmentListItemDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadAppointments() {
      setIsLoading(true)
      setError('')

      try {
        const response = await getOwnerPortalAppointments()
        if (isMounted) setAppointments(response.items)
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
        if (isMounted) setIsLoading(false)
      }
    }

    loadAppointments()

    return () => {
      isMounted = false
    }
  }, [])

  if (isLoading) {
    return <p className="text-sm text-stone-300">Učitavanje termina...</p>
  }

  if (error) {
    return <p className="text-sm font-semibold text-red-100">{error}</p>
  }

  if (!appointments.length) {
    return (
      <EmptyState
        title="Nema termina."
        description="Termini će biti prikazani kada postoje u bazi."
      />
    )
  }

  return (
    <div className="grid gap-3">
      {appointments.map((appointment) => (
        <article
          key={appointment.appointmentId}
          className="grid gap-4 rounded-2xl border border-amber-200/10 bg-black/25 p-4 lg:grid-cols-[150px_minmax(0,1fr)_minmax(0,1fr)_auto] lg:items-center"
        >
          <div>
            <span className="block font-semibold text-amber-100">
              {formatTime(appointment.time)}
            </span>
            <span className="mt-1 block text-xs text-stone-500">
              {formatDate(appointment.date)}
            </span>
          </div>
          <div className="min-w-0">
            <p className="truncate font-semibold text-stone-100">
              {appointment.serviceName || 'Usluga nije dostupna'}
            </p>
            <p className="mt-1 text-sm text-stone-400">
              Klijent: {appointment.clientName || 'Nije naveden'}
            </p>
          </div>
          <p className="text-sm text-stone-300">
            Frizer: {appointment.employeeName || 'Nije naveden'}
          </p>
          <StatusBadge
            label={translateAppointmentStatus(appointment.status)}
            tone={getAppointmentTone(appointment.status)}
          />
        </article>
      ))}
    </div>
  )
}

function OwnerModulePage({ module }: OwnerModulePageProps) {
  const copy = moduleCopy[module]

  return (
    <div className="grid gap-6">
      <PageHeader
        eyebrow="Vlasnički prostor"
        title={copy.title}
        subtitle={copy.subtitle}
      />

      <SectionCard>
        {module === 'appointments' && <AppointmentsContent />}
      </SectionCard>
    </div>
  )
}

export default OwnerModulePage
