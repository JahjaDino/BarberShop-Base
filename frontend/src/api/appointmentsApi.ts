import { getStoredAccessToken } from './authApi'
import type { PagedResult } from '../types/services'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

export type AppointmentStatus =
  | 'PENDING'
  | 'CONFIRMED'
  | 'COMPLETED'
  | 'CANCELLED'
  | 'NO_SHOW'

export type AppointmentStatusUpdateType = 'CONFIRMED' | 'COMPLETED' | 'NO_SHOW'

export interface AvailableSlotDto {
  startTime: string
  endTime: string
  available: boolean
}

export interface AppointmentBookRequest {
  employeeId: number
  serviceIds: number[]
  startTime: string
  paymentMethod: 'CASH_ON_SITE'
  notes?: string | null
}

export interface AppointmentServiceDto {
  serviceId: number
  serviceName: string
  priceAtBooking: number
  durationAtBooking: number
}

export interface AppointmentDto {
  id: number
  clientId: number
  employeeId: number
  startTime: string
  endTime: string
  status: AppointmentStatus
  totalPrice: number
  paymentMethod?: string | null
  notes?: string | null
  createdAt: string
  services: AppointmentServiceDto[]
}

export interface ClientAppointmentCardDto {
  appointmentId: number
  serviceName: string
  employeeName: string
  date: string
  time: string
  durationMinutes: number
  price: number
  paymentMethod?: string | null
  status: AppointmentStatus
}

export interface EmployeeAppointmentListItemDto {
  appointmentId: number
  date: string
  time: string
  clientName: string
  serviceName: string
  durationMinutes: number
  price: number
  status: AppointmentStatus
}

export interface OwnerAppointmentListItemDto {
  appointmentId: number
  date: string
  time: string
  clientName: string
  employeeName: string
  serviceName: string
  durationMinutes: number
  price: number
  status: AppointmentStatus
}

interface ApiErrorBody {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

export class AppointmentsApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'AppointmentsApiError'
    this.status = status
  }
}

const backendMessageTranslations: Record<string, string> = {
  'Appointment filter is not valid.': 'Filter termina nije validan.',
  'Appointment status is not valid.': 'Status termina nije validan.',
  'Appointment cannot be booked in the past.':
    'Termin nije moguće zakazati u prošlosti.',
  'Employee is not available for the selected time.':
    'Frizerka nije dostupna u odabranom terminu.',
  'Selected time is outside working hours.':
    'Odabrano vrijeme je izvan radnog vremena.',
  'Appointment overlaps with another appointment.':
    'Odabrani termin se preklapa sa drugim terminom.',
  'Appointment is not in a cancellable state.':
    'Termin trenutno nije moguće otkazati.',
  'Appointment status transition is not allowed.':
    'Promjena statusa termina nije dozvoljena.',
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

function translateMessage(message: string) {
  const trimmedMessage = message.trim()
  return backendMessageTranslations[trimmedMessage] ?? trimmedMessage
}

async function readErrorMessage(response: Response) {
  if (response.status === 401) {
    return 'Morate biti prijavljeni za ovu akciju.'
  }

  if (response.status === 403) {
    return 'Nemate dozvolu za ovu akciju.'
  }

  try {
    const body = (await response.json()) as ApiErrorBody
    const validationMessage = body.errors
      ? Object.values(body.errors).flat().map(translateMessage).join(' ')
      : ''
    const message =
      validationMessage ||
      body.message ||
      body.title ||
      'Zahtjev nije uspio. Pokušajte ponovo kasnije.'

    return translateMessage(message)
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
    throw new AppointmentsApiError(await readErrorMessage(response), response.status)
  }

  return (await response.json()) as TResponse
}

export function getAvailableSlots(
  serviceId: number,
  employeeId: number,
  date: string,
) {
  return request<AvailableSlotDto[]>(
    appendSearchParams('/appointments/available-slots', {
      serviceId,
      employeeId,
      date,
    }),
  )
}

export function bookAppointment(requestBody: AppointmentBookRequest) {
  return request<AppointmentDto>('/appointments/book', {
    method: 'POST',
    body: JSON.stringify(requestBody),
  })
}

export function getClientAppointments(filter?: string) {
  return request<PagedResult<ClientAppointmentCardDto>>(
    appendSearchParams('/appointments/my', {
      filter,
      getAll: true,
      includeTotalCount: true,
    }),
  )
}

export function getEmployeeAppointments() {
  return request<PagedResult<EmployeeAppointmentListItemDto>>(
    appendSearchParams('/employee/appointments', {
      getAll: true,
      includeTotalCount: true,
    }),
  )
}

export function getOwnerAppointments() {
  return request<PagedResult<AppointmentDto>>(
    appendSearchParams('/appointments', {
      getAll: true,
      includeTotalCount: true,
    }),
  )
}

export function getOwnerPortalAppointments() {
  return request<PagedResult<OwnerAppointmentListItemDto>>(
    appendSearchParams('/owner/appointments', {
      getAll: true,
      includeTotalCount: true,
    }),
  )
}

export function updateAppointmentStatus(
  appointmentId: number,
  newStatus: AppointmentStatusUpdateType,
) {
  return request<AppointmentDto>(`/appointments/${appointmentId}/status`, {
    method: 'PATCH',
    body: JSON.stringify({ newStatus }),
  })
}

export function cancelAppointment(appointmentId: number, reason?: string) {
  return request<AppointmentDto>(`/appointments/${appointmentId}/cancel`, {
    method: 'PATCH',
    body: JSON.stringify({ reason }),
  })
}
