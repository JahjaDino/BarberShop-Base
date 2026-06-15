import { useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import {
  cancelAppointment,
  getEmployeeAppointments,
  updateAppointmentStatus,
  type AppointmentStatus,
  type AppointmentStatusUpdateType,
  type EmployeeAppointmentListItemDto,
} from '../../api/appointmentsApi'
import AppButton from '../../components/common/AppButton'
import AppCard from '../../components/common/AppCard'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatusBadge from '../../components/common/StatusBadge'

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
  if (status === 'CONFIRMED' || status === 'COMPLETED') return 'success'
  if (status === 'PENDING') return 'warning'
  if (status === 'CANCELLED' || status === 'NO_SHOW') return 'danger'
  return 'neutral'
}

function normalizeAppointmentStatus(status: string) {
  return status.toUpperCase() as AppointmentStatus
}

interface AppointmentCardProps {
  appointment: EmployeeAppointmentListItemDto
  children?: ReactNode
}

function AppointmentCard({ appointment, children }: AppointmentCardProps) {
  return (
    <AppCard className="min-h-[132px]" variant="interactive">
      <div className="grid min-w-0 gap-5 lg:grid-cols-[minmax(0,1.25fr)_220px_minmax(240px,auto)] lg:items-center">
        <div className="min-w-0">
          <h3 className="truncate text-xl font-black text-stone-50">
            {appointment.clientName || 'Klijent'}
          </h3>
          <p className="mt-2 line-clamp-2 text-sm leading-6 text-stone-400">
            {appointment.serviceName || 'Usluga'}
          </p>
          <p className="mt-2 text-xs font-semibold uppercase tracking-[0.16em] text-amber-100/70">
            {appointment.durationMinutes} min · {formatPrice(appointment.price)}
          </p>
        </div>

        <div className="flex w-[220px] items-center justify-center justify-self-center rounded-full border border-amber-200/10 bg-white/[0.035] px-4 py-2.5 text-center shadow-[inset_0_1px_0_rgba(255,255,255,0.03)]">
          <span className="text-sm font-black text-stone-100">
            {formatDate(appointment.date)}
          </span>
          <span className="px-2 text-amber-200/50">•</span>
          <span className="text-sm font-black text-amber-100">
            {formatTime(appointment.time)}
          </span>
        </div>

        <div className="flex min-w-0 flex-col gap-3 lg:items-end">
          <StatusBadge
            label={translateStatus(appointment.status)}
            tone={getStatusTone(appointment.status)}
          />
          <div className="flex min-h-[42px] flex-wrap gap-2 lg:justify-end">
            {children}
          </div>
        </div>
      </div>
    </AppCard>
  )
}

function EmployeeAppointmentsPage() {
  const [appointments, setAppointments] = useState<
    EmployeeAppointmentListItemDto[]
  >([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [actionAppointmentId, setActionAppointmentId] = useState<number | null>(
    null,
  )

  const pendingAppointments = useMemo(
    () =>
      appointments.filter(
        (appointment) => normalizeAppointmentStatus(appointment.status) === 'PENDING',
      ),
    [appointments],
  )
  const confirmedAppointments = useMemo(
    () =>
      appointments.filter(
        (appointment) => normalizeAppointmentStatus(appointment.status) === 'CONFIRMED',
      ),
    [appointments],
  )
  const historyAppointments = useMemo(
    () =>
      appointments.filter((appointment) =>
        ['CANCELLED', 'COMPLETED', 'NO_SHOW'].includes(
          normalizeAppointmentStatus(appointment.status),
        ),
      ),
    [appointments],
  )

  const loadAppointments = async () => {
    setIsLoading(true)
    setError('')

    try {
      const response = await getEmployeeAppointments()
      setAppointments(response.items)
    } catch (requestError) {
      setAppointments([])
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Termini trenutno nisu dostupni.',
      )
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadAppointments()
  }, [])

  const handleStatusChange = async (
    appointmentId: number,
    status: AppointmentStatusUpdateType,
  ) => {
    setActionAppointmentId(appointmentId)
    setError('')

    try {
      await updateAppointmentStatus(appointmentId, status)
      await loadAppointments()
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
      await cancelAppointment(appointmentId, 'Termin je odbijen od frizera.')
      await loadAppointments()
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

  return (
    <div className="grid gap-6">
      <PageHeader
        eyebrow="Frizerski prostor"
        title="Dodijeljeni termini"
        subtitle="Pregledajte zahtjeve, potvrđene termine i historiju termina."
      />

      {error && (
        <div className="rounded-2xl border border-red-300/20 bg-red-400/10 p-4 text-sm font-semibold text-red-100">
          {error}
        </div>
      )}

      {isLoading ? (
        <SectionCard>
          <div className="rounded-2xl border border-amber-200/10 bg-white/[0.035] p-5 text-sm text-stone-300">
            Učitavanje termina...
          </div>
        </SectionCard>
      ) : (
        <>
          <SectionCard>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                Zahtjevi za potvrdu
              </p>
              <h2 className="mt-3 text-2xl font-black text-stone-50">
                Termini na čekanju
              </h2>
            </div>

            <div className="mt-5 grid gap-4">
              {pendingAppointments.length > 0 ? (
                pendingAppointments.map((appointment) => (
                  <AppointmentCard
                    key={appointment.appointmentId}
                    appointment={appointment}
                  >
                    <AppButton
                      disabled={isActionLoading(appointment.appointmentId)}
                      onClick={() =>
                        handleStatusChange(
                          appointment.appointmentId,
                          'CONFIRMED',
                        )
                      }
                      variant="secondary"
                    >
                      Prihvati
                    </AppButton>
                    <AppButton
                      disabled={isActionLoading(appointment.appointmentId)}
                      onClick={() => handleReject(appointment.appointmentId)}
                      variant="danger"
                    >
                      Odbij
                    </AppButton>
                  </AppointmentCard>
                ))
              ) : (
                <EmptyState
                  title="Nema termina na čekanju."
                  description="Novi zahtjevi za termin bit će prikazani ovdje."
                />
              )}
            </div>
          </SectionCard>

          <SectionCard>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                Aktivni termini
              </p>
              <h2 className="mt-3 text-2xl font-black text-stone-50">
                Potvrđeni termini
              </h2>
            </div>

            <div className="mt-5 grid gap-4">
              {confirmedAppointments.length > 0 ? (
                confirmedAppointments.map((appointment) => (
                  <AppointmentCard
                    key={appointment.appointmentId}
                    appointment={appointment}
                  >
                    <AppButton
                      disabled={isActionLoading(appointment.appointmentId)}
                      onClick={() =>
                        handleStatusChange(
                          appointment.appointmentId,
                          'COMPLETED',
                        )
                      }
                      variant="secondary"
                    >
                      Završi
                    </AppButton>
                    <AppButton
                      disabled={isActionLoading(appointment.appointmentId)}
                      onClick={() =>
                        handleStatusChange(appointment.appointmentId, 'NO_SHOW')
                      }
                      variant="ghost"
                    >
                      Nije se pojavio
                    </AppButton>
                  </AppointmentCard>
                ))
              ) : (
                <EmptyState
                  title="Nema potvrđenih termina."
                  description="Potvrđeni termini bit će prikazani u ovoj sekciji."
                />
              )}
            </div>
          </SectionCard>

          <SectionCard>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                Historija
              </p>
              <h2 className="mt-3 text-2xl font-black text-stone-50">
                Historija termina
              </h2>
              <p className="mt-2 max-w-2xl text-sm leading-6 text-stone-400">
                Otkazani, završeni i propušteni termini ostaju u historiji radi
                evidencije.
              </p>
            </div>

            <div className="mt-5 grid gap-4">
              {historyAppointments.length > 0 ? (
                historyAppointments.map((appointment) => (
                  <AppointmentCard
                    key={appointment.appointmentId}
                    appointment={appointment}
                  />
                ))
              ) : (
                <EmptyState
                  title="Historija je prazna."
                  description="Završeni, otkazani i propušteni termini bit će prikazani ovdje."
                />
              )}
            </div>
          </SectionCard>
        </>
      )}
    </div>
  )
}

export default EmployeeAppointmentsPage
