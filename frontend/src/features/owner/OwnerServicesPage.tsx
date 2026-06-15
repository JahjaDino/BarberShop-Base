import { useEffect, useMemo, useState } from 'react'
import {
  createOwnerService,
  createOwnerServiceCategory,
  getOwnerServiceCategories,
  getOwnerServices,
  updateOwnerService,
  updateOwnerServiceCategory,
} from '../../api/servicesApi'
import AppCard from '../../components/common/AppCard'
import AppSelect from '../../components/common/AppSelect'
import EmptyState from '../../components/common/EmptyState'
import PageHeader from '../../components/common/PageHeader'
import SectionCard from '../../components/common/SectionCard'
import StatusBadge from '../../components/common/StatusBadge'
import { buttonStyles } from '../../components/common/buttonStyles'
import type {
  OwnerServiceListItem,
  ServiceCategoryDto,
  ServiceDto,
} from '../../types/services'

interface CategoryFormState {
  name: string
  description: string
  active: boolean
}

interface ServiceFormState {
  name: string
  categoryId: string
  description: string
  durationMinutes: string
  price: string
  active: boolean
}

const initialCategoryForm: CategoryFormState = {
  name: '',
  description: '',
  active: true,
}

const initialServiceForm: ServiceFormState = {
  name: '',
  categoryId: '',
  description: '',
  durationMinutes: '',
  price: '',
  active: true,
}

function formatPrice(price: number) {
  return `${price.toFixed(price % 1 === 0 ? 0 : 2)} KM`
}

function mapServiceResponse(service: ServiceDto): OwnerServiceListItem {
  return {
    serviceId: service.id,
    categoryId: service.categoryId,
    name: service.name,
    description: service.description,
    categoryName: service.categoryName,
    durationMinutes: service.durationMinutes,
    price: service.price,
    bookingsCount: 0,
    isActive: service.active,
  }
}

function mapCategoryToForm(category: ServiceCategoryDto): CategoryFormState {
  return {
    name: category.name,
    description: category.description ?? '',
    active: category.active,
  }
}

function mapServiceToForm(service: OwnerServiceListItem): ServiceFormState {
  return {
    name: service.name,
    categoryId: String(service.categoryId),
    description: service.description ?? '',
    durationMinutes: String(service.durationMinutes),
    price: String(service.price),
    active: service.isActive,
  }
}

function validateCategoryForm(form: CategoryFormState) {
  if (!form.name.trim()) return 'Naziv kategorije je obavezan.'
  return ''
}

function validateServiceForm(form: ServiceFormState) {
  const duration = Number(form.durationMinutes)
  const price = Number(form.price)

  if (!form.name.trim()) return 'Naziv usluge je obavezan.'
  if (!form.categoryId) return 'Kategorija je obavezna.'
  if (!form.description.trim()) return 'Opis usluge je obavezan.'
  if (!Number.isFinite(duration) || duration <= 0 || duration > 480) {
    return 'Trajanje mora biti izmedu 1 i 480 minuta.'
  }
  if (!Number.isFinite(price) || price < 0 || price > 1000) {
    return 'Cijena mora biti izmedu 0 i 1000 KM.'
  }

  return ''
}

function OwnerServicesPage() {
  const [categories, setCategories] = useState<ServiceCategoryDto[]>([])
  const [services, setServices] = useState<OwnerServiceListItem[]>([])
  const [categoryForm, setCategoryForm] =
    useState<CategoryFormState>(initialCategoryForm)
  const [serviceForm, setServiceForm] =
    useState<ServiceFormState>(initialServiceForm)
  const [editingCategoryId, setEditingCategoryId] = useState<number | null>(null)
  const [editingServiceId, setEditingServiceId] = useState<number | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isSavingCategory, setIsSavingCategory] = useState(false)
  const [isSavingService, setIsSavingService] = useState(false)
  const [error, setError] = useState('')
  const [categoryError, setCategoryError] = useState('')
  const [serviceError, setServiceError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')

  const activeCategories = useMemo(
    () => categories.filter((category) => category.active),
    [categories],
  )

  const categoryOptions = activeCategories.map((category) => ({
    label: category.name,
    value: String(category.id),
  }))

  const selectedCategory = categories.find(
    (category) => String(category.id) === serviceForm.categoryId,
  )
  const previewName = serviceForm.name.trim()
  const previewDescription = serviceForm.description.trim()
  const previewDuration = serviceForm.durationMinutes
  const previewPrice = serviceForm.price
  const previewCategoryName = selectedCategory?.name

  async function loadServicesData() {
    setIsLoading(true)
    setError('')

    try {
      const [categoryItems, servicesResponse] = await Promise.all([
        getOwnerServiceCategories(),
        getOwnerServices(),
      ])

      setCategories(categoryItems)
      setServices(servicesResponse.items)
      setServiceForm((currentForm) => ({
        ...currentForm,
        categoryId:
          currentForm.categoryId ||
          String(categoryItems.find((category) => category.active)?.id ?? ''),
      }))
    } catch (apiError) {
      setError(
        apiError instanceof Error
          ? apiError.message
          : 'Usluge trenutno nisu dostupne. Pokušajte ponovo kasnije.',
      )
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    loadServicesData()
  }, [])

  function handleCategoryFieldChange(
    field: keyof CategoryFormState,
    value: string | boolean,
  ) {
    setCategoryForm((currentForm) => ({ ...currentForm, [field]: value }))
    setCategoryError('')
    setSuccessMessage('')
  }

  function handleServiceFieldChange(
    field: keyof ServiceFormState,
    value: string | boolean,
  ) {
    setServiceForm((currentForm) => ({ ...currentForm, [field]: value }))
    setServiceError('')
    setSuccessMessage('')
  }

  function resetCategoryForm() {
    setEditingCategoryId(null)
    setCategoryForm(initialCategoryForm)
    setCategoryError('')
  }

  function resetServiceForm() {
    setEditingServiceId(null)
    setServiceForm({
      ...initialServiceForm,
      categoryId: '',
    })
    setServiceError('')
  }

  function handleEditCategory(category: ServiceCategoryDto) {
    setEditingCategoryId(category.id)
    setCategoryForm(mapCategoryToForm(category))
    setCategoryError('')
    setSuccessMessage('')
  }

  function handleEditService(service: OwnerServiceListItem) {
    setEditingServiceId(service.serviceId)
    setServiceForm(mapServiceToForm(service))
    setServiceError('')
    setSuccessMessage('')
  }

  async function saveCategory(nextActive?: boolean) {
    const validationError = validateCategoryForm(categoryForm)
    if (validationError) {
      setCategoryError(validationError)
      return
    }

    try {
      setIsSavingCategory(true)
      setCategoryError('')
      setSuccessMessage('')

      const payload = {
        name: categoryForm.name.trim(),
        description: categoryForm.description.trim() || null,
        active: nextActive ?? categoryForm.active,
      }

      if (editingCategoryId) {
        const updatedCategory = await updateOwnerServiceCategory(
          editingCategoryId,
          payload,
        )

        setCategories((currentCategories) =>
          currentCategories.map((category) =>
            category.id === updatedCategory.id ? updatedCategory : category,
          ),
        )
        setSuccessMessage('Kategorija je uspjesno azurirana.')
        resetCategoryForm()
        return
      }

      const createdCategory = await createOwnerServiceCategory({
        name: payload.name,
        description: payload.description,
      })

      setCategories((currentCategories) => [...currentCategories, createdCategory])
      setServiceForm((currentForm) => ({
        ...currentForm,
        categoryId: currentForm.categoryId || String(createdCategory.id),
      }))
      setCategoryForm(initialCategoryForm)
      setSuccessMessage('Kategorija je uspjesno dodana.')
    } catch (apiError) {
      setCategoryError(
        apiError instanceof Error
          ? apiError.message
          : 'Kategoriju trenutno nije moguće sačuvati.',
      )
    } finally {
      setIsSavingCategory(false)
    }
  }

  async function toggleCategory(category: ServiceCategoryDto) {
    try {
      setError('')
      setSuccessMessage('')

      const updatedCategory = await updateOwnerServiceCategory(category.id, {
        name: category.name,
        description: category.description ?? null,
        active: !category.active,
      })

      setCategories((currentCategories) =>
        currentCategories.map((currentCategory) =>
          currentCategory.id === updatedCategory.id
            ? updatedCategory
            : currentCategory,
        ),
      )
      setSuccessMessage(
        updatedCategory.active
          ? 'Kategorija je aktivirana.'
          : 'Kategorija je deaktivirana.',
      )
    } catch (apiError) {
      setError(
        apiError instanceof Error
          ? apiError.message
          : 'Status kategorije trenutno nije moguće promijeniti.',
      )
    }
  }

  async function saveService(nextActive?: boolean) {
    const validationError = validateServiceForm(serviceForm)
    if (validationError) {
      setServiceError(validationError)
      return
    }

    try {
      setIsSavingService(true)
      setServiceError('')
      setSuccessMessage('')

      const payload = {
        categoryId: Number(serviceForm.categoryId),
        name: serviceForm.name.trim(),
        description: serviceForm.description.trim(),
        durationMinutes: Number(serviceForm.durationMinutes),
        price: Number(serviceForm.price),
        active: nextActive ?? serviceForm.active,
      }

      if (editingServiceId) {
        const updatedService = await updateOwnerService(editingServiceId, payload)
        const mappedService = mapServiceResponse(updatedService)

        setServices((currentServices) =>
          currentServices.map((service) =>
            service.serviceId === mappedService.serviceId ? mappedService : service,
          ),
        )
        resetServiceForm()
        setSuccessMessage('Usluga je uspjesno azurirana.')
        return
      }

      const createdService = await createOwnerService({
        categoryId: payload.categoryId,
        name: payload.name,
        description: payload.description,
        durationMinutes: payload.durationMinutes,
        price: payload.price,
      })
      const mappedService = mapServiceResponse(createdService)

      setServices((currentServices) => [mappedService, ...currentServices])
      setServiceForm({
        ...initialServiceForm,
        categoryId: '',
      })
      setEditingServiceId(null)
      setSuccessMessage('Usluga je uspjesno dodana.')
    } catch (apiError) {
      setServiceError(
        apiError instanceof Error
          ? apiError.message
          : 'Uslugu trenutno nije moguće sačuvati.',
      )
    } finally {
      setIsSavingService(false)
    }
  }

  async function toggleService(service: OwnerServiceListItem) {
    try {
      setError('')
      setSuccessMessage('')

      const updatedService = await updateOwnerService(service.serviceId, {
        categoryId: service.categoryId,
        name: service.name,
        description: service.description ?? null,
        durationMinutes: service.durationMinutes,
        price: service.price,
        active: !service.isActive,
      })
      const mappedService = mapServiceResponse(updatedService)

      setServices((currentServices) =>
        currentServices.map((currentService) =>
          currentService.serviceId === mappedService.serviceId
            ? mappedService
            : currentService,
        ),
      )
      setSuccessMessage(
        mappedService.isActive ? 'Usluga je aktivirana.' : 'Usluga je deaktivirana.',
      )
    } catch (apiError) {
      setError(
        apiError instanceof Error
          ? apiError.message
          : 'Status usluge trenutno nije moguće promijeniti.',
      )
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader
        eyebrow="Vlasnicki prostor"
        title="Usluge salona"
        subtitle="Upravljajte kategorijama, ponudom, trajanjem i cijenama usluga."
      />

      {successMessage && (
        <p className="rounded-2xl border border-emerald-300/20 bg-emerald-300/10 px-4 py-3 text-sm font-semibold text-emerald-100">
          {successMessage}
        </p>
      )}

      {error && (
        <p className="rounded-2xl border border-red-300/20 bg-red-400/10 px-4 py-3 text-sm font-semibold text-red-100">
          {error}
        </p>
      )}

      <section className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_380px]">
        <SectionCard>
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                Kategorije usluga
              </p>
              <h2 className="mt-3 text-2xl font-black text-stone-50">
                Kategorije
              </h2>
            </div>
            <StatusBadge label={`${categories.length} kategorija`} />
          </div>

          {isLoading ? (
            <p className="mt-6 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm text-stone-300">
              Učitavanje kategorija...
            </p>
          ) : categories.length === 0 ? (
            <div className="mt-6">
              <EmptyState
                title="Jos niste dodali kategorije."
                description="Dodajte prvu kategoriju kako biste mogli dodati usluge."
              />
            </div>
          ) : (
            <div className="mt-5 grid gap-3 md:grid-cols-2">
              {categories.map((category) => (
                <article
                  key={category.id}
                  className="rounded-2xl border border-amber-200/10 bg-black/25 p-4"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <h3 className="font-bold text-stone-50">{category.name}</h3>
                      <p className="mt-2 text-sm leading-6 text-stone-400">
                        {category.description || 'Opis kategorije nije dodan.'}
                      </p>
                    </div>
                    <StatusBadge
                      label={category.active ? 'Aktivna' : 'Neaktivna'}
                      tone={category.active ? 'success' : 'neutral'}
                    />
                  </div>
                  <div className="mt-4 flex flex-wrap gap-2">
                    <button
                      type="button"
                      onClick={() => handleEditCategory(category)}
                      className={buttonStyles.ghost}
                    >
                      Uredi
                    </button>
                    <button
                      type="button"
                      onClick={() => toggleCategory(category)}
                      className={buttonStyles.secondary}
                    >
                      {category.active ? 'Deaktiviraj' : 'Aktiviraj'}
                    </button>
                  </div>
                </article>
              ))}
            </div>
          )}
        </SectionCard>

        <SectionCard>
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
            {editingCategoryId ? 'Uredi kategoriju' : 'Nova kategorija'}
          </p>
          <h2 className="mt-3 text-2xl font-black text-stone-50">
            {editingCategoryId ? 'Azuriraj kategoriju' : 'Dodaj kategoriju'}
          </h2>

          <div className="mt-5 grid min-w-0 gap-4">
            <label className="grid min-w-0 gap-2 text-sm font-semibold text-stone-300">
              Naziv kategorije
              <input
                value={categoryForm.name}
                onChange={(event) =>
                  handleCategoryFieldChange('name', event.target.value)
                }
                className="w-full min-w-0 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                placeholder="npr. Sisanje"
              />
            </label>

            <label className="grid min-w-0 gap-2 text-sm font-semibold text-stone-300">
              Opis
              <textarea
                value={categoryForm.description}
                onChange={(event) =>
                  handleCategoryFieldChange('description', event.target.value)
                }
                rows={3}
                className="w-full min-w-0 resize-none rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                placeholder="Kratak opis kategorije."
              />
            </label>

            {editingCategoryId && (
              <label className="flex items-center gap-3 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm font-semibold text-stone-300">
                <input
                  type="checkbox"
                  checked={categoryForm.active}
                  onChange={(event) =>
                    handleCategoryFieldChange('active', event.target.checked)
                  }
                  className="h-4 w-4 accent-amber-200"
                />
                Aktivna kategorija
              </label>
            )}
          </div>

          {categoryError && (
            <p className="mt-4 rounded-2xl border border-red-300/20 bg-red-400/10 px-4 py-3 text-sm font-semibold text-red-100">
              {categoryError}
            </p>
          )}

          <div className="mt-5 flex flex-wrap gap-3">
            <button
              type="button"
              disabled={isSavingCategory}
              onClick={() => saveCategory()}
              className={buttonStyles.primary}
            >
              {isSavingCategory
                ? 'Spremanje...'
                : editingCategoryId
                  ? 'Sacuvaj izmjene'
                  : 'Dodaj kategoriju'}
            </button>
            {editingCategoryId && (
              <button
                type="button"
                onClick={resetCategoryForm}
                className={buttonStyles.ghost}
              >
                Odustani
              </button>
            )}
          </div>
        </SectionCard>
      </section>

      <section className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_380px]">
        <SectionCard>
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
                Usluge
              </p>
              <h2 className="mt-3 text-2xl font-black text-stone-50">
                Lista usluga
              </h2>
            </div>
            <StatusBadge label={`${services.length} usluga`} />
          </div>

          {isLoading ? (
            <p className="mt-6 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm text-stone-300">
              Učitavanje usluga...
            </p>
          ) : services.length === 0 ? (
            <div className="mt-6">
              <EmptyState
                title="Jos niste dodali nijednu uslugu."
                description="Dodajte kategoriju, zatim kreirajte uslugu za klijente."
              />
            </div>
          ) : (
            <div className="mt-5 grid gap-4">
              {services.map((service) => (
                <AppCard key={service.serviceId} variant="interactive">
                  <div className="grid min-w-0 gap-5 xl:grid-cols-[minmax(0,1.25fr)_minmax(320px,0.8fr)_minmax(240px,auto)] xl:items-center">
                    <div className="min-w-0">
                      <div className="flex flex-wrap items-center gap-2">
                        <h3 className="truncate text-xl font-black text-stone-50">
                          {service.name}
                        </h3>
                        <StatusBadge
                          label={service.isActive ? 'Aktivna' : 'Neaktivna'}
                          tone={service.isActive ? 'success' : 'neutral'}
                        />
                      </div>
                      <p className="mt-2 line-clamp-2 text-sm leading-6 text-stone-400">
                        {service.description || 'Opis usluge nije dodan.'}
                      </p>
                    </div>

                    <div className="grid gap-2 rounded-2xl border border-amber-200/10 bg-white/[0.035] px-4 py-3 text-sm sm:grid-cols-3">
                      <span className="min-w-0 truncate text-stone-200">
                        {service.categoryName || 'Kategorija'}
                      </span>
                      <span className="text-stone-300">
                        {service.durationMinutes} min
                      </span>
                      <span className="font-semibold text-amber-100">
                        {formatPrice(service.price)}
                      </span>
                    </div>

                    <div className="flex flex-wrap gap-2 xl:justify-end">
                      <button
                        type="button"
                        onClick={() => handleEditService(service)}
                        className={buttonStyles.ghost}
                      >
                        Uredi
                      </button>
                      <button
                        type="button"
                        onClick={() => toggleService(service)}
                        className={buttonStyles.secondary}
                      >
                        {service.isActive ? 'Deaktiviraj' : 'Aktiviraj'}
                      </button>
                    </div>
                  </div>
                </AppCard>
              ))}
            </div>
          )}
        </SectionCard>

        <SectionCard>
          <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
            {editingServiceId ? 'Uredi uslugu' : 'Nova usluga'}
          </p>
          <h2 className="mt-3 text-2xl font-black text-stone-50">
            {editingServiceId ? 'Azuriraj uslugu' : 'Dodaj uslugu'}
          </h2>

          {activeCategories.length === 0 && !isLoading && (
            <p className="mt-5 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm text-stone-300">
              Prvo dodajte aktivnu kategoriju usluge.
            </p>
          )}

          <div className="mt-5 grid min-w-0 gap-4">
            <label className="grid min-w-0 gap-2 text-sm font-semibold text-stone-300">
              Naziv usluge
              <input
                value={serviceForm.name}
                onChange={(event) =>
                  handleServiceFieldChange('name', event.target.value)
                }
                className="w-full min-w-0 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                placeholder="npr. Fade sisanje"
              />
            </label>

            <AppSelect
              disabled={activeCategories.length === 0}
              label="Kategorija"
              value={serviceForm.categoryId}
              options={categoryOptions}
              placeholder="Odaberite kategoriju"
              onChange={(value) => handleServiceFieldChange('categoryId', value)}
            />

            <label className="grid min-w-0 gap-2 text-sm font-semibold text-stone-300">
              Opis
              <textarea
                value={serviceForm.description}
                onChange={(event) =>
                  handleServiceFieldChange('description', event.target.value)
                }
                rows={3}
                className="w-full min-w-0 resize-none rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                placeholder="Kratak opis koji ce klijent vidjeti u katalogu."
              />
            </label>

            <div className="grid min-w-0 grid-cols-1 gap-4 sm:grid-cols-2">
              <label className="grid min-w-0 gap-2 text-sm font-semibold text-stone-300">
                Trajanje u minutama
                <input
                  value={serviceForm.durationMinutes}
                  onChange={(event) =>
                    handleServiceFieldChange('durationMinutes', event.target.value)
                  }
                  inputMode="numeric"
                  className="w-full min-w-0 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                  placeholder="45"
                />
              </label>

              <label className="grid min-w-0 gap-2 text-sm font-semibold text-stone-300">
                Cijena u KM
                <input
                  value={serviceForm.price}
                  onChange={(event) =>
                    handleServiceFieldChange('price', event.target.value)
                  }
                  inputMode="decimal"
                  className="w-full min-w-0 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-stone-100 outline-none transition focus:border-amber-200/35"
                  placeholder="35"
                />
              </label>
            </div>

            {editingServiceId && (
              <label className="flex items-center gap-3 rounded-2xl border border-amber-200/10 bg-black/25 px-4 py-3 text-sm font-semibold text-stone-300">
                <input
                  type="checkbox"
                  checked={serviceForm.active}
                  onChange={(event) =>
                    handleServiceFieldChange('active', event.target.checked)
                  }
                  className="h-4 w-4 accent-amber-200"
                />
                Aktivna usluga
              </label>
            )}
          </div>

          {serviceError && (
            <p className="mt-4 rounded-2xl border border-red-300/20 bg-red-400/10 px-4 py-3 text-sm font-semibold text-red-100">
              {serviceError}
            </p>
          )}

          <div className="mt-5 flex flex-wrap gap-3">
            <button
              type="button"
              disabled={isSavingService || activeCategories.length === 0}
              onClick={() => saveService()}
              className={buttonStyles.primary}
            >
              {isSavingService
                ? 'Spremanje...'
                : editingServiceId
                  ? 'Sacuvaj izmjene'
                  : 'Dodaj uslugu'}
            </button>
            {editingServiceId && (
              <button
                type="button"
                onClick={resetServiceForm}
                className={buttonStyles.ghost}
              >
                Odustani
              </button>
            )}
          </div>

          <AppCard className="mt-5 min-w-0" variant="default">
            <p className="text-xs font-semibold uppercase tracking-[0.22em] text-amber-200/65">
              {previewCategoryName ?? 'Kategorija'}
            </p>
            <h3 className="mt-3 break-words text-xl font-black text-stone-50">
              {previewName || 'Naziv usluge'}
            </h3>
            <p className="mt-4 break-words text-sm leading-6 text-stone-400">
              {previewDescription || 'Opis koji ce klijent vidjeti u katalogu usluga.'}
            </p>
            <div className="mt-5 grid min-w-0 grid-cols-1 gap-3 sm:grid-cols-2">
              <div className="min-w-0 rounded-2xl border border-amber-200/10 bg-white/[0.035] px-4 py-3">
                <p className="text-[10px] font-bold uppercase tracking-[0.16em] text-stone-500">
                  Trajanje
                </p>
                <p className="mt-1 break-words font-semibold text-stone-100">
                  {previewDuration || '0'} min
                </p>
              </div>
              <div className="min-w-0 rounded-2xl border border-amber-200/10 bg-white/[0.035] px-4 py-3">
                <p className="text-[10px] font-bold uppercase tracking-[0.16em] text-stone-500">
                  Cijena
                </p>
                <p className="mt-1 break-words font-semibold text-amber-100">
                  {previewPrice || '0'} KM
                </p>
              </div>
            </div>
          </AppCard>
        </SectionCard>
      </section>
    </div>
  )
}

export default OwnerServicesPage
