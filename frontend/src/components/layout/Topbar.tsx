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
    <header className="border-b border-amber-200/10 px-5 py-3.5 backdrop-blur-xl sm:px-8 lg:px-10">
      <div className="flex flex-col gap-3 xl:flex-row xl:items-center xl:justify-between">
        <div className="min-w-0">
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
            Classic Cuts
          </p>
          <h1 className="mt-2 text-2xl font-semibold text-stone-50">
            {metadata.title}
          </h1>
          <p className="mt-1 max-w-3xl text-sm leading-6 text-stone-400">
            {metadata.subtitle}
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2.5">
          <div className="flex items-center gap-2.5 rounded-2xl border border-emerald-300/15 bg-emerald-950/30 px-3 py-2">
            <span className="grid h-9 w-9 place-items-center rounded-full border border-amber-200/35 bg-amber-100/15 text-xs font-black text-amber-100">
              {getInitials(user)}
            </span>
            <span>
              <span className="block text-sm font-semibold text-stone-50">
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
