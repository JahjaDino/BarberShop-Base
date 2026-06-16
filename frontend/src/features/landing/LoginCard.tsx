import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  forgotPassword,
  loginUser,
  registerUser,
} from '../../api/authApi'
import type {
  AuthFormErrors,
  AuthMode,
  ForgotPasswordFormValues,
  LoginCardMode,
  LoginFormValues,
  RegisterFormValues,
} from '../auth/authTypes'
import {
  validateForgotPassword,
  validateLogin,
  validateRegister,
} from '../auth/authValidation'

interface LoginCardProps {
  mode: AuthMode
  highlighted?: boolean
}

const initialLoginValues: LoginFormValues = {
  email: '',
  password: '',
}

const initialRegisterValues: RegisterFormValues = {
  fullName: '',
  email: '',
  phone: '',
  password: '',
  confirmPassword: '',
}

const initialForgotPasswordValues: ForgotPasswordFormValues = {
  email: '',
}

function getRegisterRequest(values: RegisterFormValues) {
  const [firstName, ...lastNameParts] = values.fullName.trim().split(/\s+/)

  return {
    firstName,
    lastName: lastNameParts.join(' '),
    email: values.email.trim(),
    phone: values.phone.trim(),
    password: values.password,
  }
}

function getErrorMessage(error: unknown) {
  if (error instanceof Error) {
    return error.message
  }

  return 'Došlo je do greške. Pokušajte ponovo.'
}

function LoginCard({ mode, highlighted = false }: LoginCardProps) {
  const navigate = useNavigate()
  const [cardMode, setCardMode] = useState<LoginCardMode>(mode)
  const isLogin = cardMode === 'login'
  const isRegister = cardMode === 'register'
  const isForgotPassword = cardMode === 'forgot-password'

  const [loginValues, setLoginValues] =
    useState<LoginFormValues>(initialLoginValues)
  const [registerValues, setRegisterValues] =
    useState<RegisterFormValues>(initialRegisterValues)
  const [forgotPasswordValues, setForgotPasswordValues] =
    useState<ForgotPasswordFormValues>(initialForgotPasswordValues)
  const [loginErrors, setLoginErrors] =
    useState<AuthFormErrors<LoginFormValues>>({})
  const [registerErrors, setRegisterErrors] =
    useState<AuthFormErrors<RegisterFormValues>>({})
  const [forgotPasswordErrors, setForgotPasswordErrors] =
    useState<AuthFormErrors<ForgotPasswordFormValues>>({})
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [apiError, setApiError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')

  useEffect(() => {
    setCardMode(mode)
    setApiError('')
    setSuccessMessage('')
  }, [mode])

  const clearMessages = () => {
    setApiError('')
    setSuccessMessage('')
  }

  const handleLoginChange = (field: keyof LoginFormValues, value: string) => {
    setLoginValues((currentValues) => ({
      ...currentValues,
      [field]: value,
    }))
    setLoginErrors((currentErrors) => ({
      ...currentErrors,
      [field]: undefined,
    }))
    clearMessages()
  }

  const handleRegisterChange = (
    field: keyof RegisterFormValues,
    value: string,
  ) => {
    setRegisterValues((currentValues) => ({
      ...currentValues,
      [field]: value,
    }))
    setRegisterErrors((currentErrors) => ({
      ...currentErrors,
      [field]: undefined,
    }))
    clearMessages()
  }

  const handleForgotPasswordChange = (
    field: keyof ForgotPasswordFormValues,
    value: string,
  ) => {
    setForgotPasswordValues((currentValues) => ({
      ...currentValues,
      [field]: value,
    }))
    setForgotPasswordErrors((currentErrors) => ({
      ...currentErrors,
      [field]: undefined,
    }))
    clearMessages()
  }

  const handleLoginSubmit = async () => {
    const errors = validateLogin(loginValues)
    setLoginErrors(errors)

    if (Object.keys(errors).length > 0) {
      return
    }

    await loginUser({
      email: loginValues.email.trim(),
      password: loginValues.password,
    })
    navigate('/app/dashboard', { replace: true })
  }

  const handleRegisterSubmit = async () => {
    const errors = validateRegister(registerValues)
    setRegisterErrors(errors)

    if (Object.keys(errors).length > 0) {
      return
    }

    await registerUser(getRegisterRequest(registerValues))
    setRegisterValues(initialRegisterValues)
    setLoginValues({
      email: registerValues.email.trim(),
      password: '',
    })
    setCardMode('login')
    setSuccessMessage('Registracija je uspješna. Možete se prijaviti.')
  }

  const handleForgotPasswordSubmit = async () => {
    const errors = validateForgotPassword(forgotPasswordValues)
    setForgotPasswordErrors(errors)

    if (Object.keys(errors).length > 0) {
      return
    }

    const response = await forgotPassword({
      email: forgotPasswordValues.email.trim(),
    })
    setForgotPasswordValues(initialForgotPasswordValues)
    setSuccessMessage(response.message)
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    clearMessages()
    setIsSubmitting(true)

    try {
      if (isLogin) {
        await handleLoginSubmit()
      } else if (isRegister) {
        await handleRegisterSubmit()
      } else {
        await handleForgotPasswordSubmit()
      }
    } catch (error) {
      setApiError(getErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  const title = isLogin
    ? 'Prijava'
    : isRegister
      ? 'Registracija'
      : 'Reset lozinke'

  const submitLabel = isLogin
    ? 'Prijavi se'
    : isRegister
      ? 'Kreiraj nalog'
      : 'Pošalji upute'

  return (
    <section
      id="login-card"
      className={`rounded-2xl border border-amber-200/20 bg-stone-950/75 p-6 backdrop-blur-xl transition-all duration-500 ${
        highlighted
          ? 'ring-2 ring-amber-200 shadow-[0_0_45px_rgba(245,213,145,0.35)]'
          : 'shadow-[0_0_55px_rgba(245,213,145,0.14)]'
      }`}
    >
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs uppercase tracking-[0.24em] text-amber-200/70">
            Pristup aplikaciji
          </p>
          <h2 className="mt-2 text-2xl font-semibold text-stone-50">
            {title}
          </h2>
        </div>

        <span className="rounded-full border border-blue-200/20 bg-blue-950/40 px-3 py-1 text-xs font-medium text-blue-100">
          Online
        </span>
      </div>

      <form className="mt-6 grid gap-4" onSubmit={handleSubmit} noValidate>
        {isRegister && (
          <label className="grid gap-2 text-sm text-stone-300">
            Ime i prezime
            <input
              type="text"
              value={registerValues.fullName}
              onChange={(event) =>
                handleRegisterChange('fullName', event.target.value)
              }
              placeholder="Unesite ime i prezime"
              className="rounded-2xl border border-stone-700 bg-black/35 px-4 py-3 text-stone-100 outline-none transition placeholder:text-stone-500 focus:border-amber-200/60 focus:ring-2 focus:ring-amber-200/10"
            />
            {registerErrors.fullName && (
              <span className="text-xs font-medium text-red-300">
                {registerErrors.fullName}
              </span>
            )}
          </label>
        )}

        <label className="grid gap-2 text-sm text-stone-300">
          Email
          <input
            type="email"
            value={
              isLogin
                ? loginValues.email
                : isRegister
                  ? registerValues.email
                  : forgotPasswordValues.email
            }
            onChange={(event) => {
              if (isLogin) {
                handleLoginChange('email', event.target.value)
              } else if (isRegister) {
                handleRegisterChange('email', event.target.value)
              } else {
                handleForgotPasswordChange('email', event.target.value)
              }
            }}
            placeholder="ime@email.com"
            className="rounded-2xl border border-stone-700 bg-black/35 px-4 py-3 text-stone-100 outline-none transition placeholder:text-stone-500 focus:border-amber-200/60 focus:ring-2 focus:ring-amber-200/10"
          />
          {(isLogin
            ? loginErrors.email
            : isRegister
              ? registerErrors.email
              : forgotPasswordErrors.email) && (
            <span className="text-xs font-medium text-red-300">
              {isLogin
                ? loginErrors.email
                : isRegister
                  ? registerErrors.email
                  : forgotPasswordErrors.email}
            </span>
          )}
        </label>

        {isRegister && (
          <label className="grid gap-2 text-sm text-stone-300">
            Telefon
            <input
              type="tel"
              value={registerValues.phone}
              onChange={(event) =>
                handleRegisterChange('phone', event.target.value)
              }
              placeholder="+387 61 123 456"
              className="rounded-2xl border border-stone-700 bg-black/35 px-4 py-3 text-stone-100 outline-none transition placeholder:text-stone-500 focus:border-amber-200/60 focus:ring-2 focus:ring-amber-200/10"
            />
            {registerErrors.phone && (
              <span className="text-xs font-medium text-red-300">
                {registerErrors.phone}
              </span>
            )}
          </label>
        )}

        {!isForgotPassword && (
          <label className="grid gap-2 text-sm text-stone-300">
            Lozinka
            <input
              type="password"
              value={isLogin ? loginValues.password : registerValues.password}
              onChange={(event) =>
                isLogin
                  ? handleLoginChange('password', event.target.value)
                  : handleRegisterChange('password', event.target.value)
              }
              placeholder="********"
              className="rounded-2xl border border-stone-700 bg-black/35 px-4 py-3 text-stone-100 outline-none transition placeholder:text-stone-500 focus:border-amber-200/60 focus:ring-2 focus:ring-amber-200/10"
            />
            {(isLogin ? loginErrors.password : registerErrors.password) && (
              <span className="text-xs font-medium text-red-300">
                {isLogin ? loginErrors.password : registerErrors.password}
              </span>
            )}
          </label>
        )}

        {isRegister && (
          <label className="grid gap-2 text-sm text-stone-300">
            Potvrdi lozinku
            <input
              type="password"
              value={registerValues.confirmPassword}
              onChange={(event) =>
                handleRegisterChange('confirmPassword', event.target.value)
              }
              placeholder="********"
              className="rounded-2xl border border-stone-700 bg-black/35 px-4 py-3 text-stone-100 outline-none transition placeholder:text-stone-500 focus:border-amber-200/60 focus:ring-2 focus:ring-amber-200/10"
            />
            {registerErrors.confirmPassword && (
              <span className="text-xs font-medium text-red-300">
                {registerErrors.confirmPassword}
              </span>
            )}
          </label>
        )}

        {isLogin && (
          <button
            type="button"
            onClick={() => {
              setCardMode('forgot-password')
              clearMessages()
            }}
            className="justify-self-start text-sm font-medium text-amber-100 transition hover:text-[#d6b56c]"
          >
            Zaboravljena lozinka?
          </button>
        )}

        {isForgotPassword && (
          <button
            type="button"
            onClick={() => {
              setCardMode('login')
              clearMessages()
            }}
            className="justify-self-start text-sm font-medium text-amber-100 transition hover:text-[#d6b56c]"
          >
            Nazad na prijavu
          </button>
        )}

        {apiError && (
          <p className="rounded-2xl border border-red-300/20 bg-red-950/35 px-4 py-3 text-sm font-medium text-red-200">
            {apiError}
          </p>
        )}

        {successMessage && (
          <p className="rounded-2xl border border-blue-200/20 bg-blue-950/40 px-4 py-3 text-sm font-medium text-blue-100">
            {successMessage}
          </p>
        )}

        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-2xl bg-amber-200 px-5 py-3 text-sm font-bold uppercase tracking-[0.18em] text-blue-950 transition-all duration-300 hover:-translate-y-0.5 hover:bg-[#d6b56c] hover:shadow-lg disabled:cursor-not-allowed disabled:opacity-60 disabled:hover:translate-y-0 disabled:hover:bg-amber-200"
        >
          {isSubmitting ? 'Molimo sačekajte...' : submitLabel}
        </button>
      </form>
    </section>
  )
}

export default LoginCard
