export interface OwnerEmployee {
  employeeId: number
  fullName: string
  position: string
  email: string
  phoneNumber: string
  active: boolean
  availabilityStatus: string
  appointmentsCountToday: number
  averageRating?: number | null
}

export interface CreateEmployeeRequest {
  firstName: string
  lastName: string
  email: string
  phoneNumber: string
  password: string
  position: string
  bio?: string | null
  employmentDate?: string
}

export interface PublicEmployee {
  employeeId: number
  fullName: string
  specialization: string
  rating?: number | null
}
