import { getStoredAccessToken } from './authApi'
import type { PagedResult } from '../types/services'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

interface ApiErrorBody {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

export interface OwnerInventoryItemDto {
  itemId: number
  name: string
  quantity: number
  minimumQuantity: number
  status: string
  updatedAt: string
  reportedByEmployeeName?: string | null
  reportedAt?: string | null
  reportNote?: string | null
}

export class InventoryApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'InventoryApiError'
    this.status = status
  }
}

function getAuthorizedHeaders(): Record<string, string> {
  const token = getStoredAccessToken()

  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function readErrorMessage(response: Response) {
  if (response.status === 403) {
    return 'Nemate dozvolu za pregled inventara.'
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

async function request<TResponse>(path: string): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...getAuthorizedHeaders(),
    },
  })

  if (!response.ok) {
    throw new InventoryApiError(await readErrorMessage(response), response.status)
  }

  return (await response.json()) as TResponse
}

export function getOwnerInventory() {
  return request<PagedResult<OwnerInventoryItemDto>>(
    '/owner/inventory?getAll=true&includeTotalCount=true',
  )
}
