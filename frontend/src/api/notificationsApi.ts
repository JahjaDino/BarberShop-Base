import { getStoredAccessToken } from './authApi'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

interface ApiErrorBody {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

interface PagedResult<TItem> {
  items: TItem[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface NotificationDto {
  id: number
  userId: number
  appointmentId?: number | null
  timeOffId?: number | null
  type: string
  title: string
  message?: string | null
  status: string
  sentAt: string
}

export class NotificationsApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'NotificationsApiError'
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
  try {
    const body = (await response.json()) as ApiErrorBody
    const validationMessage = body.errors
      ? Object.values(body.errors).flat().join(' ')
      : ''

    return (
      validationMessage ||
      body.message ||
      body.title ||
      'Notifikacije trenutno nisu dostupne.'
    )
  } catch {
    return 'Notifikacije trenutno nisu dostupne.'
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
    throw new NotificationsApiError(
      await readErrorMessage(response),
      response.status,
    )
  }

  if (response.status === 204) {
    return undefined as TResponse
  }

  return (await response.json()) as TResponse
}

export function getNotifications(status?: string) {
  return request<PagedResult<NotificationDto>>(
    appendSearchParams('/notifications/my', {
      status,
      getAll: true,
      includeTotalCount: true,
    }),
  )
}

export function markNotificationAsRead(notificationId: number) {
  return request<NotificationDto>(`/notifications/${notificationId}/read`, {
    method: 'PATCH',
  })
}

export function markAllNotificationsAsRead() {
  return request<{ updatedCount: number }>('/notifications/read-all', {
    method: 'PATCH',
  })
}

export function deleteNotification(notificationId: number) {
  return request<void>(`/notifications/${notificationId}`, {
    method: 'DELETE',
  })
}

export function deleteReadNotifications() {
  return request<{ deletedCount: number }>('/notifications/read', {
    method: 'DELETE',
  })
}
