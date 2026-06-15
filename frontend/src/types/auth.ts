export type UserRole = 'CLIENT' | 'EMPLOYEE' | 'OWNER' | 'ADMIN' | string

export interface AppUser {
  id: number
  email: string
  firstName: string
  lastName: string
  phoneNumber: string
  status: string
  roles: UserRole[]
}
