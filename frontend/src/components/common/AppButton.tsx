import type { ButtonHTMLAttributes, ReactNode } from 'react'
import { buttonStyles } from './buttonStyles'

type AppButtonVariant = keyof typeof buttonStyles

interface AppButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode
  variant?: AppButtonVariant
}

function AppButton({
  children,
  className = '',
  type = 'button',
  variant = 'secondary',
  ...props
}: AppButtonProps) {
  return (
    <button
      type={type}
      className={`${buttonStyles[variant]} ${className}`}
      {...props}
    >
      {children}
    </button>
  )
}

export default AppButton
