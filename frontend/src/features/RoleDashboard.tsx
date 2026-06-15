import { Navigate } from 'react-router-dom'
import ClientDashboard from './client/ClientDashboard'
import { useAuthUser } from '../hooks/useAuthUser'

function RoleDashboard() {
  const { primaryRole } = useAuthUser()

  if (primaryRole === 'OWNER') {
    return <Navigate to="/app/owner/dashboard" replace />
  }

  if (primaryRole === 'EMPLOYEE') {
    return <Navigate to="/app/employee/dashboard" replace />
  }

  return <ClientDashboard />
}

export default RoleDashboard
