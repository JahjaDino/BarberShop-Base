import { getStoredAccessToken } from './authApi'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

interface ApiErrorBody {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

export interface OwnerAnalyticsServiceDto {
  serviceId: number
  name: string
  appointmentsCount: number
}

export interface OwnerAnalyticsEmployeeDto {
  employeeId: number
  fullName: string
  appointmentsCount: number
}

export interface OwnerAnalyticsDto {
  totalAppointmentsCount: number
  todayAppointmentsCount: number
  pendingAppointmentsCount: number
  confirmedAppointmentsCount: number
  completedAppointmentsCount: number
  cancelledAppointmentsCount: number
  todayRevenue: number
  monthRevenue: number
  activeEmployeesCount: number
  activeServicesCount: number
  occupancyRate: number
  returningClientsRate: number
  mostPopularService?: string | null
  mostPopularServiceAppointmentsThisMonth: number
  topServices: OwnerAnalyticsServiceDto[]
  mostActiveEmployees: OwnerAnalyticsEmployeeDto[]
}

export class OwnerAnalyticsApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'OwnerAnalyticsApiError'
    this.status = status
  }
}

function getAuthorizedHeaders(): Record<string, string> {
  const token = getStoredAccessToken()

  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function readErrorMessage(response: Response) {
  if (response.status === 403) {
    return 'Nemate pristup vlasničkom prostoru.'
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
    throw new OwnerAnalyticsApiError(
      await readErrorMessage(response),
      response.status,
    )
  }

  return (await response.json()) as TResponse
}

export function getOwnerAnalytics() {
  return request<OwnerAnalyticsDto>('/owner/analytics')
}
