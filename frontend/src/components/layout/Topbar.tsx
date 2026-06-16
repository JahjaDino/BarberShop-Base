import type { AppUser } from '../../types/auth'
import type { PageMetadata } from '../../types/navigation'
import { buttonStyles } from '../common/buttonStyles'

interface TopbarProps {
  metadata: PageMetadata
  user: AppUser | null
  onLogout: () => void
}

function getInitials(user: AppUser | null) {
  if (!user) {
    return 'CC'
  }

  return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase()
}

function Topbar({ metadata, user, onLogout }: TopbarProps) {
  return (
    <header className="border-b border-amber-200/10 px-4 py-3.5 backdrop-blur-xl sm:px-8 lg:px-10">
      <div className="flex flex-col gap-3 xl:flex-row xl:items-center xl:justify-between">
        <div className="min-w-0">
          <p className="break-words text-xs font-semibold uppercase tracking-[0.2em] text-amber-200/70 sm:tracking-[0.28em]">
            Classic Cuts
          </p>
          <h1 className="mt-2 break-words text-xl font-semibold text-stone-50 sm:text-2xl">
            {metadata.title}
          </h1>
          <p className="mt-1 max-w-3xl break-words text-sm leading-6 text-stone-400">
            {metadata.subtitle}
          </p>
        </div>

        <div className="flex min-w-0 flex-col gap-2.5 sm:flex-row sm:flex-wrap sm:items-center">
          <div className="flex min-w-0 items-center gap-2.5 rounded-2xl border border-emerald-300/15 bg-emerald-950/30 px-3 py-2">
            <span className="grid h-9 w-9 place-items-center rounded-full border border-amber-200/35 bg-amber-100/15 text-xs font-black text-amber-100">
              {getInitials(user)}
            </span>
            <span className="min-w-0">
              <span className="block truncate text-sm font-semibold text-stone-50">
                {user ? `${user.firstName} ${user.lastName}` : 'Korisnik'}
              </span>
            </span>
          </div>
          <button
            type="button"
            onClick={onLogout}
            className={buttonStyles.ghost}
          >
            Odjavi se
          </button>
        </div>
      </div>
    </header>
  )
}

export default Topbar
