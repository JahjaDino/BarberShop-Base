import type { AppUser, UserRole } from '../types/auth'

export function normalizeRole(role: UserRole) {
  return role.trim().toUpperCase()
}

export function hasRole(user: AppUser | null, role: UserRole) {
  if (!user) {
    return false
  }

  const expectedRole = normalizeRole(role)
  return user.roles.some((userRole) => normalizeRole(userRole) === expectedRole)
}

export function getPrimaryRole(user: AppUser | null) {
  if (!user || user.roles.length === 0) {
    return 'CLIENT'
  }

  if (hasRole(user, 'OWNER')) {
    return 'OWNER'
  }

  if (hasRole(user, 'EMPLOYEE')) {
    return 'EMPLOYEE'
  }

  if (hasRole(user, 'CLIENT')) {
    return 'CLIENT'
  }

  return 'CLIENT'
}
