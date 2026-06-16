import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import AppCard from '../../components/common/AppCard'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatusBadge from '../../components/common/StatusBadge'
import {
  createEmployeeTimeOff,
  getEmployeeDashboard,
  getEmployeeProfile,
  getEmployeeSchedule,
  getEmployeeTimeOff,
  type EmployeeDashboardDto,
  type EmployeeProfileDto,
  type EmployeeScheduleItemDto,
  type EmployeeScheduleDto,
  type EmployeeTimeOffCreateRequest,
  type EmployeeTimeOffDto,
} from '../../api/employeeApi'
import { buttonStyles } from '../../components/common/buttonStyles'

type EmployeeModule = 'schedule' | 'appointments' | 'time-off' | 'services' | 'profile'

interface EmployeeModulePageProps {
  module: EmployeeModule
}

const moduleCopy: Record<EmployeeModule, { title: string; subtitle: string }> = {
  schedule: {
    title: 'Moj raspored',
    subtitle: 'Dnevni pregled radnog vremena.',
  },
  appointments: {
    title: 'Dodijeljeni termini',
    subtitle: 'Termini koji su dodijeljeni vama.',
  },
  'time-off': {
    title: 'Odsustva',
    subtitle: 'Zahtjevi za odsustvo i dostupnost.',
  },
  services: {
    title: 'Usluge',
    subtitle: 'Usluge koje obavljate u salonu.',
  },
  profile: {
    title: 'Profil',
    subtitle: 'Vaši podaci, specijalnost i radno vrijeme.',
  },
}

function formatDateTime(value: string) {
  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleString('bs-BA', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
      })
}

function toDateTimeLocalValue(date: Date) {
  const offsetDate = new Date(date.getTime() - date.getTimezoneOffset() * 60000)
  return offsetDate.toISOString().slice(0, 16)
}

function toDateOnlyValue(date: Date) {
  return toDateTimeLocalValue(date).slice(0, 10)
}

function toApiDateTimeOffset(value: string) {
  return new Date(value).toISOString()
}

function translateStatus(status: string) {
  const labels: Record<string, string> = {
    PENDING: 'Na čekanju',
    APPROVED: 'Odobreno',
    REJECTED: 'Odbijeno',
    CANCELLED: 'Otkazano',
  }

  return labels[status] ?? status
}

function getStatusTone(status: string) {
  if (status === 'APPROVED' || status === 'CONFIRMED') return 'success'
  if (status === 'PENDING') return 'warning'
  if (status === 'REJECTED' || status === 'CANCELLED') return 'danger'
  return 'neutral'
}

function formatTime(value: string) {
  if (/^\d{2}:\d{2}/.test(value)) return value.slice(0, 5)

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

function formatWorkingHours(
  workingHours: EmployeeScheduleDto['workingHours'],
) {
  if (!workingHours.length) return 'Radno vrijeme nije uneseno.'

  return workingHours
    .map(
      (workingHour) =>
        `${formatTime(workingHour.startTime)} - ${formatTime(workingHour.endTime)}`,
    )
    .join(', ')
}

function translateAppointmentStatus(status?: string | null) {
  const labels: Record<string, string> = {
    PENDING: 'Na čekanju',
    CONFIRMED: 'Potvrđen',
    COMPLETED: 'Završen',
    CANCELLED: 'Otkazan',
    NO_SHOW: 'Nije se pojavio',
  }

  return status ? labels[status] ?? status : ''
}

function translateDayStatus(status?: string) {
  const labels: Record<string, string> = {
    AVAILABLE: 'Dostupan',
    BUSY: 'Zauzet',
    ABSENT: 'Odsutan',
  }

  return status ? labels[status] ?? status : 'Nije dostupno'
}

function getDayStatusTone(status?: string) {
  if (status === 'AVAILABLE') return 'success'
  if (status === 'BUSY') return 'warning'
  if (status === 'ABSENT') return 'danger'
  return 'neutral'
}

function getTodayAppointments(items: EmployeeScheduleItemDto[]) {
  return items.filter(
    (item) =>
      item.type === 'APPOINTMENT' &&
      (item.status?.toUpperCase() === 'PENDING' ||
        item.status?.toUpperCase() === 'CONFIRMED'),
  )
}

function ScheduleContent() {
  const [schedule, setSchedule] = useState<EmployeeScheduleDto | null>(null)
  const [dashboard, setDashboard] = useState<EmployeeDashboardDto | null>(null)
  const [selectedDate, setSelectedDate] = useState(() =>
    toDateOnlyValue(new Date()),
  )
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadSchedule() {
      setIsLoading(true)
      setError('')

      try {
        const [scheduleData, dashboardData] = await Promise.all([
          getEmployeeSchedule(selectedDate),
          getEmployeeDashboard(selectedDate),
        ])

        if (isMounted) {
          setSchedule(scheduleData)
          setDashboard(dashboardData)
        }
      } catch (requestError) {
        if (isMounted) {
          setSchedule(null)
          setDashboard(null)
          setError(
            requestError instanceof Error
              ? requestError.message
              : 'Raspored trenutno nije dostupan.',
          )
        }
      } finally {
        if (isMounted) setIsLoading(false)
      }
    }

    loadSchedule()

    return () => {
      isMounted = false
    }
  }, [selectedDate])

  if (isLoading) {
    return <p className="text-sm text-stone-300">Učitavanje rasporeda...</p>
  }

  if (error) {
    return <p className="text-sm font-semibold text-red-100">{error}</p>
  }

  const appointments = getTodayAppointments(schedule?.items ?? [])
  const hasWorkingHours = (schedule?.workingHours.length ?? 0) > 0
  const currentTime = new Date().toLocaleTimeString('bs-BA', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  })
  const nextScheduleAppointment =
    appointments.find((appointment) => appointment.time >= currentTime) ??
    appointments[0]
  const nextAppointmentLabel =
    nextScheduleAppointment
      ? `${nextScheduleAppointment.time} - ${nextScheduleAppointment.title}`
      : 'Nema termina za izabrani datum.'

  return (
    <div className="grid gap-6">
      <label className="grid max-w-xs gap-2 text-sm font-semibold text-stone-300">
        Datum rasporeda
        <input
          type="date"
          value={selectedDate}
          onChange={(event) => setSelectedDate(event.target.value)}
          className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition [color-scheme:dark] focus:border-amber-200/35"
        />
      </label>

      <div className="grid gap-4 md:grid-cols-3">
        <AppCard variant="subtle">
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/65">
            Radno vrijeme danas
          </p>
          <p className="mt-3 text-xl font-black text-stone-50">
            {formatWorkingHours(schedule?.workingHours ?? [])}
          </p>
        </AppCard>

        <AppCard variant="subtle">
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/65">
            Sljedeći termin
          </p>
          <p className="mt-3 text-xl font-black text-stone-50">
            {nextAppointmentLabel}
          </p>
        </AppCard>

        <AppCard variant="subtle">
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/65">
            Status dana
          </p>
          <div className="mt-3">
            <StatusBadge
              label={translateDayStatus(dashboard?.summary.dayStatus)}
              tone={getDayStatusTone(dashboard?.summary.dayStatus)}
            />
          </div>
        </AppCard>
      </div>

      <div>
        <h2 className="text-2xl font-black text-stone-50">Današnji termini</h2>

        {!hasWorkingHours && (
          <p className="mt-4 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm text-stone-300">
            Nije definisano radno vrijeme za danas.
          </p>
        )}

        {appointments.length === 0 ? (
          <div className="mt-4">
            <EmptyState
              title="Nema termina za danas."
              description="Kada termini budu dodijeljeni vama, bit će prikazani ovdje."
            />
          </div>
        ) : (
          <div className="mt-4 grid gap-4">
            {appointments.map((item) => (
              <AppCard
                key={`${item.time}-${item.appointmentId}`}
                className="grid gap-3 rounded-2xl sm:grid-cols-[90px_1fr_auto]"
                variant="subtle"
              >
                <span className="font-black text-amber-100">
                  {formatTime(item.time)}
                </span>
                <div>
                  <h3 className="font-bold text-stone-50">{item.title}</h3>
                  <p className="mt-1 text-sm text-stone-400">
                    {item.clientName || 'Klijent nije naveden'}
                  </p>
                </div>
                {item.status && (
                  <StatusBadge
                    label={translateAppointmentStatus(item.status)}
                    tone={getStatusTone(item.status)}
                  />
                )}
              </AppCard>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

function TimeOffContent() {
  const [timeOffs, setTimeOffs] = useState<EmployeeTimeOffDto[]>([])
  const [form, setForm] = useState<EmployeeTimeOffCreateRequest>(() => {
    const start = new Date()
    start.setHours(start.getHours() + 1, 0, 0, 0)
    const end = new Date(start)
    end.setHours(end.getHours() + 1)

    return {
      startTime: toDateTimeLocalValue(start),
      endTime: toDateTimeLocalValue(end),
      reason: '',
    }
  })
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')

  async function loadTimeOffs(isMounted = true) {
    try {
      setIsLoading(true)
      setError('')

      const data = await getEmployeeTimeOff()
      if (isMounted) setTimeOffs(data)
    } catch (requestError) {
      if (isMounted) {
        setTimeOffs([])
        setError(
          requestError instanceof Error
            ? requestError.message
            : 'Odsustva trenutno nisu dostupna.',
        )
      }
    } finally {
      if (isMounted) setIsLoading(false)
    }
  }

  useEffect(() => {
    loadTimeOffs()
  }, [])

  function handleTimeOffChange(
    field: keyof EmployeeTimeOffCreateRequest,
    value: string,
  ) {
    setForm((currentForm) => ({ ...currentForm, [field]: value }))
    setError('')
    setSuccessMessage('')
  }

  async function handleSubmitTimeOff(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!form.startTime || !form.endTime) {
      setError('Unesite početak i kraj odsustva.')
      return
    }

    if (new Date(form.startTime) >= new Date(form.endTime)) {
      setError('Početak odsustva mora biti prije kraja.')
      return
    }

    try {
      setIsSubmitting(true)
      setError('')
      setSuccessMessage('')

      await createEmployeeTimeOff({
        startTime: toApiDateTimeOffset(form.startTime),
        endTime: toApiDateTimeOffset(form.endTime),
        reason: form.reason?.trim() || null,
      })

      setSuccessMessage('Zahtjev za odsustvo je poslan.')
      await loadTimeOffs()
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Zahtjev za odsustvo nije moguće poslati.',
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  if (isLoading) {
    return <p className="text-sm text-stone-300">Učitavanje odsustava...</p>
  }

  return (
    <div className="grid gap-6">
      <AppCard variant="subtle">
        <h2 className="text-2xl font-black text-stone-50">
          Novi zahtjev za odsustvo
        </h2>
        <form onSubmit={handleSubmitTimeOff} className="mt-5 grid gap-4">
          <div className="grid gap-4 md:grid-cols-2">
            <label className="grid gap-2 text-sm font-semibold text-stone-300">
              Početak
              <input
                type="datetime-local"
                value={form.startTime}
                onChange={(event) =>
                  handleTimeOffChange('startTime', event.target.value)
                }
                className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition [color-scheme:dark] focus:border-amber-200/35"
              />
            </label>

            <label className="grid gap-2 text-sm font-semibold text-stone-300">
              Kraj
              <input
                type="datetime-local"
                value={form.endTime}
                onChange={(event) =>
                  handleTimeOffChange('endTime', event.target.value)
                }
                className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition [color-scheme:dark] focus:border-amber-200/35"
              />
            </label>
          </div>

          <label className="grid gap-2 text-sm font-semibold text-stone-300">
            Razlog
            <textarea
              value={form.reason ?? ''}
              onChange={(event) =>
                handleTimeOffChange('reason', event.target.value)
              }
              rows={3}
              className="resize-none rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
              placeholder="Unesite razlog odsustva."
            />
          </label>

          {error && (
            <p className="rounded-2xl border border-red-300/20 bg-red-400/10 px-4 py-3 text-sm font-semibold text-red-100">
              {error}
            </p>
          )}

          {successMessage && (
            <p className="rounded-2xl border border-blue-200/20 bg-blue-950/40 px-4 py-3 text-sm font-semibold text-blue-100">
              {successMessage}
            </p>
          )}

          <button
            type="submit"
            disabled={isSubmitting}
            className={buttonStyles.primary}
          >
            {isSubmitting ? 'Slanje...' : 'Pošalji zahtjev'}
          </button>
        </form>
      </AppCard>

      <div>
        <h2 className="text-2xl font-black text-stone-50">Moji zahtjevi</h2>
        {!timeOffs.length ? (
          <div className="mt-4">
            <EmptyState
              title="Nemate zahtjeva za odsustvo."
              description="Kada pošaljete zahtjev za odsustvo, bit će prikazan ovdje."
            />
          </div>
        ) : (
          <div className="mt-4 grid gap-4">
            {timeOffs.map((item) => (
              <AppCard key={item.timeOffId} variant="subtle">
                <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
                  <div>
                    <h3 className="font-bold text-stone-50">
                      {formatDateTime(item.startDate)} - {formatDateTime(item.endDate)}
                    </h3>
                    <p className="mt-2 text-sm text-stone-400">
                      {item.reason || 'Razlog nije unesen.'}
                    </p>
                    {item.reviewNote && (
                      <p className="mt-2 text-sm text-stone-500">
                        Napomena: {item.reviewNote}
                      </p>
                    )}
                  </div>
                  <StatusBadge
                    label={translateStatus(item.status)}
                    tone={getStatusTone(item.status)}
                  />
                </div>
              </AppCard>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

function ProfileContent() {
  const [profile, setProfile] = useState<EmployeeProfileDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadProfile() {
      setIsLoading(true)
      setError('')

      try {
        const data = await getEmployeeProfile()
        if (isMounted) setProfile(data)
      } catch (requestError) {
        if (isMounted) {
          setProfile(null)
          setError(
            requestError instanceof Error
              ? requestError.message
              : 'Profil trenutno nije dostupan.',
          )
        }
      } finally {
        if (isMounted) setIsLoading(false)
      }
    }

    loadProfile()

    return () => {
      isMounted = false
    }
  }, [])

  if (isLoading) {
    return <p className="text-sm text-stone-300">Učitavanje profila...</p>
  }

  if (error) {
    return <p className="text-sm font-semibold text-red-100">{error}</p>
  }

  if (!profile) {
    return (
      <EmptyState
        title="Profil nije dostupan."
        description="Podaci profila će biti prikazani kada ih backend vrati."
      />
    )
  }

  const fields = [
    ['Ime', profile.firstName],
    ['Prezime', profile.lastName],
    ['Email', profile.email],
    ['Telefon', profile.phoneNumber],
    ['Pozicija', profile.position],
    ['Salon', profile.shopName],
  ]

  return (
    <div className="grid gap-4 md:grid-cols-2">
      {fields.map(([label, value]) => (
        <label key={label} className="grid gap-2 text-sm text-stone-300">
          {label}
          <input
            disabled
            value={value || 'Nije uneseno'}
            className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-200"
          />
        </label>
      ))}
    </div>
  )
}

function EmployeeModulePage({ module }: EmployeeModulePageProps) {
  const copy = moduleCopy[module]

  return (
    <div className="grid gap-6">
      <PageHeader
        eyebrow="Studio prostor"
        title={copy.title}
        subtitle={copy.subtitle}
      />

      <SectionCard>
        {module === 'schedule' && <ScheduleContent />}
        {module === 'time-off' && <TimeOffContent />}
        {module === 'profile' && <ProfileContent />}
        {(module === 'appointments' || module === 'services') && (
          <EmptyState
            title="Nema podataka za prikaz."
            description="Ova stranica koristi posebnu implementaciju kada je povezana kroz rute."
          />
        )}
      </SectionCard>
    </div>
  )
}

export default EmployeeModulePage
