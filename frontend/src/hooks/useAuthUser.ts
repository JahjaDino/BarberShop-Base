import { useEffect, useState } from 'react'
import { AUTH_USER_UPDATED_EVENT, getStoredUser } from '../api/authApi'
import type { AppUser } from '../types/auth'
import { getPrimaryRole, hasRole } from '../utils/roles'

export function useAuthUser() {
  const [user, setUser] = useState<AppUser | null>(() => getStoredUser())
  const primaryRole = getPrimaryRole(user)

  useEffect(() => {
    function refreshUser() {
      setUser(getStoredUser())
    }

    window.addEventListener(AUTH_USER_UPDATED_EVENT, refreshUser)
    window.addEventListener('storage', refreshUser)

    return () => {
      window.removeEventListener(AUTH_USER_UPDATED_EVENT, refreshUser)
      window.removeEventListener('storage', refreshUser)
    }
  }, [])

  return {
    user,
    primaryRole,
    isClient: hasRole(user, 'CLIENT') || !user,
    hasRole: (role: string) => hasRole(user, role),
  }
}
