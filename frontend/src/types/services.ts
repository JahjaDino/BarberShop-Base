export interface PagedResult<TItem> {
  items: TItem[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface ServiceCategoryDto {
  id: number
  shopId: number
  name: string
  description?: string | null
  active: boolean
}

export interface ServiceDto {
  id: number
  shopId: number
  categoryId: number
  categoryName: string
  name: string
  description?: string | null
  durationMinutes: number
  price: number
  allowOverlap: boolean
  maxParallelAppointments: number
  active: boolean
}

export interface OwnerServiceListItem {
  serviceId: number
  categoryId: number
  name: string
  description?: string | null
  categoryName: string
  durationMinutes: number
  price: number
  allowOverlap: boolean
  maxParallelAppointments: number
  bookingsCount: number
  isActive: boolean
}

export interface ServiceSearchParams {
  shopId?: number
  categoryId?: number
  name?: string
  active?: boolean
  minPrice?: number
  maxPrice?: number
  getAll?: boolean
  page?: number
  pageSize?: number
}

export interface ServiceCategorySearchParams {
  name?: string
  active?: boolean
  getAll?: boolean
  page?: number
  pageSize?: number
}

export interface ServiceCreateRequest {
  shopId: number
  categoryId: number
  name: string
  description?: string | null
  durationMinutes: number
  price: number
  allowOverlap?: boolean
  maxParallelAppointments?: number
}

export interface ServiceUpdateRequest extends ServiceCreateRequest {
  active: boolean
}

export interface OwnerServiceCategoryCreateRequest {
  name: string
  description?: string | null
}

export interface OwnerServiceCategoryUpdateRequest
  extends OwnerServiceCategoryCreateRequest {
  active: boolean
}

export interface OwnerServiceCreateRequest {
  categoryId: number
  name: string
  description?: string | null
  durationMinutes: number
  price: number
  allowOverlap: boolean
  maxParallelAppointments: number
}

export interface OwnerServiceUpdateRequest extends OwnerServiceCreateRequest {
  active: boolean
}
