import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'

import {
  changePassword,
  getCurrentUser,
  updateProfile,
  type ChangePasswordRequest,
  type CurrentUser,
  type ProfileUpdateRequest,
} from '../../api/authApi'
import PageHeader from './PageHeader'
import SectionCard from './SectionCard'
import StatusBadge from './StatusBadge'
import { buttonStyles } from './buttonStyles'

interface UserProfilePageProps {
  eyebrow?: string
  title?: string
  subtitle?: string
}

const initialPasswordForm: ChangePasswordRequest & {
  confirmPassword: string
} = {
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
}

function roleLabel(roles: string[]) {
  if (roles.includes('OWNER')) return 'Vlasnik'
  if (roles.includes('EMPLOYEE')) return 'Frizer'
  return 'Klijent'
}

function createProfileForm(user: CurrentUser): ProfileUpdateRequest {
  return {
    firstName: user.firstName,
    lastName: user.lastName,
    phoneNumber: user.phoneNumber ?? '',
  }
}

function UserProfilePage({
  eyebrow = 'Classic Cuts',
  title = 'Profil',
  subtitle = 'Upravljajte osnovnim informacijama svog računa.',
}: UserProfilePageProps) {
  const [user, setUser] = useState<CurrentUser | null>(null)
  const [profileForm, setProfileForm] = useState<ProfileUpdateRequest>({
    firstName: '',
    lastName: '',
    phoneNumber: '',
  })
  const [passwordForm, setPasswordForm] = useState(initialPasswordForm)
  const [isLoading, setIsLoading] = useState(true)
  const [isSavingProfile, setIsSavingProfile] = useState(false)
  const [isChangingPassword, setIsChangingPassword] = useState(false)
  const [profileError, setProfileError] = useState('')
  const [profileSuccess, setProfileSuccess] = useState('')
  const [passwordError, setPasswordError] = useState('')
  const [passwordSuccess, setPasswordSuccess] = useState('')

  async function loadProfile() {
    setIsLoading(true)
    setProfileError('')

    try {
      const currentUser = await getCurrentUser()
      setUser(currentUser)
      setProfileForm(createProfileForm(currentUser))
    } catch (error) {
      setUser(null)
      setProfileError(
        error instanceof Error
          ? error.message
          : 'Profil trenutno nije moguće učitati.',
      )
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadProfile()
  }, [])

  function handleProfileChange(field: keyof ProfileUpdateRequest, value: string) {
    setProfileForm((currentForm) => ({ ...currentForm, [field]: value }))
    setProfileError('')
    setProfileSuccess('')
  }

  function handlePasswordChange(
    field: keyof typeof initialPasswordForm,
    value: string,
  ) {
    setPasswordForm((currentForm) => ({ ...currentForm, [field]: value }))
    setPasswordError('')
    setPasswordSuccess('')
  }

  async function handleProfileSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (profileForm.firstName.trim().length < 2) {
      setProfileError('Ime mora imati najmanje 2 karaktera.')
      return
    }

    if (profileForm.lastName.trim().length < 2) {
      setProfileError('Prezime mora imati najmanje 2 karaktera.')
      return
    }

    try {
      setIsSavingProfile(true)
      setProfileError('')
      setProfileSuccess('')

      const updatedUser = await updateProfile({
        firstName: profileForm.firstName.trim(),
        lastName: profileForm.lastName.trim(),
        phoneNumber: profileForm.phoneNumber.trim(),
      })

      setUser(updatedUser)
      setProfileForm(createProfileForm(updatedUser))
      setProfileSuccess('Profil je uspješno ažuriran.')
    } catch (error) {
      setProfileError(
        error instanceof Error
          ? error.message
          : 'Profil trenutno nije moguće ažurirati.',
      )
    } finally {
      setIsSavingProfile(false)
    }
  }

  async function handlePasswordSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!passwordForm.currentPassword) {
      setPasswordError('Unesite trenutnu lozinku.')
      return
    }

    if (passwordForm.newPassword.length < 8) {
      setPasswordError('Nova lozinka mora imati najmanje 8 karaktera.')
      return
    }

    if (passwordForm.newPassword !== passwordForm.confirmPassword) {
      setPasswordError('Potvrda lozinke mora biti ista kao nova lozinka.')
      return
    }

    try {
      setIsChangingPassword(true)
      setPasswordError('')
      setPasswordSuccess('')

      await changePassword({
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword,
      })

      setPasswordForm(initialPasswordForm)
      setPasswordSuccess('Lozinka je uspješno promijenjena.')
    } catch (error) {
      setPasswordError(
        error instanceof Error
          ? error.message
          : 'Lozinku trenutno nije moguće promijeniti.',
      )
    } finally {
      setIsChangingPassword(false)
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader eyebrow={eyebrow} title={title} subtitle={subtitle} />

      {isLoading ? (
        <SectionCard>
          <p className="text-sm text-stone-300">Učitavanje profila...</p>
        </SectionCard>
      ) : (
        <section className="grid min-w-0 gap-6 xl:grid-cols-[minmax(0,1fr)_minmax(320px,420px)]">
          <SectionCard>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
              <div className="min-w-0">
                <p className="break-words text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/70 sm:tracking-[0.28em]">
                  Lični podaci
                </p>
                <h2 className="mt-3 break-words text-2xl font-black text-stone-50">
                  Osnovne informacije
                </h2>
              </div>
              {user && (
                <StatusBadge label={roleLabel(user.roles)} tone="neutral" />
              )}
            </div>

            <form onSubmit={handleProfileSubmit} className="mt-6 grid gap-4">
              <div className="grid gap-4 md:grid-cols-2">
                <label className="grid gap-2 text-sm font-semibold text-stone-300">
                  Ime
                  <input
                    value={profileForm.firstName}
                    onChange={(event) =>
                      handleProfileChange('firstName', event.target.value)
                    }
                    className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                  />
                </label>

                <label className="grid gap-2 text-sm font-semibold text-stone-300">
                  Prezime
                  <input
                    value={profileForm.lastName}
                    onChange={(event) =>
                      handleProfileChange('lastName', event.target.value)
                    }
                    className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                  />
                </label>

                <label className="grid gap-2 text-sm font-semibold text-stone-300">
                  Email
                  <input
                    value={user?.email ?? ''}
                    disabled
                    className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-300 outline-none disabled:cursor-not-allowed disabled:opacity-80"
                  />
                </label>

                <label className="grid gap-2 text-sm font-semibold text-stone-300">
                  Telefon
                  <input
                    value={profileForm.phoneNumber}
                    onChange={(event) =>
                      handleProfileChange('phoneNumber', event.target.value)
                    }
                    placeholder="Unesite broj telefona"
                    className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                  />
                </label>
              </div>

              {profileError && (
                <p className="rounded-2xl border border-red-300/20 bg-red-400/10 px-4 py-3 text-sm font-semibold text-red-100">
                  {profileError}
                </p>
              )}

              {profileSuccess && (
                <p className="rounded-2xl border border-emerald-300/20 bg-emerald-300/10 px-4 py-3 text-sm font-semibold text-emerald-100">
                  {profileSuccess}
                </p>
              )}

              <button
                type="submit"
                disabled={isSavingProfile}
                className={buttonStyles.primary}
              >
                {isSavingProfile ? 'Spremanje...' : 'Sačuvaj profil'}
              </button>
            </form>
          </SectionCard>

          <SectionCard>
              <p className="break-words text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/70 sm:tracking-[0.28em]">
                Sigurnost
              </p>
            <h2 className="mt-3 break-words text-2xl font-black text-stone-50">
              Promjena lozinke
            </h2>
            <p className="mt-2 text-sm leading-6 text-stone-400">
              Unesite trenutnu lozinku i novu lozinku za svoj račun.
            </p>

            <form onSubmit={handlePasswordSubmit} className="mt-6 grid gap-4">
              <label className="grid gap-2 text-sm font-semibold text-stone-300">
                Trenutna lozinka
                <input
                  type="password"
                  value={passwordForm.currentPassword}
                  onChange={(event) =>
                    handlePasswordChange('currentPassword', event.target.value)
                  }
                  className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                />
              </label>

              <label className="grid gap-2 text-sm font-semibold text-stone-300">
                Nova lozinka
                <input
                  type="password"
                  value={passwordForm.newPassword}
                  onChange={(event) =>
                    handlePasswordChange('newPassword', event.target.value)
                  }
                  className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                />
              </label>

              <label className="grid gap-2 text-sm font-semibold text-stone-300">
                Potvrda nove lozinke
                <input
                  type="password"
                  value={passwordForm.confirmPassword}
                  onChange={(event) =>
                    handlePasswordChange('confirmPassword', event.target.value)
                  }
                  className="rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                />
              </label>

              {passwordError && (
                <p className="rounded-2xl border border-red-300/20 bg-red-400/10 px-4 py-3 text-sm font-semibold text-red-100">
                  {passwordError}
                </p>
              )}

              {passwordSuccess && (
                <p className="rounded-2xl border border-emerald-300/20 bg-emerald-300/10 px-4 py-3 text-sm font-semibold text-emerald-100">
                  {passwordSuccess}
                </p>
              )}

              <button
                type="submit"
                disabled={isChangingPassword}
                className={buttonStyles.secondary}
              >
                {isChangingPassword ? 'Spremanje...' : 'Promijeni lozinku'}
              </button>
            </form>
          </SectionCard>
        </section>
      )}
    </div>
  )
}

export default UserProfilePage
