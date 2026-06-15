import { getStoredAccessToken } from './authApi'
import type { PagedResult } from '../types/services'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

export type OwnerTimeOffStatus = 'PENDING' | 'APPROVED' | 'REJECTED'

export interface OwnerTimeOffRequestDto {
  timeOffId: number
  employeeId: number
  employeeName: string
  startDate: string
  endDate: string
  reason?: string | null
  status: OwnerTimeOffStatus
  reviewedAt?: string | null
  reviewNote?: string | null
}

interface ApiErrorBody {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

class OwnerTimeOffApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'OwnerTimeOffApiError'
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
    return 'Nemate dozvolu za upravljanje zahtjevima za odsustvo.'
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
    throw new OwnerTimeOffApiError(await readErrorMessage(response), response.status)
  }

  return (await response.json()) as TResponse
}

export function getOwnerTimeOffRequests(status?: OwnerTimeOffStatus) {
  return request<PagedResult<OwnerTimeOffRequestDto>>(
    appendSearchParams('/owner/time-off-requests', {
      status,
      getAll: true,
      includeTotalCount: true,
    }),
  )
}

export function approveOwnerTimeOffRequest(
  timeOffId: number,
  reviewNote?: string,
) {
  return request<OwnerTimeOffRequestDto>(
    `/owner/time-off-requests/${timeOffId}/approve`,
    {
      method: 'PATCH',
      body: JSON.stringify({ reviewNote: reviewNote?.trim() || null }),
    },
  )
}

export function rejectOwnerTimeOffRequest(
  timeOffId: number,
  reviewNote?: string,
) {
  return request<OwnerTimeOffRequestDto>(
    `/owner/time-off-requests/${timeOffId}/reject`,
    {
      method: 'PATCH',
      body: JSON.stringify({ reviewNote: reviewNote?.trim() || null }),
    },
  )
}
