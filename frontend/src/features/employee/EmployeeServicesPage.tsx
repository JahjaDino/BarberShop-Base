import { useEffect, useState } from 'react'
import { getEmployeeServices } from '../../api/employeeApi'
import type { EmployeeServiceDto } from '../../api/employeeApi'
import AppCard from '../../components/common/AppCard'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatusBadge from '../../components/common/StatusBadge'

function formatPrice(price: number) {
  return `${price.toFixed(price % 1 === 0 ? 0 : 2)} KM`
}

function EmployeeServicesPage() {
  const [services, setServices] = useState<EmployeeServiceDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadServices() {
      try {
        setIsLoading(true)
        setError('')

        const data = await getEmployeeServices()

        if (isMounted) {
          setServices(data)
        }
      } catch (apiError) {
        if (isMounted) {
          setError(
            apiError instanceof Error
              ? apiError.message
              : 'Usluge trenutno nisu dostupne.',
          )
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadServices()

    return () => {
      isMounted = false
    }
  }, [])

  return (
    <div className="grid gap-6">
      <PageHeader
        eyebrow="Frizerski prostor"
        title="Usluge"
        subtitle="Pregled aktivnih usluga dostupnih u vasem salonu."
      />

      <SectionCard>
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
              Katalog salona
            </p>
            <h2 className="mt-3 text-2xl font-black text-stone-50">
              Aktivne usluge
            </h2>
          </div>
          <StatusBadge label={`${services.length} usluga`} />
        </div>

        {isLoading && (
          <p className="mt-6 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm text-stone-300">
            Učitavanje usluga...
          </p>
        )}

        {error && (
          <p className="mt-6 rounded-2xl border border-red-300/20 bg-red-400/10 px-4 py-3 text-sm font-semibold text-red-100">
            {error}
          </p>
        )}

        {!isLoading && !error && services.length === 0 && (
          <div className="mt-6">
            <EmptyState
              title="Trenutno nema aktivnih usluga."
              description="Kada vlasnik doda aktivne usluge za salon, bit ce prikazane ovdje."
            />
          </div>
        )}

        {!isLoading && !error && services.length > 0 && (
          <div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            {services.map((service) => (
              <AppCard
                key={service.serviceId}
                className="flex min-h-[300px] min-w-0 flex-col"
                variant="interactive"
              >
                <div className="flex items-start justify-between gap-4">
                  <div className="min-w-0">
                    <p className="truncate text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/65">
                      {service.categoryName || 'Usluga'}
                    </p>
                    <h3 className="mt-3 break-words text-xl font-black text-stone-50">
                      {service.name}
                    </h3>
                  </div>
                  <StatusBadge label="Aktivna" tone="success" />
                </div>

                <p className="mt-4 flex-1 break-words text-sm leading-6 text-stone-400">
                  {service.description || 'Opis usluge nije dodan.'}
                </p>

                <div className="mt-5 grid min-w-0 grid-cols-1 gap-3 sm:grid-cols-2">
                  <div className="min-w-0 rounded-2xl border border-amber-200/10 bg-white/[0.035] px-4 py-3">
                    <p className="text-[10px] font-bold uppercase tracking-[0.16em] text-stone-500">
                      Trajanje
                    </p>
                    <p className="mt-1 font-semibold text-stone-100">
                      {service.durationMinutes} min
                    </p>
                  </div>
                  <div className="min-w-0 rounded-2xl border border-amber-200/10 bg-white/[0.035] px-4 py-3">
                    <p className="text-[10px] font-bold uppercase tracking-[0.16em] text-stone-500">
                      Cijena
                    </p>
                    <p className="mt-1 font-semibold text-amber-100">
                      {formatPrice(service.price)}
                    </p>
                  </div>
                </div>
              </AppCard>
            ))}
          </div>
        )}
      </SectionCard>
    </div>
  )
}

export default EmployeeServicesPage
