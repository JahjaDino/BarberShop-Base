import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  getClientDashboard,
  type ClientDashboardDto,
  type ClientServiceCardDto,
} from '../../api/clientApi'
import AppCard from '../../components/common/AppCard'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatusBadge from '../../components/common/StatusBadge'
import { buttonStyles } from '../../components/common/buttonStyles'

function formatPrice(price: number) {
  return `${price.toFixed(price % 1 === 0 ? 0 : 2)} KM`
}

function formatDate(date: string) {
  const [year, month, day] = date.split('-')
  return year && month && day ? `${day}.${month}.${year}` : date
}

function formatTime(time: string) {
  return time.slice(0, 5)
}

function PopularServiceCard({ service }: { service: ClientServiceCardDto }) {
  return (
    <AppCard className="flex min-h-[230px] flex-col" variant="interactive">
      <p className="break-words text-xs font-semibold uppercase tracking-[0.18em] text-amber-200/65 sm:tracking-[0.22em]">
        {service.categoryName || 'Usluga'}
      </p>
      <h3 className="mt-3 break-words text-xl font-black text-stone-50">{service.name}</h3>
      <p className="mt-3 flex-1 break-words text-sm leading-6 text-stone-400">
        {service.description || 'Opis usluge nije unesen.'}
      </p>
      <div className="mt-5 flex min-w-0 flex-col gap-2 rounded-2xl border border-amber-200/10 bg-white/[0.035] px-4 py-3 text-sm sm:flex-row sm:items-center sm:justify-between sm:gap-3">
        <span className="text-stone-300">{service.durationMinutes} min</span>
        <span className="font-semibold text-amber-100">
          {formatPrice(service.price)}
        </span>
      </div>
      <Link to="/app/book-appointment" className={`mt-5 ${buttonStyles.ghost}`}>
        Zakaži
      </Link>
    </AppCard>
  )
}

function ClientDashboard() {
  const [dashboard, setDashboard] = useState<ClientDashboardDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadDashboard() {
      setIsLoading(true)
      setError('')

      try {
        const data = await getClientDashboard()
        if (isMounted) {
          setDashboard(data)
        }
      } catch (requestError) {
        if (isMounted) {
          setDashboard(null)
          setError(
            requestError instanceof Error
              ? requestError.message
              : 'Pregled trenutno nije dostupan.',
          )
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadDashboard()

    return () => {
      isMounted = false
    }
  }, [])

  return (
    <div className="grid gap-6">
      <PageHeader
        title="Pregled"
        subtitle="Upravljajte terminima i rezervacijama na jednom mjestu."
      />

      <section className="min-w-0 rounded-[28px] border border-amber-200/15 bg-black/30 p-4 shadow-[0_0_45px_rgba(0,0,0,0.26)] backdrop-blur-xl sm:rounded-[32px] sm:p-6 lg:p-8">
        <h1 className="max-w-3xl break-words text-3xl font-black leading-tight text-stone-50 sm:text-4xl">
          Dobrodošli nazad
        </h1>
        <p className="mt-4 max-w-2xl break-words text-sm leading-7 text-stone-300 sm:text-base sm:leading-8">
          Zakažite novi termin i pratite svoje rezervacije u Classic Cuts
          aplikaciji.
        </p>
        <div className="mt-6 flex flex-col gap-3 sm:flex-row">
          <Link to="/app/book-appointment" className={buttonStyles.primary}>
            Zakaži termin
          </Link>
          <Link to="/app/my-appointments" className={buttonStyles.secondary}>
            Moji termini
          </Link>
        </div>
      </section>

      {error && (
        <div className="rounded-2xl border border-red-300/20 bg-red-400/10 p-4 text-sm font-semibold text-red-100">
          {error}
        </div>
      )}

      {isLoading ? (
        <SectionCard>
          <div className="rounded-2xl border border-amber-200/10 bg-white/[0.035] p-5 text-sm text-stone-300">
            Učitavanje pregleda...
          </div>
        </SectionCard>
      ) : (
        <>
          <SectionCard>
            <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                  Naredni termin
                </p>
                {dashboard?.nextAppointment ? (
                  <>
                    <h2 className="mt-3 text-2xl font-black text-stone-50">
                      {dashboard.nextAppointment.serviceName}
                    </h2>
                    <p className="mt-3 text-sm leading-6 text-stone-400">
                      {dashboard.nextAppointment.employeeName} ·{' '}
                      {formatDate(dashboard.nextAppointment.date)} ·{' '}
                      {formatTime(dashboard.nextAppointment.time)}
                    </p>
                  </>
                ) : (
                  <>
                    <h2 className="mt-3 text-2xl font-black text-stone-50">
                      Trenutno nemate zakazan termin.
                    </h2>
                    <p className="mt-3 max-w-2xl text-sm leading-6 text-stone-400">
                      Kada rezervacija bude potvrđena, ovdje ćete vidjeti
                      uslugu, frizera, datum i vrijeme dolaska.
                    </p>
                  </>
                )}
              </div>
              {dashboard?.nextAppointment && (
                <StatusBadge
                  label={dashboard.nextAppointment.status}
                  tone={
                    dashboard.nextAppointment.status === 'CONFIRMED'
                      ? 'success'
                      : 'warning'
                  }
                />
              )}
            </div>
          </SectionCard>

          <SectionCard>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                  Usluge
                </p>
                <h2 className="mt-3 text-2xl font-black text-stone-50">
                  Popularne usluge
                </h2>
              </div>
              <Link to="/app/services" className={buttonStyles.ghost}>
                Pregledaj sve
              </Link>
            </div>

            <div className="mt-6">
              {dashboard?.popularServices?.length ? (
                <div className="grid gap-4 md:grid-cols-3">
                  {dashboard.popularServices.slice(0, 3).map((service) => (
                    <PopularServiceCard
                      key={service.serviceId}
                      service={service}
                    />
                  ))}
                </div>
              ) : (
                <EmptyState
                  title="Nema usluga za prikaz."
                  description="Kada salon doda aktivne usluge, bit će prikazane ovdje."
                  actionLabel="Pregledaj usluge"
                  actionTo="/app/services"
                />
              )}
            </div>
          </SectionCard>
        </>
      )}
    </div>
  )
}

export default ClientDashboard
