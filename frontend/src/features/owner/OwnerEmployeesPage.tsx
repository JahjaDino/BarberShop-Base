import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import {
  createEmployee,
  createWorkingHour,
  getEmployeeWorkingHours,
  getEmployees,
  normalizeTime,
  normalizeTimeForApi,
  updateWorkingHour,
  type WorkingHourDto,
} from '../../api/employeesApi'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatusBadge from '../../components/common/StatusBadge'
import { buttonStyles } from '../../components/common/buttonStyles'
import type { CreateEmployeeRequest, OwnerEmployee } from '../../types/employee'

const initialForm: CreateEmployeeRequest = {
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
  password: '',
  position: '',
  bio: '',
  employmentDate: new Date().toISOString().slice(0, 10),
}

const weekDays = [
  { value: 1, label: 'Ponedjeljak' },
  { value: 2, label: 'Utorak' },
  { value: 3, label: 'Srijeda' },
  { value: 4, label: 'Četvrtak' },
  { value: 5, label: 'Petak' },
  { value: 6, label: 'Subota' },
  { value: 0, label: 'Nedjelja' },
]

interface WorkingHourRow {
  id?: number
  dayOfWeek: number
  startTime: string
  endTime: string
  active: boolean
}

function createEmptyWorkingHourRows(): WorkingHourRow[] {
  return createDefaultWorkingHourRows(false)
}

function createDefaultWorkingHourRows(useBusinessWeekDefaults = true): WorkingHourRow[] {
  return weekDays.map((day) => ({
    dayOfWeek: day.value,
    startTime: '08:00',
    endTime: '16:00',
    active: useBusinessWeekDefaults && day.value >= 1 && day.value <= 5,
  }))
}

function getAvailabilityLabel(status: string) {
  if (status === 'AVAILABLE') return 'Dostupan'
  if (status === 'BUSY') return 'Zauzet'
  if (status === 'ABSENT') return 'Odsutan'
  return 'Nepoznato'
}

function getAvailabilityTone(status: string) {
  if (status === 'AVAILABLE') return 'success'
  if (status === 'BUSY') return 'warning'
  if (status === 'ABSENT') return 'neutral'
  return 'neutral'
}

function mapWorkingHoursToRows(workingHours: WorkingHourDto[]) {
  const rows = createEmptyWorkingHourRows()

  workingHours.forEach((workingHour) => {
    const rowIndex = rows.findIndex(
      (row) => row.dayOfWeek === workingHour.dayOfWeek,
    )

    if (rowIndex >= 0 && !rows[rowIndex].id) {
      rows[rowIndex] = {
        id: workingHour.id,
        dayOfWeek: workingHour.dayOfWeek,
        startTime: normalizeTime(workingHour.startTime) || '08:00',
        endTime: normalizeTime(workingHour.endTime) || '16:00',
        active: workingHour.active,
      }
    }
  })

  return rows
}

interface WorkingHourTimeInputProps {
  label: string
  value: string
  disabled: boolean
  onChange: (value: string) => void
}

function WorkingHourTimeInput({
  label,
  value,
  disabled,
  onChange,
}: WorkingHourTimeInputProps) {
  const normalizedValue = normalizeTime(value) || ''

  return (
    <label className="grid gap-2 text-sm font-semibold text-stone-300">
      {label}
      <span
        className={`relative block min-w-0 overflow-hidden rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 font-mono text-stone-100 transition focus-within:border-amber-200/35 ${
          disabled ? 'opacity-45' : ''
        }`}
      >
        <span aria-hidden="true">{normalizedValue || '--:--'}</span>
        <input
          type="time"
          lang="en-GB"
          step="300"
          value={normalizedValue}
          disabled={disabled}
          aria-label={label}
          onChange={(event) => onChange(normalizeTime(event.target.value))}
          className="time-input-24h absolute inset-0 h-full w-full cursor-pointer opacity-0 disabled:cursor-not-allowed"
        />
      </span>
    </label>
  )
}

function OwnerEmployeesPage() {
  const [employees, setEmployees] = useState<OwnerEmployee[]>([])
  const [form, setForm] = useState<CreateEmployeeRequest>(initialForm)
  const [selectedEmployee, setSelectedEmployee] = useState<OwnerEmployee | null>(
    null,
  )
  const [workingHoursPanelOpen, setWorkingHoursPanelOpen] = useState(false)
  const [workingHourRows, setWorkingHourRows] = useState<WorkingHourRow[]>(
    createEmptyWorkingHourRows,
  )
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isLoadingWorkingHours, setIsLoadingWorkingHours] = useState(false)
  const [isSavingWorkingHours, setIsSavingWorkingHours] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [workingHoursError, setWorkingHoursError] = useState('')
  const [workingHoursSuccess, setWorkingHoursSuccess] = useState('')

  const activeEmployees = useMemo(
    () => employees.filter((employee) => employee.active),
    [employees],
  )

  async function loadEmployees() {
    try {
      setIsLoading(true)
      setError('')

      const data = await getEmployees()

      setEmployees(data)
      setSelectedEmployee((currentEmployee) => {
        if (!currentEmployee) return null

        return (
          data.find(
            (employee) =>
              employee.employeeId === currentEmployee.employeeId &&
              employee.active,
          ) ?? null
        )
      })
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Greška prilikom učitavanja frizera.',
      )
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadEmployees()
  }, [])

  async function loadWorkingHoursForEmployee(employee: OwnerEmployee) {
    try {
      setIsLoadingWorkingHours(true)
      setWorkingHoursError('')
      setWorkingHoursSuccess('')
      setWorkingHourRows(createEmptyWorkingHourRows())

      const response = await getEmployeeWorkingHours(employee.employeeId)

      setWorkingHourRows(
        response.items.length > 0
          ? mapWorkingHoursToRows(response.items)
          : createDefaultWorkingHourRows(),
      )
    } catch (err) {
      setWorkingHourRows(createEmptyWorkingHourRows())
      setWorkingHoursError(
        err instanceof Error
          ? err.message
          : 'Radno vrijeme trenutno nije moguće učitati.',
      )
    } finally {
      setIsLoadingWorkingHours(false)
    }
  }

  function handleEditWorkingHours(employee: OwnerEmployee) {
    setSelectedEmployee(employee)
    setWorkingHoursPanelOpen(true)
    void loadWorkingHoursForEmployee(employee)
  }

  function handleChange(field: keyof CreateEmployeeRequest, value: string) {
    setForm((currentForm) => ({ ...currentForm, [field]: value }))
    setError('')
    setSuccess('')
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (
      !form.firstName.trim() ||
      !form.lastName.trim() ||
      !form.email.trim() ||
      !form.password.trim() ||
      !form.position.trim()
    ) {
      setError('Ime, prezime, email, lozinka i pozicija su obavezni.')
      return
    }

    if (form.password.length < 8) {
      setError('Lozinka mora imati najmanje 8 karaktera.')
      return
    }

    try {
      setIsSubmitting(true)
      setError('')
      setSuccess('')

      const createdEmployee = await createEmployee({
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        phoneNumber: form.phoneNumber.trim(),
        password: form.password,
        position: form.position.trim(),
        bio: form.bio?.trim() || null,
        employmentDate: form.employmentDate,
      })

      setEmployees((currentEmployees) => [createdEmployee, ...currentEmployees])
      if (createdEmployee.active) {
        setSelectedEmployee(createdEmployee)
        setWorkingHoursPanelOpen(true)
        setWorkingHourRows(createDefaultWorkingHourRows())
      }
      setForm(initialForm)
      setSuccess('Frizer je uspješno dodan.')
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Greška prilikom dodavanja frizera.',
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  function updateWorkingHourRow(
    dayOfWeek: number,
    field: keyof Omit<WorkingHourRow, 'dayOfWeek' | 'id'>,
    value: string | boolean,
  ) {
    setWorkingHourRows((currentRows) =>
      currentRows.map((row) =>
        row.dayOfWeek === dayOfWeek
          ? {
              ...row,
              [field]:
                field === 'startTime' || field === 'endTime'
                  ? normalizeTime(String(value))
                  : value,
            }
          : row,
      ),
    )
    setWorkingHoursError('')
    setWorkingHoursSuccess('')
  }

  async function handleSaveWorkingHours() {
    if (!selectedEmployee) {
      setWorkingHoursError('Odaberite frizera prije spremanja radnog vremena.')
      return
    }

    const normalizedRows = workingHourRows.map((row) => ({
      ...row,
      startTime: normalizeTimeForApi(row.startTime),
      endTime: normalizeTimeForApi(row.endTime),
    }))

    const invalidRow = normalizedRows.find(
      (row) =>
        row.active &&
        (!/^\d{2}:\d{2}$/.test(row.startTime) ||
          !/^\d{2}:\d{2}$/.test(row.endTime) ||
          row.startTime >= row.endTime),
    )

    if (invalidRow) {
      setWorkingHoursError(
        !/^\d{2}:\d{2}$/.test(invalidRow.startTime) ||
          !/^\d{2}:\d{2}$/.test(invalidRow.endTime)
          ? 'Vrijeme mora biti u formatu HH:mm.'
          : 'Za aktivne dane unesite ispravno vrijeme. Početak mora biti prije kraja.',
      )
      return
    }

    try {
      setIsSavingWorkingHours(true)
      setWorkingHoursError('')
      setWorkingHoursSuccess('')

      for (const row of normalizedRows) {
        if (!row.id && !row.active) continue

        if (row.id) {
          const payload = {
            employeeId: selectedEmployee.employeeId,
            dayOfWeek: row.dayOfWeek,
            startTime: normalizeTimeForApi(row.startTime),
            endTime: normalizeTimeForApi(row.endTime),
            active: row.active,
          }

          console.log('Working hours payload', payload)
          await updateWorkingHour(row.id, payload)
        } else {
          const payload = {
            employeeId: selectedEmployee.employeeId,
            dayOfWeek: row.dayOfWeek,
            startTime: normalizeTimeForApi(row.startTime),
            endTime: normalizeTimeForApi(row.endTime),
          }

          console.log('Working hours payload', payload)
          await createWorkingHour(payload)
        }
      }

      const response = await getEmployeeWorkingHours(selectedEmployee.employeeId)
      setWorkingHourRows(
        response.items.length > 0
          ? mapWorkingHoursToRows(response.items)
          : createDefaultWorkingHourRows(),
      )
      setWorkingHoursSuccess('Radno vrijeme uspješno sačuvano.')
    } catch (err) {
      setWorkingHoursError(
        err instanceof Error
          ? err.message
          : 'Radno vrijeme trenutno nije moguće spremiti.',
      )
    } finally {
      setIsSavingWorkingHours(false)
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader
        eyebrow="Vlasnički prostor"
        title="Frizeri"
        subtitle="Upravljajte frizerima koji rade u salonu."
      />

      <div className="grid min-w-0 gap-6 xl:grid-cols-[minmax(320px,420px)_minmax(0,1fr)]">
        <div className="grid content-start gap-6">
          <SectionCard>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
              Novi frizer
            </p>
            <h2 className="mt-3 text-2xl font-black text-stone-50">
              Dodaj frizera
            </h2>

            <form onSubmit={handleSubmit} className="mt-6 grid gap-4">
              <div className="grid min-w-0 gap-4 sm:grid-cols-2 xl:grid-cols-1">
                <label className="grid gap-2 text-sm font-semibold text-stone-300">
                  Ime
                  <input
                    value={form.firstName}
                    onChange={(event) =>
                      handleChange('firstName', event.target.value)
                    }
                    className="w-full min-w-0 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                    placeholder="npr. Ime"
                  />
                </label>

                <label className="grid gap-2 text-sm font-semibold text-stone-300">
                  Prezime
                  <input
                    value={form.lastName}
                    onChange={(event) =>
                      handleChange('lastName', event.target.value)
                    }
                    className="w-full min-w-0 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                    placeholder="npr. Prezime"
                  />
                </label>
              </div>

              <label className="grid gap-2 text-sm font-semibold text-stone-300">
                Email
                <input
                  type="email"
                  value={form.email}
                  onChange={(event) => handleChange('email', event.target.value)}
                  className="w-full min-w-0 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                  placeholder="frizer@classiccuts.ba"
                />
              </label>

              <label className="grid gap-2 text-sm font-semibold text-stone-300">
                Telefon
                <input
                  value={form.phoneNumber}
                  onChange={(event) =>
                    handleChange('phoneNumber', event.target.value)
                  }
                  className="w-full min-w-0 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                  placeholder="+387 61 000 000"
                />
              </label>

              <label className="grid gap-2 text-sm font-semibold text-stone-300">
                Pozicija
                <input
                  value={form.position}
                  onChange={(event) =>
                    handleChange('position', event.target.value)
                  }
                  className="w-full min-w-0 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                  placeholder="npr. Senior frizer"
                />
              </label>

              <label className="grid gap-2 text-sm font-semibold text-stone-300">
                Privremena lozinka
                <input
                  type="password"
                  value={form.password}
                  onChange={(event) =>
                    handleChange('password', event.target.value)
                  }
                  className="w-full min-w-0 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                  placeholder="Najmanje 8 karaktera"
                />
              </label>

              <label className="grid gap-2 text-sm font-semibold text-stone-300">
                Datum zaposlenja
                <input
                  type="date"
                  value={form.employmentDate}
                  onChange={(event) =>
                    handleChange('employmentDate', event.target.value)
                  }
                  className="w-full min-w-0 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                />
              </label>

              <label className="grid gap-2 text-sm font-semibold text-stone-300">
                Bio
                <textarea
                  value={form.bio ?? ''}
                  onChange={(event) => handleChange('bio', event.target.value)}
                  rows={3}
                  className="w-full min-w-0 resize-none rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                  placeholder="Kratak opis specijalnosti."
                />
              </label>

              {error && (
                <div className="rounded-2xl border border-red-300/20 bg-red-400/10 px-4 py-3 text-sm font-semibold text-red-100">
                  {error}
                </div>
              )}

              {success && (
                <div className="rounded-2xl border border-emerald-300/20 bg-emerald-300/10 px-4 py-3 text-sm font-semibold text-emerald-100">
                  {success}
                </div>
              )}

              <button
                type="submit"
                disabled={isSubmitting}
                className={buttonStyles.primary}
              >
                {isSubmitting ? 'Dodavanje...' : 'Dodaj frizera'}
              </button>
            </form>
          </SectionCard>

          <SectionCard>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                  Aktivni frizeri
                </p>
                <h2 className="mt-3 text-2xl font-black text-stone-50">
                  Radno vrijeme
                </h2>
              </div>

              <button
                type="button"
                onClick={loadEmployees}
                className={buttonStyles.secondary}
              >
                Osvježi
              </button>
            </div>

            {isLoading ? (
              <p className="mt-6 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm text-stone-300">
                Učitavanje frizera...
              </p>
            ) : activeEmployees.length === 0 ? (
              <div className="mt-6">
                <EmptyState
                  title="Nema aktivnih frizera za podešavanje radnog vremena."
                  description="Dodajte ili aktivirajte frizera prije podešavanja radnog vremena."
                />
              </div>
            ) : (
              <div className="mt-6 grid gap-3">
                {activeEmployees.map((employee) => (
                  <article
                    key={employee.employeeId}
                    className={`rounded-2xl border p-4 transition ${
                      selectedEmployee?.employeeId === employee.employeeId
                        ? 'border-amber-200/40 bg-amber-100/10'
                        : 'border-amber-200/10 bg-black/25'
                    }`}
                  >
                    <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                      <div className="min-w-0">
                        <h3 className="break-words font-bold text-stone-50 sm:truncate">
                          {employee.fullName}
                        </h3>
                        <p className="mt-1 text-sm text-stone-400">
                          {employee.position || 'Pozicija nije unesena'}
                        </p>
                        <div className="mt-3">
                          <StatusBadge
                            label={getAvailabilityLabel(employee.availabilityStatus)}
                            tone={getAvailabilityTone(employee.availabilityStatus)}
                          />
                        </div>
                      </div>
                      <button
                        type="button"
                        onClick={() => handleEditWorkingHours(employee)}
                        className={
                          selectedEmployee?.employeeId === employee.employeeId
                            ? buttonStyles.primary
                            : buttonStyles.secondary
                        }
                      >
                        Uredi radno vrijeme
                      </button>
                    </div>
                  </article>
                ))}
              </div>
            )}
          </SectionCard>
        </div>

        <SectionCard>
          <div className="flex min-w-0 flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div className="min-w-0">
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                Radno vrijeme
              </p>
              <h2 className="mt-3 break-words text-2xl font-black text-stone-50">
                {selectedEmployee
                  ? selectedEmployee.fullName
                  : 'Odaberite frizera'}
              </h2>
              <p className="mt-2 max-w-2xl text-sm leading-6 text-stone-400">
                Radno vrijeme direktno utiče na slobodne termine u booking toku.
              </p>
            </div>

            {selectedEmployee && (
              <button
                type="button"
                disabled={isSavingWorkingHours || isLoadingWorkingHours}
                onClick={handleSaveWorkingHours}
                className={buttonStyles.primary}
              >
                {isSavingWorkingHours ? 'Spremanje...' : 'Spremi radno vrijeme'}
              </button>
            )}
          </div>

          {!workingHoursPanelOpen || !selectedEmployee ? (
            <div className="mt-6">
              <EmptyState
                title="Odaberite frizera."
                description="Nakon odabira frizera možete urediti njegovo radno vrijeme."
              />
            </div>
          ) : isLoadingWorkingHours ? (
            <p className="mt-6 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm text-stone-300">
              Učitavanje radnog vremena...
            </p>
          ) : (
            <>
              {workingHoursError && (
                <div className="mt-6 rounded-2xl border border-red-300/20 bg-red-400/10 px-4 py-3 text-sm font-semibold text-red-100">
                  {workingHoursError}
                </div>
              )}

              {workingHoursSuccess && (
                <div className="mt-6 rounded-2xl border border-emerald-300/20 bg-emerald-300/10 px-4 py-3 text-sm font-semibold text-emerald-100">
                  {workingHoursSuccess}
                </div>
              )}

              {!workingHourRows.some((row) => row.id) && (
                <p className="mt-6 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm text-stone-300">
                  Radno vrijeme nije uneseno.
                </p>
              )}

              <div className="mt-6 grid gap-3">
                {weekDays.map((day) => {
                  const row = workingHourRows.find(
                    (currentRow) => currentRow.dayOfWeek === day.value,
                  )

                  if (!row) return null

                  return (
                    <article
                      key={day.value}
                      className="grid min-w-0 gap-4 rounded-2xl border border-amber-200/10 bg-black/25 p-4 lg:grid-cols-[minmax(140px,180px)_minmax(0,1fr)_minmax(0,1fr)_minmax(110px,130px)] lg:items-center"
                    >
                      <label className="flex items-center gap-3 text-sm font-bold text-stone-100">
                        <input
                          type="checkbox"
                          checked={row.active}
                          onChange={(event) =>
                            updateWorkingHourRow(
                              day.value,
                              'active',
                              event.target.checked,
                            )
                          }
                          className="h-4 w-4 accent-[#d6b56c]"
                        />
                        {day.label}
                      </label>

                      <WorkingHourTimeInput
                        label="Početak"
                        value={row.startTime}
                        disabled={!row.active}
                        onChange={(value) =>
                          updateWorkingHourRow(day.value, 'startTime', value)
                        }
                      />

                      <WorkingHourTimeInput
                        label="Kraj"
                        value={row.endTime}
                        disabled={!row.active}
                        onChange={(value) =>
                          updateWorkingHourRow(day.value, 'endTime', value)
                        }
                      />

                      <StatusBadge
                        label={row.active ? 'Aktivan dan' : 'Neaktivan'}
                        tone={row.active ? 'success' : 'neutral'}
                      />
                    </article>
                  )
                })}
              </div>
            </>
          )}
        </SectionCard>
      </div>
    </div>
  )
}

export default OwnerEmployeesPage
