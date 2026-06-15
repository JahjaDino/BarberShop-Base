import { getStoredAccessToken } from './authApi'
import type { PagedResult } from '../types/services'
import type { AppointmentStatus } from './appointmentsApi'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

export type PaymentStatus = 'PENDING' | 'PAID' | 'CANCELLED'

export interface OwnerPaymentListItemDto {
  appointmentId: number
  date: string
  time: string
  clientName: string
  employeeName: string
  serviceName: string
  amount: number
  paymentMethod?: string | null
  paymentStatus?: PaymentStatus | string | null
  appointmentStatus: AppointmentStatus
}

interface ApiErrorBody {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

export class OwnerPaymentsApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'OwnerPaymentsApiError'
    this.status = status
  }
}

function getAuthorizedHeaders(): Record<string, string> {
  const token = getStoredAccessToken()

  return token ? { Authorization: `Bearer ${token}` } : {}
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
      ? Object.values(body.errors).flat().join(' ')
      : ''

    return (
      validationMessage ||
      body.message ||
      body.title ||
      'Plaćanja trenutno nisu dostupna.'
    )
  } catch {
    return 'Plaćanja trenutno nisu dostupna.'
  }
}

async function request<TResponse>(path: string): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...getAuthorizedHeaders(),
    },
  })

  if (!response.ok) {
    throw new OwnerPaymentsApiError(await readErrorMessage(response), response.status)
  }

  return (await response.json()) as TResponse
}

export function getOwnerPayments() {
  return request<PagedResult<OwnerPaymentListItemDto>>(
    '/owner/payments?getAll=true&includeTotalCount=true',
  )
}
