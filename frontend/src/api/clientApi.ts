import { getStoredAccessToken } from './authApi'
import type { ClientAppointmentCardDto } from './appointmentsApi'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

export interface ClientServiceCardDto {
  serviceId: number
  name: string
  description?: string | null
  categoryName: string
  durationMinutes: number
  price: number
}

export interface FavoriteServiceDto extends ClientServiceCardDto {
  createdAt: string
}

export interface ClientDashboardDto {
  nextAppointment?: ClientAppointmentCardDto | null
  popularServices: ClientServiceCardDto[]
  unreadNotificationsCount: number
}

interface ApiErrorBody {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

const backendMessageTranslations: Record<string, string> = {
  'Service does not exist.': 'Usluga ne postoji.',
  'Service is not active.': 'Usluga nije aktivna.',
  'Service is already in favorites.': 'Usluga je već u omiljenim.',
  'Current user does not have a client profile.':
    'Trenutni korisnik nema korisnički profil.',
  'Shop does not exist.': 'Salon ne postoji.',
  'Service does not belong to the selected shop.':
    'Usluga ne pripada odabranom salonu.',
}

function translateMessage(message: string) {
  const trimmedMessage = message.trim()
  return backendMessageTranslations[trimmedMessage] ?? trimmedMessage
}

export class ClientApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ClientApiError'
    this.status = status
  }
}

function getAuthorizedHeaders(): Record<string, string> {
  const token = getStoredAccessToken()

  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function readErrorMessage(response: Response) {
  if (response.status === 403) {
    return 'Nemate pristup korisničkom prostoru.'
  }

  try {
    const body = (await response.json()) as ApiErrorBody
    const validationMessage = body.errors
      ? Object.values(body.errors).flat().join(' ')
      : ''

    return translateMessage(
      validationMessage ||
        body.message ||
        body.title ||
        'Zahtjev nije uspio. Pokušajte ponovo kasnije.',
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
    throw new ClientApiError(await readErrorMessage(response), response.status)
  }

  if (response.status === 204) {
    return undefined as TResponse
  }

  return (await response.json()) as TResponse
}

export function getClientDashboard() {
  return request<ClientDashboardDto>('/client/dashboard')
}

export function getFavoriteServices() {
  return request<FavoriteServiceDto[]>('/client/favorite-services')
}

export function addFavoriteService(serviceId: number) {
  return request<FavoriteServiceDto>(`/client/favorite-services/${serviceId}`, {
    method: 'POST',
  })
}

export function removeFavoriteService(serviceId: number) {
  return request<void>(`/client/favorite-services/${serviceId}`, {
    method: 'DELETE',
  })
}
