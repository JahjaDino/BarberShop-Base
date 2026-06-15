export type AuthMode = 'login' | 'register'
export type LoginCardMode = AuthMode | 'forgot-password'

export interface LoginFormValues {
  email: string
  password: string
}

export interface RegisterFormValues extends LoginFormValues {
  fullName: string
  phone: string
  confirmPassword: string
}

export interface ForgotPasswordFormValues {
  email: string
}

export type AuthFormErrors<T> = Partial<Record<keyof T, string>>
