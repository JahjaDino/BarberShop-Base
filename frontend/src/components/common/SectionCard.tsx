import type { ReactNode } from 'react'

interface SectionCardProps {
  children: ReactNode
  className?: string
}

function SectionCard({ children, className = '' }: SectionCardProps) {
  return (
    <section
      className={`rounded-[28px] border border-amber-200/10 bg-white/[0.035] p-5 shadow-[0_0_34px_rgba(0,0,0,0.20)] backdrop-blur lg:p-6 ${className}`}
    >
      {children}
    </section>
  )
}

export default SectionCard
