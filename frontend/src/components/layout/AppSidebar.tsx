import { NavLink } from 'react-router-dom'
import type { NavigationGroup } from '../../types/navigation'

interface AppSidebarProps {
  navigationGroups: NavigationGroup[]
  workspaceLabel: string
}

function AppSidebar({ navigationGroups, workspaceLabel }: AppSidebarProps) {
  return (
    <aside className="flex w-full flex-col border-b border-amber-200/10 bg-black/35 p-5 backdrop-blur-xl lg:fixed lg:left-0 lg:top-0 lg:z-20 lg:h-screen lg:w-[280px] lg:border-b-0 lg:border-r">
      <div className="shrink-0">
        <div className="flex items-center gap-3">
          <span className="grid h-11 w-11 place-items-center rounded-2xl border border-amber-200/25 bg-amber-100/10 text-lg font-black text-amber-100 shadow-[0_0_30px_rgba(245,213,145,0.12)]">
            CC
          </span>
          <span>
            <span className="block text-lg font-semibold tracking-wide text-stone-50">
              Classic Cuts
            </span>
            <span className="block text-xs uppercase tracking-[0.28em] text-amber-200/70">
              {workspaceLabel}
            </span>
          </span>
        </div>
      </div>

      <nav className="mt-8 grid flex-1 content-start gap-4">
        {navigationGroups.map((group) => (
          <div key={group.label}>
            <p className="px-3 text-xs font-semibold uppercase tracking-[0.2em] text-amber-200/55">
              {group.label}
            </p>
            <div className="mt-2 grid gap-1">
              {group.items.map((item) => (
                <NavLink
                  key={item.path}
                  to={item.path}
                  className={({ isActive }) =>
                    `rounded-2xl border px-4 py-2.5 text-sm font-medium transition-all duration-300 ${
                      isActive
                        ? 'border-amber-200/35 bg-amber-100/10 text-amber-100 shadow-[0_0_28px_rgba(245,213,145,0.08)]'
                        : 'border-transparent text-stone-300 hover:border-amber-200/25 hover:bg-amber-100/10 hover:text-amber-100'
                    }`
                  }
                >
                  {item.label}
                </NavLink>
              ))}
            </div>
          </div>
        ))}
      </nav>

      <div className="mt-5 shrink-0 border-t border-amber-200/10 pt-5 text-xs leading-5 text-stone-500">
        Classic Cuts aplikacija
      </div>
    </aside>
  )
}

export default AppSidebar
