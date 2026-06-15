import { getStoredAccessToken } from './authApi'
import type {
  PagedResult,
  OwnerServiceCategoryCreateRequest,
  OwnerServiceCategoryUpdateRequest,
  OwnerServiceCreateRequest,
  OwnerServiceListItem,
  OwnerServiceUpdateRequest,
  ServiceCategoryDto,
  ServiceCategorySearchParams,
  ServiceCreateRequest,
  ServiceDto,
  ServiceSearchParams,
  ServiceUpdateRequest,
} from '../types/services'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

interface ApiErrorBody {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

export class ServicesApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ServicesApiError'
    this.status = status
  }
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
    return 'Nemate dozvolu za upravljanje uslugama ovog salona.'
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
      ...options.headers,
    },
  })

  if (!response.ok) {
    throw new ServicesApiError(await readErrorMessage(response), response.status)
  }

  if (response.status === 204) {
    return undefined as TResponse
  }

  return (await response.json()) as TResponse
}

function getAuthorizedHeaders(): Record<string, string> {
  const token = getStoredAccessToken()

  return token
    ? {
        Authorization: `Bearer ${token}`,
      }
    : {}
}

export function getServiceCategories(params: ServiceCategorySearchParams = {}) {
  return request<PagedResult<ServiceCategoryDto>>(
    appendSearchParams('/service-categories', {
      getAll: true,
      ...params,
    }),
  )
}

export function getServices(params: ServiceSearchParams = {}) {
  return request<PagedResult<ServiceDto>>(
    appendSearchParams('/services', {
      getAll: true,
      ...params,
    }),
  )
}

export function getOwnerServices(params: ServiceSearchParams = {}) {
  return request<PagedResult<OwnerServiceListItem>>(
    appendSearchParams('/owner/services', {
      getAll: true,
      ...params,
    }),
    {
      headers: getAuthorizedHeaders(),
    },
  )
}

export function getServiceById(id: number) {
  return request<ServiceDto>(`/services/${id}`)
}

export function createService(requestBody: ServiceCreateRequest) {
  return request<ServiceDto>('/services', {
    method: 'POST',
    headers: getAuthorizedHeaders(),
    body: JSON.stringify(requestBody),
  })
}

export function getOwnerServiceCategories() {
  return request<ServiceCategoryDto[]>('/owner/service-categories', {
    headers: getAuthorizedHeaders(),
  })
}

export function createOwnerServiceCategory(
  requestBody: OwnerServiceCategoryCreateRequest,
) {
  return request<ServiceCategoryDto>('/owner/service-categories', {
    method: 'POST',
    headers: getAuthorizedHeaders(),
    body: JSON.stringify(requestBody),
  })
}

export function updateOwnerServiceCategory(
  id: number,
  requestBody: OwnerServiceCategoryUpdateRequest,
) {
  return request<ServiceCategoryDto>(`/owner/service-categories/${id}`, {
    method: 'PUT',
    headers: getAuthorizedHeaders(),
    body: JSON.stringify(requestBody),
  })
}

export function createOwnerService(requestBody: OwnerServiceCreateRequest) {
  return request<ServiceDto>('/owner/services', {
    method: 'POST',
    headers: getAuthorizedHeaders(),
    body: JSON.stringify(requestBody),
  })
}

export function updateOwnerService(
  id: number,
  requestBody: OwnerServiceUpdateRequest,
) {
  return request<ServiceDto>(`/owner/services/${id}`, {
    method: 'PUT',
    headers: getAuthorizedHeaders(),
    body: JSON.stringify(requestBody),
  })
}

export function updateService(id: number, requestBody: ServiceUpdateRequest) {
  return request<ServiceDto>(`/services/${id}`, {
    method: 'PUT',
    headers: getAuthorizedHeaders(),
    body: JSON.stringify(requestBody),
  })
}

export function deleteService(id: number) {
  return request<void>(`/services/${id}`, {
    method: 'DELETE',
    headers: getAuthorizedHeaders(),
  })
}
