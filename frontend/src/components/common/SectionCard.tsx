import type { ReactNode } from 'react'

interface SectionCardProps {
  children: ReactNode
  className?: string
}

function SectionCard({ children, className = '' }: SectionCardProps) {
  return (
    <section
      className={`min-w-0 rounded-[24px] border border-amber-200/10 bg-white/[0.035] p-4 shadow-[0_0_34px_rgba(0,0,0,0.20)] backdrop-blur sm:rounded-[28px] sm:p-5 lg:p-6 ${className}`}
    >
      {children}
    </section>
  )
}

export default SectionCard
