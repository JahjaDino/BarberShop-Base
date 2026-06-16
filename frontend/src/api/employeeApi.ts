import { getStoredAccessToken } from './authApi'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

interface ApiErrorBody {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

export interface EmployeeServiceDto {
  serviceId: number
  name: string
  description?: string | null
  categoryName: string
  durationMinutes: number
  price: number
}

export interface EmployeeScheduleWorkingHoursDto {
  startTime: string
  endTime: string
}

export interface EmployeeScheduleItemDto {
  time: string
  title: string
  description?: string | null
  clientName?: string | null
  appointmentId?: number | null
  type: string
  status?: string | null
}

export interface EmployeeDashboardSummaryDto {
  todayAppointmentsCount: number
  confirmedTodayAppointmentsCount: number
  nextAppointmentTime?: string | null
  nextAppointmentServiceName?: string | null
  workingHoursToday: EmployeeScheduleWorkingHoursDto[]
  dayStatus: string
}

export interface EmployeeAssignedAppointmentDto {
  appointmentId: number
  clientName: string
  serviceName: string
  time: string
  status: string
}

export interface EmployeeTimeOffSummaryDto {
  hasActiveTimeOff: boolean
  activeTimeOffCount: number
  pendingTimeOffCount: number
  nextTimeOffDate?: string | null
}

export interface EmployeeDashboardDto {
  summary: EmployeeDashboardSummaryDto
  todaySchedule: EmployeeScheduleItemDto[]
  assignedAppointments: EmployeeAssignedAppointmentDto[]
  timeOffSummary: EmployeeTimeOffSummaryDto
}

export interface EmployeeScheduleDto {
  date: string
  workingHours: EmployeeScheduleWorkingHoursDto[]
  items: EmployeeScheduleItemDto[]
}

export interface EmployeeTimeOffDto {
  timeOffId: number
  startDate: string
  endDate: string
  reason?: string | null
  status: string
  reviewedAt?: string | null
  reviewNote?: string | null
  reviewedByName?: string | null
}

export interface EmployeeTimeOffCreateRequest {
  startTime: string
  endTime: string
  reason?: string | null
}

export interface EmployeeProfileDto {
  employeeId: number
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  position: string
  shopName: string
  workingHoursSummary: Array<{
    dayOfWeek: number
    startTime: string
    endTime: string
    active: boolean
  }>
}

export interface EmployeeInventoryItemDto {
  itemId: number
  name: string
  quantity: number
  unit: string
  minimumQuantity: number
  status: string
  lastUpdated: string
  reportNote?: string | null
}

export interface EmployeeInventoryReportRequest {
  name: string
  quantity: number
  minimumQuantity: number
  unit?: string | null
  status?: string | null
  note?: string | null
}

export class EmployeeApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'EmployeeApiError'
    this.status = status
  }
}

function getAuthorizedHeaders(): Record<string, string> {
  const token = getStoredAccessToken()

  return token ? { Authorization: `Bearer ${token}` } : {}
}

function appendSearchParams(
  path: string,
  params?: Record<string, string | number | boolean | undefined>,
) {
  const searchParams = new URLSearchParams()

  Object.entries(params ?? {}).forEach(([key, value]) => {
    if (value !== undefined && value !== '') {
      searchParams.set(key, String(value))
    }
  })

  const query = searchParams.toString()
  return query ? `${path}?${query}` : path
}

async function readErrorMessage(response: Response) {
  if (response.status === 403) {
    return 'Nemate pristup studio prostoru.'
  }

  try {
    const body = (await response.json()) as ApiErrorBody
    const validationMessage = body.errors
      ? Object.values(body.errors).flat().join(' ')
      : ''

    return (
      validationMessage ||
      body.message ||
      body.title ||
      'Zahtjev nije uspio. Pokušajte ponovo kasnije.'
    )
  } catch {
    return 'Zahtjev nije uspio. Pokušajte ponovo kasnije.'
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
      ...getAuthorizedHeaders(),
      ...options.headers,
    },
  })

  if (!response.ok) {
    throw new EmployeeApiError(await readErrorMessage(response), response.status)
  }

  return (await response.json()) as TResponse
}

export function getEmployeeServices() {
  return request<EmployeeServiceDto[]>('/employee/services')
}

export function getEmployeeInventory() {
  return request<EmployeeInventoryItemDto[]>('/employee/inventory')
}

export function reportEmployeeInventory(
  requestBody: EmployeeInventoryReportRequest,
) {
  return request<EmployeeInventoryItemDto>('/employee/inventory', {
    method: 'POST',
    body: JSON.stringify(requestBody),
  })
}

export function getEmployeeDashboard(date?: string) {
  return request<EmployeeDashboardDto>(
    appendSearchParams('/employee/dashboard', { date }),
  )
}

export function getEmployeeSchedule(date?: string) {
  return request<EmployeeScheduleDto>(
    appendSearchParams('/employee/schedule', { date }),
  )
}

export function getEmployeeTimeOff() {
  return request<EmployeeTimeOffDto[]>('/employee/time-off')
}

export function createEmployeeTimeOff(requestBody: EmployeeTimeOffCreateRequest) {
  return request<EmployeeTimeOffDto>('/employee/time-off', {
    method: 'POST',
    body: JSON.stringify(requestBody),
  })
}

export function getEmployeeProfile() {
  return request<EmployeeProfileDto>('/employee/profile')
}
