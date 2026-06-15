import type { BookingServiceOption } from './bookingTypes'

interface ServiceSelectionProps {
  services: BookingServiceOption[]
  selectedServiceId: string
  onSelect: (serviceId: string) => void
}

function ServiceSelection({
  services,
  selectedServiceId,
  onSelect,
}: ServiceSelectionProps) {
  return (
    <section className="min-w-0 rounded-[28px] border border-amber-200/10 bg-white/[0.035] p-5 backdrop-blur">
      <p className="text-xs font-semibold uppercase tracking-[0.28em] text-amber-200/70">
        Korak 1
      </p>
      <h2 className="mt-3 text-2xl font-black text-stone-50">
        Odaberite uslugu
      </h2>

      <div className="mt-5 grid min-w-0 gap-3 md:grid-cols-3">
        {services.map((service) => {
          const isSelected = selectedServiceId === service.id

          return (
            <button
              key={service.id}
              type="button"
              onClick={() => onSelect(service.id)}
              className={`flex min-h-[190px] flex-col rounded-2xl border p-4 text-left transition-all duration-300 ${
                isSelected
                  ? 'border-amber-200/50 bg-amber-100/10 text-stone-50 shadow-[0_0_30px_rgba(245,213,145,0.10)]'
                  : 'border-amber-200/10 bg-black/25 text-stone-300 hover:border-amber-200/30 hover:bg-black/35'
              }`}
            >
              <span className="block text-base font-bold">{service.name}</span>
              <span className="mt-2 block flex-1 text-sm leading-6 text-stone-400">
                {service.description}
              </span>
              <span className="mt-4 flex items-center justify-between text-xs font-semibold uppercase tracking-[0.18em] text-amber-100">
                <span>{service.duration}</span>
                <span>{service.price}</span>
              </span>
            </button>
          )
        })}
      </div>
    </section>
  )
}

export default ServiceSelection
