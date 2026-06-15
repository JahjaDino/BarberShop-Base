import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  getEmployeeDashboard,
  getEmployeeSchedule,
  type EmployeeDashboardDto,
  type EmployeeScheduleDto,
  type EmployeeScheduleItemDto,
} from '../../api/employeeApi'
import {
  getEmployeeAppointments,
  type EmployeeAppointmentListItemDto,
} from '../../api/appointmentsApi'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatCard from '../../components/common/StatCard'
import StatusBadge from '../../components/common/StatusBadge'
import { buttonStyles } from '../../components/common/buttonStyles'

function formatDateTime(value?: string | null) {
  if (!value) return 'Nema termina'

  const timeMatch = value.match(/T(\d{2}:\d{2})/)
  if (timeMatch) return timeMatch[1]

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value

  return date.toLocaleTimeString('bs-BA', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })
}

function formatAppointmentDate(date: string) {
  const [year, month, day] = date.split('-')

  return year && month && day ? `${day}.${month}.${year}` : date
}

function toDateOnlyValue(date: Date) {
  const offsetDate = new Date(date.getTime() - date.getTimezoneOffset() * 60000)
  return offsetDate.toISOString().slice(0, 10)
}

function translateStatus(status: string) {
  const labels: Record<string, string> = {
    PENDING: 'Na čekanju',
    CONFIRMED: 'Potvrđen',
    COMPLETED: 'Završen',
    CANCELLED: 'Otkazan',
    NO_SHOW: 'Nije se pojavio',
    AVAILABLE: 'Dostupan',
    BUSY: 'Zauzet',
    ABSENT: 'Odsutan',
  }

  return labels[status] ?? (status || 'Nije dostupno')
}

function getScheduleAppointments(items: EmployeeScheduleItemDto[]) {
  return items.filter(
    (item) =>
      item.type === 'APPOINTMENT' &&
      (item.status?.toUpperCase() === 'PENDING' ||
        item.status?.toUpperCase() === 'CONFIRMED'),
  )
}

function isBlockingAppointment(status?: string | null) {
  const normalizedStatus = status?.toUpperCase()

  return normalizedStatus === 'PENDING' || normalizedStatus === 'CONFIRMED'
}

function getStatusTone(status: string) {
  if (status === 'CONFIRMED' || status === 'AVAILABLE') return 'success'
  if (status === 'PENDING' || status === 'BUSY') return 'warning'
  if (status === 'CANCELLED' || status === 'NO_SHOW' || status === 'ABSENT') {
    return 'danger'
  }

  return 'neutral'
}

function EmployeeDashboard() {
  const [dashboard, setDashboard] = useState<EmployeeDashboardDto | null>(null)
  const [schedule, setSchedule] = useState<EmployeeScheduleDto | null>(null)
  const [appointments, setAppointments] = useState<
    EmployeeAppointmentListItemDto[]
  >([])
  const [selectedDate, setSelectedDate] = useState(() =>
    toDateOnlyValue(new Date()),
  )
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadDashboard() {
      setIsLoading(true)
      setError('')

      try {
        const [dashboardData, scheduleData, appointmentsData] = await Promise.all([
          getEmployeeDashboard(selectedDate),
          getEmployeeSchedule(selectedDate),
          getEmployeeAppointments(),
        ])

        if (isMounted) {
          const today = toDateOnlyValue(new Date())
          const now = new Date().toLocaleTimeString('bs-BA', {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false,
          })
          const blockingAppointments = appointmentsData.items.filter(
            (appointment) => isBlockingAppointment(appointment.status),
          )
          const selectedDateHasAppointments = blockingAppointments.some(
            (appointment) => appointment.date === selectedDate,
          )
          const nextAppointment = blockingAppointments
            .filter(
              (appointment) =>
                `${appointment.date}T${appointment.time.slice(0, 5)}` >=
                `${today}T${now}`,
            )
            .sort((first, second) =>
              `${first.date}T${first.time}`.localeCompare(
                `${second.date}T${second.time}`,
              ),
            )[0]

          if (
            selectedDate === today &&
            !selectedDateHasAppointments &&
            scheduleData.workingHours.length === 0 &&
            nextAppointment &&
            nextAppointment.date !== selectedDate
          ) {
            setSelectedDate(nextAppointment.date)
            return
          }

          setDashboard(dashboardData)
          setSchedule(scheduleData)
          setAppointments(appointmentsData.items)
        }
      } catch (requestError) {
        if (isMounted) {
          setDashboard(null)
          setSchedule(null)
          setAppointments([])
          setError(
            requestError instanceof Error
              ? requestError.message
              : 'Pregled radnog dana trenutno nije dostupan.',
          )
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadDashboard()

    return () => {
      isMounted = false
    }
  }, [selectedDate])

  const scheduleAppointments = getScheduleAppointments(schedule?.items ?? [])
  const selectedDateAppointments = appointments.filter(
    (appointment) =>
      appointment.date === selectedDate &&
      isBlockingAppointment(appointment.status),
  )
  const nowDate = toDateOnlyValue(new Date())
  const nowTime = new Date().toLocaleTimeString('bs-BA', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })
  const upcomingAppointments = appointments
    .filter((appointment) => isBlockingAppointment(appointment.status))
    .filter(
      (appointment) =>
        `${appointment.date}T${appointment.time.slice(0, 5)}` >=
        `${nowDate}T${nowTime}`,
    )
    .sort((first, second) =>
      `${first.date}T${first.time}`.localeCompare(
        `${second.date}T${second.time}`,
      ),
    )
  const visibleSchedule = schedule?.items ?? dashboard?.todaySchedule ?? []
  const workingHours = (schedule?.workingHours.length
    ? schedule.workingHours
    : dashboard?.summary.workingHoursToday ?? [])
    .map((item) => `${item.startTime} - ${item.endTime}`)
    .join(', ')
  const assignedAppointments =
    selectedDateAppointments.length > 0
      ? selectedDateAppointments.map((appointment) => ({
          appointmentId: appointment.appointmentId,
          clientName: appointment.clientName || 'Klijent nije naveden',
          serviceName: appointment.serviceName,
          time: appointment.time.slice(0, 5),
          status: appointment.status,
        }))
      : scheduleAppointments.length > 0
        ? scheduleAppointments.map((appointment) => ({
          appointmentId: appointment.appointmentId ?? 0,
          clientName: appointment.clientName || 'Klijent nije naveden',
          serviceName: appointment.title,
          time: appointment.time,
          status: appointment.status || 'PENDING',
        }))
        : dashboard?.assignedAppointments ?? []
  const todayAppointmentsCount =
    selectedDateAppointments.length ||
    scheduleAppointments.length ||
    dashboard?.summary.todayAppointmentsCount ||
    0
  const confirmedTodayCount =
    selectedDateAppointments.filter(
      (appointment) => appointment.status?.toUpperCase() === 'CONFIRMED',
    ).length ||
    scheduleAppointments.filter(
      (appointment) => appointment.status?.toUpperCase() === 'CONFIRMED',
    ).length ||
    dashboard?.summary.confirmedTodayAppointmentsCount ||
    0
  const currentTime = new Date().toLocaleTimeString('bs-BA', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })
  const nextScheduleAppointment =
    scheduleAppointments.find((appointment) => appointment.time >= currentTime) ??
    scheduleAppointments[0]
  const nextAppointment = selectedDateAppointments[0] ?? upcomingAppointments[0]
  const nextAppointmentValue = nextAppointment
    ? nextAppointment.time.slice(0, 5)
    : dashboard?.summary.nextAppointmentTime
      ? formatDateTime(dashboard.summary.nextAppointmentTime)
      : nextScheduleAppointment?.time ?? 'Nema termina'
  const nextAppointmentDetail =
    nextAppointment
      ? `${formatAppointmentDate(nextAppointment.date)} - ${nextAppointment.serviceName}`
      : dashboard?.summary.nextAppointmentServiceName ||
        nextScheduleAppointment?.title ||
        'Nema narednog termina'

  return (
    <div className="grid gap-6">
      <PageHeader
        eyebrow="Frizerski prostor"
        title="Dobrodošli u frizerski prostor"
        subtitle="Pratite današnje termine, raspored i odsustva."
      />

      <div className="flex flex-col gap-3 sm:flex-row">
        <Link to="/app/employee/schedule" className={buttonStyles.primary}>
          Pogledaj raspored
        </Link>
        <Link to="/app/employee/time-off" className={buttonStyles.secondary}>
          Zatraži odsustvo
        </Link>
        <label className="grid gap-2 text-sm font-semibold text-stone-300 sm:ml-auto">
          Datum pregleda
          <input
            type="date"
            value={selectedDate}
            onChange={(event) => setSelectedDate(event.target.value)}
            className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition [color-scheme:dark] focus:border-amber-200/35"
          />
        </label>
      </div>

      {error && (
        <div className="rounded-2xl border border-red-300/20 bg-red-400/10 p-4 text-sm font-semibold text-red-100">
          {error}
        </div>
      )}

      {isLoading ? (
        <SectionCard>
          <div className="rounded-2xl border border-amber-200/10 bg-white/[0.035] p-5 text-sm text-stone-300">
            Učitavanje radnog dana...
          </div>
        </SectionCard>
      ) : (
        <>
          <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <StatCard
              label="Termini danas"
              value={String(todayAppointmentsCount)}
              detail={`${confirmedTodayCount} potvrđeno`}
            />
            <StatCard
              label="Sljedeći termin"
              value={nextAppointmentValue}
              detail={nextAppointmentDetail}
            />
            <StatCard
              label="Radno vrijeme"
              value={workingHours || 'Nije uneseno'}
              detail="Današnja smjena"
            />
            <StatCard
              label="Status dana"
              value={translateStatus(dashboard?.summary.dayStatus ?? '')}
              detail="Prema današnjem rasporedu"
            />
          </section>

          <section className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_360px]">
            <SectionCard>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                Današnji raspored
              </p>
              <div className="mt-5 grid gap-3">
                {visibleSchedule.length ? (
                  visibleSchedule.map((item) => (
                    <article
                      key={`${item.time}-${item.title}-${item.appointmentId ?? item.type}`}
                      className="grid gap-3 rounded-2xl border border-amber-200/10 bg-black/25 p-4 sm:grid-cols-[82px_1fr_auto] sm:items-center"
                    >
                      <span className="font-black text-amber-100">
                        {item.time}
                      </span>
                      <span className="font-semibold text-stone-100">
                        {item.title}
                      </span>
                      {item.clientName && (
                        <span className="text-sm text-stone-400">
                          {item.clientName}
                        </span>
                      )}
                    </article>
                  ))
                ) : (
                  <EmptyState
                    title="Nema rasporeda za danas."
                    description="Kada postoje radni sati ili termini, bit će prikazani ovdje."
                  />
                )}
              </div>
            </SectionCard>

            <SectionCard>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                Odsustva
              </p>
              <h3 className="mt-4 text-2xl font-black text-stone-50">
                {dashboard?.timeOffSummary.hasActiveTimeOff
                  ? 'Imate aktivno odsustvo.'
                  : 'Nemate aktivnih odsustava.'}
              </h3>
              <p className="mt-3 text-sm leading-6 text-stone-400">
                Zahtjevi na čekanju:{' '}
                {dashboard?.timeOffSummary.pendingTimeOffCount ?? 0}
              </p>
              <Link to="/app/employee/time-off" className={`mt-5 ${buttonStyles.ghost}`}>
                Zatraži odsustvo
              </Link>
            </SectionCard>
          </section>

          <SectionCard>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
              Dodijeljeni termini
            </p>
            <div className="mt-5 grid gap-3">
              {assignedAppointments.length ? (
                assignedAppointments.map((appointment) => (
                  <article
                    key={appointment.appointmentId || `${appointment.time}-${appointment.serviceName}`}
                    className="grid gap-3 rounded-2xl border border-amber-200/10 bg-black/25 p-4 md:grid-cols-[1fr_1fr_90px_auto] md:items-center"
                  >
                    <span className="font-semibold text-stone-50">
                      {appointment.clientName}
                    </span>
                    <span className="text-stone-300">
                      {appointment.serviceName}
                    </span>
                    <span className="text-amber-100">{appointment.time}</span>
                    <StatusBadge
                      label={translateStatus(appointment.status)}
                      tone={getStatusTone(appointment.status)}
                    />
                  </article>
                ))
              ) : (
                <EmptyState
                  title="Nema dodijeljenih termina."
                  description="Kada vam budu dodijeljeni termini, bit će prikazani ovdje."
                />
              )}
            </div>
          </SectionCard>
        </>
      )}
    </div>
  )
}

export default EmployeeDashboard
