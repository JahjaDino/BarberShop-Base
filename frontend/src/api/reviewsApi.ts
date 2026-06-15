import { getStoredAccessToken } from './authApi'
import type { PagedResult } from '../types/services'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

export interface ReviewDto {
  id: number
  appointmentId: number
  rating: number
  comment?: string | null
  createdAt: string
  serviceName: string
  clientName: string
  employeeName: string
  appointmentStartTime: string
}

export interface PendingReviewDto {
  appointmentId: number
  serviceName: string
  employeeName: string
  appointmentDate: string
}

export interface ReviewInsertRequest {
  appointmentId: number
  rating: number
  comment?: string | null
}

interface ApiErrorBody {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

export class ReviewsApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ReviewsApiError'
    this.status = status
  }
}

function getAuthorizedHeaders(): Record<string, string> {
  const token = getStoredAccessToken()

  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function readErrorMessage(response: Response) {
  if (response.status === 403) {
    return 'Nemate dozvolu za ovu recenziju.'
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
    throw new ReviewsApiError(await readErrorMessage(response), response.status)
  }

  return (await response.json()) as TResponse
}

export function getMyReviews() {
  return request<PagedResult<ReviewDto>>('/reviews/my?getAll=true')
}

export function getOwnerReviews() {
  return request<PagedResult<ReviewDto>>('/reviews?getAll=true')
}

export function getPendingReviews() {
  return request<PendingReviewDto[]>('/reviews/pending')
}

export function createReview(requestBody: ReviewInsertRequest) {
  return request<ReviewDto>('/reviews', {
    method: 'POST',
    body: JSON.stringify(requestBody),
  })
}
