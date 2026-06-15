import type {
  AuthFormErrors,
  ForgotPasswordFormValues,
  LoginFormValues,
  RegisterFormValues,
} from './authTypes'

const validateEmail = (email: string) => {
  if (!email.trim()) {
    return 'Email je obavezan.'
  }

  if (!email.includes('@')) {
    return 'Email mora sadržavati @.'
  }

  return undefined
}

const validatePassword = (password: string) => {
  if (!password) {
    return 'Lozinka je obavezna.'
  }

  if (password.length < 8) {
    return 'Lozinka mora imati najmanje 8 karaktera.'
  }

  return undefined
}

export function validateLogin(values: LoginFormValues) {
  const errors: AuthFormErrors<LoginFormValues> = {}
  const emailError = validateEmail(values.email)
  const passwordError = validatePassword(values.password)

  if (emailError) {
    errors.email = emailError
  }

  if (passwordError) {
    errors.password = passwordError
  }

  return errors
}

export function validateRegister(values: RegisterFormValues) {
  const errors: AuthFormErrors<RegisterFormValues> = {
    ...validateLogin(values),
  }

  const nameParts = values.fullName.trim().split(/\s+/).filter(Boolean)

  if (nameParts.length < 2) {
    errors.fullName = 'Unesite ime i prezime.'
  }

  if (values.phone.trim().length > 30) {
    errors.phone = 'Telefon može imati najviše 30 karaktera.'
  }

  if (values.confirmPassword !== values.password) {
    errors.confirmPassword = 'Lozinke se ne poklapaju.'
  }

  return errors
}

export function validateForgotPassword(values: ForgotPasswordFormValues) {
  const errors: AuthFormErrors<ForgotPasswordFormValues> = {}
  const emailError = validateEmail(values.email)

  if (emailError) {
    errors.email = emailError
  }

  return errors
}
