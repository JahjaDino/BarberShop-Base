import { NavLink } from 'react-router-dom'
import type { NavigationGroup } from '../../types/navigation'

interface AppSidebarProps {
  navigationGroups: NavigationGroup[]
  workspaceLabel: string
}

function AppSidebar({ navigationGroups, workspaceLabel }: AppSidebarProps) {
  return (
    <aside className="sticky top-0 z-30 flex w-full min-w-0 flex-col border-b border-amber-200/10 bg-black/60 p-4 backdrop-blur-xl lg:fixed lg:left-0 lg:top-0 lg:z-20 lg:h-screen lg:w-[280px] lg:border-b-0 lg:border-r lg:bg-black/35 lg:p-5">
      <div className="shrink-0">
        <div className="flex min-w-0 items-center gap-3">
          <span className="grid h-11 w-11 place-items-center rounded-2xl border border-amber-200/25 bg-amber-100/10 text-lg font-black text-amber-100 shadow-[0_0_30px_rgba(245,213,145,0.12)]">
            MM
          </span>
          <span className="min-w-0">
            <span className="block truncate text-lg font-semibold tracking-wide text-stone-50">
              Hair Studio MIMI
            </span>
            <span className="block truncate text-xs uppercase tracking-[0.2em] text-amber-200/70 lg:tracking-[0.28em]">
              {workspaceLabel}
            </span>
          </span>
        </div>
      </div>

      <nav className="app-scrollbar mt-4 flex min-w-0 gap-3 overflow-x-auto pb-1 lg:mt-8 lg:grid lg:flex-1 lg:content-start lg:gap-4 lg:overflow-visible lg:pb-0">
        {navigationGroups.map((group) => (
          <div key={group.label} className="shrink-0 lg:shrink">
            <p className="px-3 text-xs font-semibold uppercase tracking-[0.16em] text-amber-200/55 lg:tracking-[0.2em]">
              {group.label}
            </p>
            <div className="mt-2 flex gap-1 lg:grid">
              {group.items.map((item) => (
                <NavLink
                  key={item.path}
                  to={item.path}
                  className={({ isActive }) =>
                    `inline-flex whitespace-nowrap rounded-2xl border px-4 py-2.5 text-sm font-medium transition-all duration-300 lg:flex lg:whitespace-normal ${
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

      <div className="mt-5 hidden shrink-0 border-t border-amber-200/10 pt-5 text-xs leading-5 text-stone-500 lg:block">
        Hair Studio MIMI aplikacija
      </div>
    </aside>
  )
}

export default AppSidebar
