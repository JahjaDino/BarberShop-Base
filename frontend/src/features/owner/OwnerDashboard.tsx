import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  getOwnerAppointments,
  type AppointmentDto,
  type AppointmentStatus,
} from '../../api/appointmentsApi'
import { getEmployees } from '../../api/employeesApi'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatCard from '../../components/common/StatCard'
import StatusBadge from '../../components/common/StatusBadge'
import { buttonStyles } from '../../components/common/buttonStyles'
import type { OwnerEmployee } from '../../types/employee'

type StatusTone = 'success' | 'warning' | 'neutral' | 'danger'

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

function getAppointmentTone(status: AppointmentStatus): StatusTone {
  if (status === 'CONFIRMED' || status === 'COMPLETED') return 'success'
  if (status === 'PENDING') return 'warning'
  if (status === 'CANCELLED' || status === 'NO_SHOW') return 'danger'
  return 'neutral'
}

function getAvailabilityLabel(employee: OwnerEmployee) {
  if (!employee.active) return 'Neaktivan'
  if (employee.availabilityStatus === 'AVAILABLE') return 'Dostupan'
  if (employee.availabilityStatus === 'BUSY') return 'Zauzet'
  if (employee.availabilityStatus === 'ABSENT') return 'Odsutan'
  return 'Nepoznato'
}

function getAvailabilityTone(employee: OwnerEmployee): StatusTone {
  if (!employee.active) return 'neutral'
  if (employee.availabilityStatus === 'AVAILABLE') return 'success'
  if (employee.availabilityStatus === 'BUSY') return 'warning'
  if (employee.availabilityStatus === 'ABSENT') return 'neutral'
  return 'neutral'
}

function formatTime(value: string) {
  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleTimeString('bs-BA', {
    hour: '2-digit',
    minute: '2-digit',
  })
}

function getServiceNames(appointment: AppointmentDto) {
  const names = appointment.services
    .map((service) => service.serviceName)
    .filter(Boolean)

  return names.length ? names.join(', ') : 'Usluga nije dostupna'
}

function isToday(value: string) {
  const date = new Date(value)
  const today = new Date()

  return (
    date.getFullYear() === today.getFullYear() &&
    date.getMonth() === today.getMonth() &&
    date.getDate() === today.getDate()
  )
}

function OwnerDashboard() {
  const [employees, setEmployees] = useState<OwnerEmployee[]>([])
  const [appointments, setAppointments] = useState<AppointmentDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadDashboardData() {
      setIsLoading(true)
      setError('')

      try {
        const [employeeItems, appointmentResponse] = await Promise.all([
          getEmployees(),
          getOwnerAppointments(),
        ])

        if (isMounted) {
          setEmployees(employeeItems)
          setAppointments(appointmentResponse.items)
        }
      } catch (requestError) {
        if (isMounted) {
          setEmployees([])
          setAppointments([])
          setError(
            requestError instanceof Error
              ? requestError.message
              : 'Pregled salona trenutno nije dostupan.',
          )
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadDashboardData()

    return () => {
      isMounted = false
    }
  }, [])

  const activeEmployees = useMemo(
    () => employees.filter((employee) => employee.active),
    [employees],
  )
  const availableEmployeesCount = activeEmployees.filter(
    (employee) => employee.availabilityStatus === 'AVAILABLE',
  ).length
  const todayAppointments = useMemo(
    () => appointments.filter((appointment) => isToday(appointment.startTime)),
    [appointments],
  )
  const pendingAppointmentsCount = todayAppointments.filter(
    (appointment) => appointment.status === 'PENDING',
  ).length

  return (
    <div className="grid gap-6">
      <PageHeader
        eyebrow="Vlasnički prostor"
        title="Dobrodošli u vlasnički prostor"
        subtitle="Upravljajte terminima, frizerima, uslugama i poslovanjem salona."
      />

      <div className="flex flex-col gap-3 sm:flex-row">
        <Link to="/app/owner/appointments" className={buttonStyles.primary}>
          Pregled termina
        </Link>
        <Link to="/app/owner/employees" className={buttonStyles.secondary}>
          Dodaj frizera
        </Link>
      </div>

      {error && (
        <div className="rounded-2xl border border-red-300/20 bg-red-400/10 p-4 text-sm font-semibold text-red-100">
          {error}
        </div>
      )}

      {isLoading ? (
        <SectionCard>
          <div className="rounded-2xl border border-amber-200/10 bg-white/[0.035] p-5 text-sm text-stone-300">
            Učitavanje pregleda salona...
          </div>
        </SectionCard>
      ) : (
        <>
          <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
            <StatCard
              label="Današnji termini"
              value={String(todayAppointments.length)}
              detail="Stvarni termini iz baze"
            />
            <StatCard
              label="Termini na čekanju"
              value={String(pendingAppointmentsCount)}
              detail="Čekaju potvrdu"
            />
            <StatCard
              label="Aktivni frizeri"
              value={String(activeEmployees.length)}
              detail={`${availableEmployeesCount} trenutno dostupno`}
            />
          </section>

          <section className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_360px]">
            <SectionCard>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                Današnji termini
              </p>
              <div className="mt-5 grid gap-3">
                {todayAppointments.length > 0 ? (
                  todayAppointments.map((appointment) => (
                    <article
                      key={appointment.id}
                      className="grid gap-3 rounded-2xl border border-amber-200/10 bg-black/25 p-4 md:grid-cols-[80px_1fr_auto] md:items-center"
                    >
                      <span className="font-black text-amber-100">
                        {formatTime(appointment.startTime)}
                      </span>
                      <span className="text-stone-100">
                        {getServiceNames(appointment)}
                      </span>
                      <StatusBadge
                        label={translateAppointmentStatus(appointment.status)}
                        tone={getAppointmentTone(appointment.status)}
                      />
                    </article>
                  ))
                ) : (
                  <EmptyState
                    title="Nema termina."
                    description="Današnji termini će biti prikazani kada postoje u bazi."
                  />
                )}
              </div>
            </SectionCard>

            <SectionCard>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                Frizeri danas
              </p>
              <div className="mt-5 grid gap-3">
                {activeEmployees.length === 0 ? (
                  <EmptyState
                    title="Nema aktivnih frizera."
                    description="Aktivni frizeri će biti prikazani kada postoje u bazi."
                  />
                ) : (
                  activeEmployees.map((employee) => (
                    <article
                      key={employee.employeeId}
                      className="rounded-2xl border border-amber-200/10 bg-black/25 p-4"
                    >
                      <div className="flex items-center justify-between gap-4">
                        <div className="min-w-0">
                          <h3 className="truncate font-bold text-stone-50">
                            {employee.fullName}
                          </h3>
                          <p className="mt-1 text-sm text-stone-400">
                            {employee.position || 'Pozicija nije unesena'}
                          </p>
                        </div>
                        <StatusBadge
                          label={getAvailabilityLabel(employee)}
                          tone={getAvailabilityTone(employee)}
                        />
                      </div>
                      <p className="mt-3 text-sm text-stone-400">
                        {employee.appointmentsCountToday} termina danas
                      </p>
                    </article>
                  ))
                )}
              </div>
            </SectionCard>
          </section>
        </>
      )}
    </div>
  )
}

export default OwnerDashboard
