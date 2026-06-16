import type { ComponentPropsWithoutRef, ElementType, ReactNode } from 'react'

type AppCardVariant = 'default' | 'interactive' | 'subtle'

interface AppCardProps<T extends ElementType = 'article'> {
  as?: T
  children: ReactNode
  className?: string
  variant?: AppCardVariant
}

const variantClasses: Record<AppCardVariant, string> = {
  default:
    'border-amber-200/10 bg-black/25 shadow-[0_0_26px_rgba(0,0,0,0.16)]',
  interactive:
    'border-amber-200/10 bg-black/25 shadow-[0_0_26px_rgba(0,0,0,0.16)] transition hover:border-amber-200/25 hover:bg-black/35',
  subtle: 'border-amber-200/10 bg-white/[0.035]',
}

function AppCard<T extends ElementType = 'article'>({
  as,
  children,
  className = '',
  variant = 'default',
}: AppCardProps<T> & Omit<ComponentPropsWithoutRef<T>, keyof AppCardProps<T>>) {
  const Component = as ?? 'article'

  return (
    <Component
      className={`min-w-0 rounded-3xl border p-4 sm:p-5 ${variantClasses[variant]} ${className}`}
    >
      {children}
    </Component>
  )
}

export default AppCard
