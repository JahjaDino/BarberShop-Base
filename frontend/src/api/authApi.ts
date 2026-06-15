const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5288/api'

export const AUTH_TOKEN_STORAGE_KEY = 'barbershop.accessToken'
export const AUTH_REFRESH_TOKEN_STORAGE_KEY = 'barbershop.refreshToken'
export const AUTH_USER_STORAGE_KEY = 'barbershop.user'
export const AUTH_USER_UPDATED_EVENT = 'barbershop:user-updated'

export interface CurrentUser {
  id: number
  email: string
  firstName: string
  lastName: string
  phoneNumber: string
  status: string
  roles: string[]
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
  user: CurrentUser
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest extends LoginRequest {
  firstName: string
  lastName: string
  phone: string
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  token: string
  newPassword: string
}

export interface ProfileUpdateRequest {
  firstName: string
  lastName: string
  phoneNumber: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

interface ApiMessageResponse {
  message: string
}

interface ApiValidationErrorResponse {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

const backendMessageTranslations: Record<string, string> = {
  'Email is already registered.': 'Email je već registrovan.',
  'Invalid credentials.': 'Pogrešan email ili lozinka.',
  'Invalid email or password.': 'Pogrešan email ili lozinka.',
  'Invalid token.': 'Nevažeći token.',
  'Current password is not valid.': 'Trenutna lozinka nije tačna.',
  'Password has been changed successfully.':
    'Lozinka je uspješno promijenjena.',
  'If the email exists, password reset instructions will be sent.':
    'Ako email postoji, upute za reset lozinke će biti poslane.',
  'Invalid or expired reset token.':
    'Token za reset lozinke nije validan ili je istekao.',
  'Password has been reset successfully.':
    'Lozinka je uspješno resetovana.',
  'Logged out successfully.': 'Uspješno ste se odjavili.',
  'One or more validation errors occurred.':
    'Provjerite unesene podatke i pokušajte ponovo.',
}

const validationMessageTranslations: Array<[RegExp, string]> = [
  [/^The Email field is required\.$/i, 'Email je obavezan.'],
  [/^The Password field is required\.$/i, 'Lozinka je obavezna.'],
  [/^The FirstName field is required\.$/i, 'Ime je obavezno.'],
  [/^The LastName field is required\.$/i, 'Prezime je obavezno.'],
  [/^The Token field is required\.$/i, 'Token je obavezan.'],
  [/^The NewPassword field is required\.$/i, 'Nova lozinka je obavezna.'],
  [/^The CurrentPassword field is required\.$/i,
    'Trenutna lozinka je obavezna.'],
  [/^The PhoneNumber field must be a string with a maximum length of 30\.$/i,
    'Telefon može imati najviše 30 karaktera.'],
  [/^The field Email must be a string with a maximum length of 256\.$/i,
    'Email može imati najviše 256 karaktera.'],
  [/^The field FirstName must be a string with a minimum length of 2 and a maximum length of 100\.$/i,
    'Ime mora imati između 2 i 100 karaktera.'],
  [/^The field LastName must be a string with a minimum length of 2 and a maximum length of 100\.$/i,
    'Prezime mora imati između 2 i 100 karaktera.'],
  [/^The field Phone must be a string with a maximum length of 30\.$/i,
    'Telefon može imati najviše 30 karaktera.'],
  [/^The field Password must be a string or array type with a minimum length of '8'\.$/i,
    'Lozinka mora imati najmanje 8 karaktera.'],
  [/^The field NewPassword must be a string or array type with a minimum length of '8'\.$/i,
    'Nova lozinka mora imati najmanje 8 karaktera.'],
  [/^The Email field is not a valid e-mail address\.$/i,
    'Email adresa nije validna.'],
]

function translateMessage(message: string) {
  const trimmedMessage = message.trim()
  return backendMessageTranslations[trimmedMessage] ?? trimmedMessage
}

function translateValidationMessage(message: string) {
  const trimmedMessage = message.trim()
  const translation = validationMessageTranslations.find(([pattern]) =>
    pattern.test(trimmedMessage),
  )

  return translation?.[1] ?? translateMessage(trimmedMessage)
}

function getValidationErrorMessage(errors: Record<string, string[]>) {
  const messages = Object.values(errors)
    .flat()
    .map(translateValidationMessage)
    .filter(Boolean)

  return messages.length > 0
    ? messages.join(' ')
    : 'Provjerite unesene podatke i pokušajte ponovo.'
}

export class AuthApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'AuthApiError'
    this.status = status
  }
}

async function readErrorMessage(response: Response) {
  try {
    const body = (await response.json()) as ApiValidationErrorResponse

    if (body.errors) {
      return getValidationErrorMessage(body.errors)
    }

    if (body.message) {
      return translateMessage(body.message)
    }

    if (body.title) {
      return translateMessage(body.title)
    }

    return 'Zahtjev nije uspio.'
  } catch {
    return 'Zahtjev nije uspio.'
  }
}

async function request<TResponse>(
  path: string,
  options: RequestInit = {},
): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  if (!response.ok) {
    throw new AuthApiError(await readErrorMessage(response), response.status)
  }

  return (await response.json()) as TResponse
}

export function saveAuthSession(authResponse: AuthResponse) {
  localStorage.setItem(AUTH_TOKEN_STORAGE_KEY, authResponse.accessToken)
  localStorage.setItem(
    AUTH_REFRESH_TOKEN_STORAGE_KEY,
    authResponse.refreshToken,
  )
  localStorage.setItem(AUTH_USER_STORAGE_KEY, JSON.stringify(authResponse.user))
  window.dispatchEvent(new Event(AUTH_USER_UPDATED_EVENT))
}

export function clearAuthSession() {
  localStorage.removeItem(AUTH_TOKEN_STORAGE_KEY)
  localStorage.removeItem(AUTH_REFRESH_TOKEN_STORAGE_KEY)
  localStorage.removeItem(AUTH_USER_STORAGE_KEY)
  window.dispatchEvent(new Event(AUTH_USER_UPDATED_EVENT))
}

export function getStoredAccessToken() {
  return localStorage.getItem(AUTH_TOKEN_STORAGE_KEY)
}

export function getStoredRefreshToken() {
  return localStorage.getItem(AUTH_REFRESH_TOKEN_STORAGE_KEY)
}

export function getStoredUser() {
  const storedUser = localStorage.getItem(AUTH_USER_STORAGE_KEY)

  if (!storedUser) {
    return null
  }

  try {
    return JSON.parse(storedUser) as CurrentUser
  } catch {
    clearAuthSession()
    return null
  }
}

export async function loginUser(requestBody: LoginRequest) {
  const response = await request<AuthResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify(requestBody),
  })

  saveAuthSession(response)
  return response
}

export async function registerUser(requestBody: RegisterRequest) {
  const response = await request<AuthResponse>('/auth/register', {
    method: 'POST',
    body: JSON.stringify(requestBody),
  })

  saveAuthSession(response)
  return response
}

export function forgotPassword(requestBody: ForgotPasswordRequest) {
  return request<ApiMessageResponse>('/auth/forgot-password', {
    method: 'POST',
    body: JSON.stringify(requestBody),
  }).then(() => {
    return {
      message: 'Ako email postoji, upute za reset lozinke će biti poslane.',
    }
  })
}

export async function resetPassword(requestBody: ResetPasswordRequest) {
  await request<ApiMessageResponse>('/auth/reset-password', {
    method: 'POST',
    body: JSON.stringify(requestBody),
  })

  return {
    message: 'Lozinka je uspješno resetovana.',
  }
}

export async function getCurrentUser() {
  const token = getStoredAccessToken()

  if (!token) {
    throw new AuthApiError('Korisnik nije prijavljen.', 401)
  }

  return request<CurrentUser>('/auth/me', {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${token}`,
    },
  })
}

export async function updateProfile(requestBody: ProfileUpdateRequest) {
  const token = getStoredAccessToken()

  if (!token) {
    throw new AuthApiError('Korisnik nije prijavljen.', 401)
  }

  const user = await request<CurrentUser>('/profile', {
    method: 'PATCH',
    headers: {
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(requestBody),
  })

  localStorage.setItem(AUTH_USER_STORAGE_KEY, JSON.stringify(user))
  window.dispatchEvent(new Event(AUTH_USER_UPDATED_EVENT))
  return user
}

export async function changePassword(requestBody: ChangePasswordRequest) {
  const token = getStoredAccessToken()

  if (!token) {
    throw new AuthApiError('Korisnik nije prijavljen.', 401)
  }

  await request<ApiMessageResponse>('/auth/change-password', {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(requestBody),
  })

  return {
    message: 'Lozinka je uspješno promijenjena.',
  }
}

export async function logoutUser() {
  const refreshToken = getStoredRefreshToken()

  if (refreshToken) {
    await request<ApiMessageResponse>('/auth/logout', {
      method: 'POST',
      body: JSON.stringify({ refreshToken }),
    })
  }

  clearAuthSession()
}
