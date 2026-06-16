import { getStoredAccessToken } from './authApi'
import type {
  CreateEmployeeRequest,
  OwnerEmployee,
  PublicEmployee,
} from '../types/employee'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

interface ApiErrorBody {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

const backendMessageTranslations: Record<string, string> = {
  'User with the same email already exists.':
    'Korisnik sa istim emailom vec postoji.',
  'Required EMPLOYEE role does not exist.':
    'Rola za frizerku nije podesena na serveru.',
  'Current user does not have access to an owner shop.':
    'Nemate pristup vlasnickom salonu.',
  'Working hours overlap with existing working hours.':
    'Radno vrijeme se preklapa sa postojećim radnim vremenom.',
  'Start time must be before end time.':
    'Početak radnog vremena mora biti prije kraja.',
  'The request field is required.': 'Podaci za radno vrijeme su obavezni.',
  'The JSON value could not be converted to System.TimeOnly.':
    'Vrijeme mora biti u formatu HH:mm.',
  'Day of week is required.': 'Dan u sedmici je obavezan.',
  'Day of week must be between 0 and 6.':
    'Dan u sedmici nije validan.',
  'Employee does not belong to your shop.':
    'Frizerka ne pripada vašem salonu.',
  'Employee does not exist.': 'Frizerka ne postoji.',
  'Employee is not active.': 'Frizerka nije aktivna.',
}

export class EmployeesApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'EmployeesApiError'
    this.status = status
  }
}

export interface WorkingHourDto {
  id: number
  employeeId: number
  dayOfWeek: number
  startTime: string
  endTime: string
  active: boolean
}

export interface WorkingHourCreateRequest {
  employeeId: number
  dayOfWeek: number
  startTime: string
  endTime: string
}

export interface WorkingHourUpdateRequest extends WorkingHourCreateRequest {
  active: boolean
}

interface PagedResult<TItem> {
  items: TItem[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

function getAuthHeaders(): Record<string, string> {
  const token = getStoredAccessToken()

  return token ? { Authorization: `Bearer ${token}` } : {}
}

export function normalizeTimeForApi(value: string): string {
  if (!value) return ''

  const trimmedValue = value.trim()

  if (/^\d{2}:\d{2}$/.test(trimmedValue)) return trimmedValue

  if (/^\d{2}:\d{2}:\d{2}$/.test(trimmedValue)) {
    return trimmedValue.slice(0, 5)
  }

  const twelveHourMatch = trimmedValue.match(
    /^(\d{1,2}):(\d{2})\s?(AM|PM)$/i,
  )

  if (twelveHourMatch) {
    const [, hourPart, minutePart, period] = twelveHourMatch
    let hour = Number(hourPart)

    if (period.toUpperCase() === 'PM' && hour < 12) {
      hour += 12
    }

    if (period.toUpperCase() === 'AM' && hour === 12) {
      hour = 0
    }

    return `${String(hour).padStart(2, '0')}:${minutePart}`
  }

  return trimmedValue
}

export function normalizeTime(value: string) {
  const apiTime = normalizeTimeForApi(value)
  const twentyFourHourMatch = apiTime.match(/^(\d{1,2}):(\d{2})$/)

  if (twentyFourHourMatch) {
    const [, hourPart, minutePart] = twentyFourHourMatch
    const hour = Number(hourPart)
    const minute = Number(minutePart)

    if (hour < 0 || hour > 23 || minute < 0 || minute > 59) return ''

    return `${String(hour).padStart(2, '0')}:${minutePart}`
  }

  return ''
}

function normalizeWorkingHourCreatePayload(
  payload: WorkingHourCreateRequest,
): WorkingHourCreateRequest {
  const startTime = normalizeTimeForApi(payload.startTime)
  const endTime = normalizeTimeForApi(payload.endTime)

  if (
    !/^\d{2}:\d{2}$/.test(startTime) ||
    !/^\d{2}:\d{2}$/.test(endTime)
  ) {
    throw new EmployeesApiError('Vrijeme mora biti u formatu HH:mm.', 400)
  }

  return {
    ...payload,
    startTime,
    endTime,
  }
}

function normalizeWorkingHourUpdatePayload(
  payload: WorkingHourUpdateRequest,
): WorkingHourUpdateRequest {
  const startTime = normalizeTimeForApi(payload.startTime)
  const endTime = normalizeTimeForApi(payload.endTime)

  if (
    !/^\d{2}:\d{2}$/.test(startTime) ||
    !/^\d{2}:\d{2}$/.test(endTime)
  ) {
    throw new EmployeesApiError('Vrijeme mora biti u formatu HH:mm.', 400)
  }

  return {
    ...payload,
    startTime,
    endTime,
  }
}

async function readErrorMessage(response: Response) {
  if (response.status === 403) {
    return 'Nemate dozvolu za upravljanje frizerkama ovog salona.'
  }

  try {
    const body = (await response.json()) as ApiErrorBody
    const validationMessage = body.errors
      ? Object.values(body.errors).flat().join(' ')
      : ''
    const message =
      validationMessage ||
      body.message ||
      body.title ||
      'Greska prilikom komunikacije sa serverom.'

    const timeOnlyError = message.includes('System.TimeOnly')

    return timeOnlyError
      ? 'Vrijeme mora biti u formatu HH:mm.'
      : backendMessageTranslations[message] ?? message
  } catch {
    return 'Greska prilikom komunikacije sa serverom.'
  }
}

async function request<TResponse>(
  path: string,
  options: RequestInit = {},
): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  if (!response.ok) {
    throw new EmployeesApiError(await readErrorMessage(response), response.status)
  }

  return (await response.json()) as TResponse
}

export function getEmployees() {
  return request<OwnerEmployee[]>('/owner/employees', {
    headers: getAuthHeaders(),
  })
}

export function createEmployee(payload: CreateEmployeeRequest) {
  return request<OwnerEmployee>('/owner/employees', {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(payload),
  })
}

export function activateEmployee(employeeId: number) {
  return request<OwnerEmployee>(`/owner/employees/${employeeId}/activate`, {
    method: 'PATCH',
    headers: getAuthHeaders(),
  })
}

export function deactivateEmployee(employeeId: number) {
  return request<OwnerEmployee>(`/owner/employees/${employeeId}/deactivate`, {
    method: 'PATCH',
    headers: getAuthHeaders(),
  })
}

export function getPublicEmployees(shopId: number) {
  return request<PublicEmployee[]>(`/public/shops/${shopId}/employees`)
}

export function getEmployeeWorkingHours(employeeId: number) {
  return request<PagedResult<WorkingHourDto>>(
    `/employees/${employeeId}/working-hours?getAll=true&includeTotalCount=true`,
    {
      headers: getAuthHeaders(),
    },
  )
}

export function createWorkingHour(payload: WorkingHourCreateRequest) {
  const normalizedPayload = normalizeWorkingHourCreatePayload(payload)
  console.log('Working hours payload', normalizedPayload)

  return request<WorkingHourDto>('/working-hours', {
    method: 'POST',
    headers: getAuthHeaders(),
    body: JSON.stringify(normalizedPayload),
  })
}

export function updateWorkingHour(
  workingHourId: number,
  payload: WorkingHourUpdateRequest,
) {
  const normalizedPayload = normalizeWorkingHourUpdatePayload(payload)
  console.log('Working hours payload', normalizedPayload)

  return request<WorkingHourDto>(`/working-hours/${workingHourId}`, {
    method: 'PUT',
    headers: getAuthHeaders(),
    body: JSON.stringify(normalizedPayload),
  })
}
