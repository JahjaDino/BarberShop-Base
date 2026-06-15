import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getServiceCategories, getServices } from '../../api/servicesApi'
import AppCard from '../../components/common/AppCard'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatusBadge from '../../components/common/StatusBadge'
import { buttonStyles } from '../../components/common/buttonStyles'
import type { ServiceCategoryDto, ServiceDto } from '../../types/services'

const allCategoriesValue = 'all'

function formatPrice(price: number) {
  return `${price.toFixed(price % 1 === 0 ? 0 : 2)} KM`
}

function ServicesPage() {
  const [categories, setCategories] = useState<ServiceCategoryDto[]>([])
  const [services, setServices] = useState<ServiceDto[]>([])
  const [activeCategory, setActiveCategory] = useState<string>(allCategoriesValue)
  const [isLoading, setIsLoading] = useState(true)
  const [isFiltering, setIsFiltering] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadInitialData() {
      setIsLoading(true)
      setError('')

      try {
        const [categoriesResponse, servicesResponse] = await Promise.all([
          getServiceCategories({ active: true }),
          getServices({ active: true }),
        ])

        if (!isMounted) return

        setCategories(categoriesResponse.items)
        setServices(servicesResponse.items)
      } catch {
        if (isMounted) {
          setError('Usluge trenutno nisu dostupne. Pokušajte ponovo kasnije.')
        }
      } finally {
        if (isMounted) {
          setIsLoading(false)
        }
      }
    }

    loadInitialData()

    return () => {
      isMounted = false
    }
  }, [])

  useEffect(() => {
    if (isLoading) return

    let isMounted = true

    async function loadFilteredServices() {
      setIsFiltering(true)
      setError('')

      try {
        const categoryId =
          activeCategory === allCategoriesValue ? undefined : Number(activeCategory)
        const response = await getServices({ active: true, categoryId })

        if (isMounted) {
          setServices(response.items)
        }
      } catch {
        if (isMounted) {
          setError('Usluge trenutno nisu dostupne. Pokušajte ponovo kasnije.')
        }
      } finally {
        if (isMounted) {
          setIsFiltering(false)
        }
      }
    }

    loadFilteredServices()

    return () => {
      isMounted = false
    }
  }, [activeCategory, isLoading])

  return (
    <div className="grid gap-6">
      <PageHeader
        title="Usluge"
        subtitle="Pregledajte usluge salona i odaberite tretman koji vam odgovara."
      />

      <SectionCard>
        <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
              Kategorije
            </p>
            <h2 className="mt-3 text-2xl font-black text-stone-50">
              Katalog usluga
            </h2>
          </div>
          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              onClick={() => setActiveCategory(allCategoriesValue)}
              className={`rounded-2xl border px-4 py-2.5 text-sm font-semibold transition ${
                activeCategory === allCategoriesValue
                  ? 'border-amber-200/40 bg-amber-100/10 text-amber-100'
                  : 'border-amber-200/10 bg-black/25 text-stone-300 hover:border-amber-200/25 hover:text-amber-100'
              }`}
            >
              Sve
            </button>
            {categories.map((category) => (
              <button
                key={category.id}
                type="button"
                onClick={() => setActiveCategory(String(category.id))}
                className={`rounded-2xl border px-4 py-2.5 text-sm font-semibold transition ${
                  activeCategory === String(category.id)
                    ? 'border-amber-200/40 bg-amber-100/10 text-amber-100'
                    : 'border-amber-200/10 bg-black/25 text-stone-300 hover:border-amber-200/25 hover:text-amber-100'
                }`}
              >
                {category.name}
              </button>
            ))}
          </div>
        </div>

        {isLoading && (
          <p className="mt-6 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm text-stone-300">
            Učitavanje usluga...
          </p>
        )}

        {!isLoading && categories.length === 0 && (
          <p className="mt-6 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm text-stone-300">
            Još nisu dodane kategorije usluga.
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
              title="Trenutno nema dostupnih usluga."
              description="Kada salon doda aktivne usluge, bit će prikazane ovdje."
            />
          </div>
        )}

        {!isLoading && services.length > 0 && (
          <>
            {isFiltering && (
              <p className="mt-6 text-sm text-stone-400">Učitavanje usluga...</p>
            )}
            <div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
              {services.map((service) => (
                <AppCard
                  key={service.id}
                  className="flex min-h-[410px] flex-col"
                  variant="interactive"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/65">
                        {service.categoryName || 'Usluga'}
                      </p>
                      <h3 className="mt-3 text-xl font-black text-stone-50">
                        {service.name}
                      </h3>
                    </div>
                    {service.active && (
                      <StatusBadge label="Dostupno" tone="success" />
                    )}
                  </div>

                  <p className="mt-4 flex-1 text-sm leading-6 text-stone-400">
                    {service.description ||
                      'Opis usluge će biti prikazan nakon dopune podataka.'}
                  </p>

                  <div className="mt-5 grid grid-cols-2 gap-3">
                    <div className="rounded-2xl border border-amber-200/10 bg-white/[0.035] px-4 py-3">
                      <p className="text-[10px] font-bold uppercase tracking-[0.16em] text-stone-500">
                        Trajanje
                      </p>
                      <p className="mt-1 font-semibold text-stone-100">
                        {service.durationMinutes} min
                      </p>
                    </div>
                    <div className="rounded-2xl border border-amber-200/10 bg-white/[0.035] px-4 py-3">
                      <p className="text-[10px] font-bold uppercase tracking-[0.16em] text-stone-500">
                        Cijena
                      </p>
                      <p className="mt-1 font-semibold text-amber-100">
                        {formatPrice(service.price)}
                      </p>
                    </div>
                  </div>

                  <Link
                    to="/app/book-appointment"
                    state={{ selectedServiceId: String(service.id) }}
                    className={`mt-5 ${buttonStyles.secondary}`}
                  >
                    Zakaži
                  </Link>
                </AppCard>
              ))}
            </div>
          </>
        )}
      </SectionCard>
    </div>
  )
}

export default ServicesPage
