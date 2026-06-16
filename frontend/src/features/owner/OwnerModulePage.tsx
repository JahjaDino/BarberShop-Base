import { useCallback, useEffect, useState } from 'react'
import {
  cancelAppointment,
  getOwnerPortalAppointments,
  updateAppointmentStatus,
  type AppointmentStatus,
  type OwnerAppointmentListItemDto,
} from '../../api/appointmentsApi'
import AppButton from '../../components/common/AppButton'
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
  const [actionAppointmentId, setActionAppointmentId] = useState<number | null>(
    null,
  )

  const loadAppointments = useCallback(async (showLoading = true) => {
    if (showLoading) {
      setIsLoading(true)
    }
    setError('')

    try {
      const response = await getOwnerPortalAppointments()
      setAppointments(response.items)
    } catch (requestError) {
      setAppointments([])
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Termini trenutno nisu dostupni.',
      )
    } finally {
      if (showLoading) {
        setIsLoading(false)
      }
    }
  }, [])

  useEffect(() => {
    loadAppointments()
  }, [loadAppointments])

  const handleConfirm = async (appointmentId: number) => {
    setActionAppointmentId(appointmentId)
    setError('')

    try {
      await updateAppointmentStatus(appointmentId, 'CONFIRMED')
      await loadAppointments(false)
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Status termina nije moguće promijeniti.',
      )
    } finally {
      setActionAppointmentId(null)
    }
  }

  const handleReject = async (appointmentId: number) => {
    setActionAppointmentId(appointmentId)
    setError('')

    try {
      await cancelAppointment(appointmentId, 'Termin je odbijen od vlasnika.')
      await loadAppointments(false)
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Termin nije moguće odbiti.',
      )
    } finally {
      setActionAppointmentId(null)
    }
  }

  const isActionLoading = (appointmentId: number) =>
    actionAppointmentId === appointmentId

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
          className="grid gap-4 rounded-2xl border border-amber-200/10 bg-black/25 p-4 lg:grid-cols-[150px_minmax(0,1fr)_minmax(0,1fr)_minmax(180px,auto)] lg:items-center"
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
            Frizerka: {appointment.employeeName || 'Nije navedena'}
          </p>
          <div className="flex min-w-0 flex-col gap-3 lg:items-end">
            <StatusBadge
              label={translateAppointmentStatus(appointment.status)}
              tone={getAppointmentTone(appointment.status)}
            />
            {appointment.status === 'PENDING' && (
              <div className="flex min-h-[42px] w-full flex-col gap-2 sm:w-auto sm:flex-row sm:flex-wrap lg:justify-end">
                <AppButton
                  disabled={isActionLoading(appointment.appointmentId)}
                  onClick={() => handleConfirm(appointment.appointmentId)}
                  variant="secondary"
                >
                  Potvrdi
                </AppButton>
                <AppButton
                  disabled={isActionLoading(appointment.appointmentId)}
                  onClick={() => handleReject(appointment.appointmentId)}
                  variant="danger"
                >
                  Odbij
                </AppButton>
              </div>
            )}
          </div>
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
