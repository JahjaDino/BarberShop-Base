import { useEffect } from 'react'
import { Navigate, Outlet, useLocation, useNavigate } from 'react-router-dom'
import {
  clearAuthSession,
  getStoredAccessToken,
  logoutUser,
} from '../../api/authApi'
import { useAuthUser } from '../../hooks/useAuthUser'
import {
  getNavigationForRole,
  pageMetadata,
} from '../../routes/appNavigation'
import AppSidebar from './AppSidebar'
import Topbar from './Topbar'

function MainFrame() {
  const navigate = useNavigate()
  const location = useLocation()
  const { user, primaryRole } = useAuthUser()
  const hasToken = Boolean(getStoredAccessToken())
  const { navigationGroups, workspaceLabel } = getNavigationForRole(primaryRole)

  useEffect(() => {
    if (!hasToken) {
      navigate('/', { replace: true })
    }
  }, [hasToken, navigate])

  if (!hasToken) {
    return null
  }

  if (location.pathname === '/app') {
    return <Navigate to="/app/dashboard" replace />
  }

  const metadata =
    pageMetadata[location.pathname] ?? pageMetadata['/app/dashboard']

  const handleLogout = async () => {
    try {
      await logoutUser()
    } catch {
      clearAuthSession()
    } finally {
      navigate('/', { replace: true })
    }
  }

  return (
    <main className="min-h-screen overflow-x-hidden bg-[#050b1a] text-stone-100">
      <div className="min-h-screen min-w-0 lg:pl-[280px]">
        <AppSidebar
          navigationGroups={navigationGroups}
          workspaceLabel={workspaceLabel}
        />

        <section className="relative min-w-0 overflow-hidden">
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_18%_8%,rgba(245,213,145,0.14),transparent_28%),radial-gradient(circle_at_80%_18%,rgba(30,64,175,0.18),transparent_26%),linear-gradient(135deg,#050b1a_0%,#071126_46%,#020617_100%)]" />
          <div className="absolute inset-0 bg-[linear-gradient(rgba(255,255,255,0.03)_1px,transparent_1px),linear-gradient(90deg,rgba(255,255,255,0.03)_1px,transparent_1px)] bg-[size:64px_64px] opacity-20" />

          <div className="relative flex min-h-screen min-w-0 flex-col">
            <Topbar
              metadata={metadata}
              user={user}
              onLogout={handleLogout}
            />
            <div className="min-w-0 flex-1 px-4 py-5 sm:px-8 sm:py-7 lg:px-10">
              <Outlet />
            </div>
          </div>
        </section>
      </div>
    </main>
  )
}

export default MainFrame
