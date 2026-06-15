import { useEffect, useMemo, useState } from 'react'
import { useLocation } from 'react-router-dom'
import {
  bookAppointment,
  getAvailableSlots,
  type AvailableSlotDto,
} from '../../api/appointmentsApi'
import { getPublicEmployees } from '../../api/employeesApi'
import { getServices } from '../../api/servicesApi'
import BarberSelection from './BarberSelection'
import BookingSummary from './BookingSummary'
import type {
  BarberOption,
  BookingServiceOption,
  DateOption,
  TimeSlot,
} from './bookingTypes'
import DateCalendar from './DateCalendar'
import ServiceSelection from './ServiceSelection'
import TimeSlotGrid from './TimeSlotGrid'

interface BookingLocationState {
  selectedServiceId?: string
}

const monthNames = [
  'Januar',
  'Februar',
  'Mart',
  'April',
  'Maj',
  'Juni',
  'Juli',
  'August',
  'Septembar',
  'Oktobar',
  'Novembar',
  'Decembar',
]

const dayLabels = ['Ned', 'Pon', 'Uto', 'Sri', 'Čet', 'Pet', 'Sub']

function createDateOption(date: Date): DateOption {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')

  return {
    value: `${year}-${month}-${day}`,
    day: dayLabels[date.getDay()],
    label: `${day}. ${monthNames[date.getMonth()]}`,
  }
}

function mapServiceToBookingOption(service: {
  id: number
  shopId: number
  name: string
  durationMinutes: number
  price: number
  description?: string | null
}): BookingServiceOption {
  return {
    id: String(service.id),
    shopId: service.shopId,
    name: service.name,
    duration: `${service.durationMinutes} min`,
    price: `${service.price.toFixed(service.price % 1 === 0 ? 0 : 2)} KM`,
    description:
      service.description ||
      'Opis usluge će biti prikazan nakon dopune podataka.',
  }
}

function formatSlotTime(value: string) {
  if (/^\d{2}:\d{2}/.test(value)) {
    return value.slice(0, 5)
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleTimeString('bs-BA', {
    hour: '2-digit',
    minute: '2-digit',
  })
}

function mapAvailableSlot(slot: AvailableSlotDto, date: string): TimeSlot {
  return {
    value: formatSlotTime(slot.startTime),
    startTime: `${date}T${formatSlotTime(slot.startTime)}:00Z`,
    endTime: `${date}T${formatSlotTime(slot.endTime)}:00Z`,
    status: slot.available ? 'available' : 'busy',
  }
}

function BookAppointmentPage() {
  const location = useLocation()
  const locationState = location.state as BookingLocationState | null
  const [services, setServices] = useState<BookingServiceOption[]>([])
  const [selectedServiceId, setSelectedServiceId] = useState('')
  const [barbers, setBarbers] = useState<BarberOption[]>([])
  const [selectedBarberId, setSelectedBarberId] = useState('')
  const [selectedDate, setSelectedDate] = useState<DateOption>(
    createDateOption(new Date()),
  )
  const [selectedTime, setSelectedTime] = useState('')
  const selectedPaymentMethod = 'CASH_ON_SITE' as const
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([])
  const [isBookingPrepared, setIsBookingPrepared] = useState(false)
  const [isLoadingServices, setIsLoadingServices] = useState(true)
  const [servicesError, setServicesError] = useState('')
  const [isLoadingBarbers, setIsLoadingBarbers] = useState(false)
  const [barbersError, setBarbersError] = useState('')
  const [isLoadingSlots, setIsLoadingSlots] = useState(false)
  const [slotsError, setSlotsError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [bookingMessage, setBookingMessage] = useState('')
  const [bookingError, setBookingError] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadServices() {
      setIsLoadingServices(true)
      setServicesError('')

      try {
        const response = await getServices({ active: true })
        const bookingServices = response.items.map(mapServiceToBookingOption)

        if (!isMounted) return

        setServices(bookingServices)
        setSelectedServiceId((currentServiceId) => {
          const requestedServiceId = locationState?.selectedServiceId
          const requestedServiceExists = bookingServices.some(
            (service) => service.id === requestedServiceId,
          )
          const currentServiceExists = bookingServices.some(
            (service) => service.id === currentServiceId,
          )

          if (requestedServiceId && requestedServiceExists) {
            return requestedServiceId
          }

          if (currentServiceExists) {
            return currentServiceId
          }

          return bookingServices[0]?.id ?? ''
        })
      } catch {
        if (isMounted) {
          setServicesError('Usluge trenutno nisu dostupne. Pokušajte ponovo kasnije.')
        }
      } finally {
        if (isMounted) {
          setIsLoadingServices(false)
        }
      }
    }

    loadServices()

    return () => {
      isMounted = false
    }
  }, [locationState?.selectedServiceId])

  const selectedService = useMemo(
    () => services.find((service) => service.id === selectedServiceId),
    [selectedServiceId, services],
  )
  const selectedBarber = useMemo(
    () => barbers.find((barber) => barber.id === selectedBarberId),
    [selectedBarberId, barbers],
  )
  const selectedSlot = useMemo(
    () => timeSlots.find((slot) => slot.value === selectedTime),
    [selectedTime, timeSlots],
  )

  useEffect(() => {
    let isMounted = true

    async function loadBarbersForSelectedService() {
      if (!selectedService?.shopId) {
        setBarbers([])
        setSelectedBarberId('')
        setBarbersError('')
        return
      }

      setIsLoadingBarbers(true)
      setBarbersError('')

      try {
        const publicEmployees = await getPublicEmployees(selectedService.shopId)
        const barberOptions = publicEmployees.map((employee) => ({
          id: String(employee.employeeId),
          name: employee.fullName,
          specialty: employee.specialization || 'Frizer',
        }))

        if (!isMounted) return

        setBarbers(barberOptions)
        setSelectedBarberId((currentBarberId) => {
          const currentBarberExists = barberOptions.some(
            (barber) => barber.id === currentBarberId,
          )

          return currentBarberExists ? currentBarberId : barberOptions[0]?.id ?? ''
        })
      } catch {
        if (isMounted) {
          setBarbers([])
          setSelectedBarberId('')
          setBarbersError(
            'Frizeri trenutno nisu dostupni. Pokušajte ponovo kasnije.',
          )
        }
      } finally {
        if (isMounted) {
          setIsLoadingBarbers(false)
        }
      }
    }

    loadBarbersForSelectedService()

    return () => {
      isMounted = false
    }
  }, [selectedService?.shopId])

  const resetSelectedTime = () => {
    setSelectedTime('')
    setIsBookingPrepared(false)
    setBookingMessage('')
    setBookingError('')
  }

  useEffect(() => {
    let isMounted = true

    async function loadAvailableSlots() {
      if (!selectedServiceId || !selectedBarberId || !selectedDate.value) {
        setTimeSlots([])
        setSlotsError('')
        return
      }

      setIsLoadingSlots(true)
      setSlotsError('')

      try {
        const slots = await getAvailableSlots(
          Number(selectedServiceId),
          Number(selectedBarberId),
          selectedDate.value,
        )

        if (!isMounted) return

        setTimeSlots(
          slots.map((slot) => mapAvailableSlot(slot, selectedDate.value)),
        )
      } catch {
        if (isMounted) {
          setTimeSlots([])
          setSlotsError(
            'Slobodni termini trenutno nisu dostupni. Pokušajte ponovo kasnije.',
          )
        }
      } finally {
        if (isMounted) {
          setIsLoadingSlots(false)
        }
      }
    }

    loadAvailableSlots()

    return () => {
      isMounted = false
    }
  }, [selectedServiceId, selectedBarberId, selectedDate.value])

  const handleServiceSelect = (serviceId: string) => {
    if (serviceId !== selectedServiceId) {
      setSelectedServiceId(serviceId)
      setSelectedBarberId('')
      resetSelectedTime()
    }
  }

  const handleBarberSelect = (barberId: string) => {
    if (barberId !== selectedBarberId) {
      setSelectedBarberId(barberId)
      resetSelectedTime()
    }
  }

  const handleDateSelect = (date: DateOption) => {
    if (date.value !== selectedDate.value) {
      setSelectedDate(date)
      resetSelectedTime()
    }
  }

  const handleTimeSelect = (time: string) => {
    setSelectedTime(time)
    setIsBookingPrepared(false)
    setBookingMessage('')
    setBookingError('')
  }

  const handleConfirmBooking = async () => {
    if (!selectedService || !selectedBarber || !selectedSlot) {
      return
    }

    setIsSubmitting(true)
    setBookingMessage('')
    setBookingError('')

    try {
      await bookAppointment({
        employeeId: Number(selectedBarber.id),
        serviceIds: [Number(selectedService.id)],
        startTime: selectedSlot.startTime,
        paymentMethod: selectedPaymentMethod,
      })

      setIsBookingPrepared(true)
      setBookingMessage(
        'Zahtjev za termin je poslan. Termin je trenutno na čekanju potvrde.',
      )
      setSelectedTime('')
      setTimeSlots((currentSlots) =>
        currentSlots.map((slot) =>
          slot.startTime === selectedSlot.startTime
            ? { ...slot, status: 'busy' }
            : slot,
        ),
      )
    } catch (error) {
      setBookingError(
        error instanceof Error
          ? error.message
          : 'Termin nije moguće zakazati. Pokušajte ponovo.',
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="grid min-w-0 gap-6">
      <section className="min-w-0 rounded-[32px] border border-amber-200/15 bg-black/25 p-6 shadow-[0_0_45px_rgba(0,0,0,0.28)] backdrop-blur-xl lg:p-8">
        <p className="text-xs font-semibold uppercase tracking-[0.32em] text-amber-200/75">
          Rezervacija
        </p>
        <h1 className="mt-4 max-w-3xl text-4xl font-black leading-tight text-stone-50 sm:text-5xl">
          Zakažite termin u nekoliko koraka
        </h1>
        <p className="mt-5 max-w-2xl text-base leading-8 text-stone-300">
          Odaberite uslugu, frizera i slobodan termin. Usluge se sada učitavaju
          iz backend kataloga salona.
        </p>
      </section>

      {isLoadingServices && (
        <section className="rounded-[28px] border border-amber-200/10 bg-white/[0.035] p-5 text-sm text-stone-300 backdrop-blur">
          Učitavanje usluga...
        </section>
      )}

      {servicesError && (
        <section className="rounded-[28px] border border-red-300/20 bg-red-400/10 p-5 text-sm font-semibold text-red-100">
          {servicesError}
        </section>
      )}

      {!isLoadingServices && !servicesError && services.length === 0 && (
        <section className="rounded-[28px] border border-amber-200/10 bg-white/[0.035] p-5 text-sm text-stone-300 backdrop-blur">
          Trenutno nema dostupnih usluga.
        </section>
      )}

      {!isLoadingServices && !servicesError && services.length > 0 && (
        <ServiceSelection
          services={services}
          selectedServiceId={selectedServiceId}
          onSelect={handleServiceSelect}
        />
      )}

      {isLoadingBarbers && (
        <section className="rounded-[28px] border border-amber-200/10 bg-white/[0.035] p-5 text-sm text-stone-300 backdrop-blur">
          Učitavanje frizera...
        </section>
      )}

      {barbersError && (
        <section className="rounded-[28px] border border-red-300/20 bg-red-400/10 p-5 text-sm font-semibold text-red-100">
          {barbersError}
        </section>
      )}

      {!isLoadingBarbers && !barbersError && (
        <BarberSelection
          barbers={barbers}
          selectedBarberId={selectedBarberId}
          onSelect={handleBarberSelect}
        />
      )}
      <DateCalendar
        selectedDate={selectedDate.value}
        onSelect={handleDateSelect}
      />

      {isLoadingSlots && (
        <section className="rounded-[28px] border border-amber-200/10 bg-white/[0.035] p-5 text-sm text-stone-300 backdrop-blur">
          Učitavanje slobodnih termina...
        </section>
      )}

      {slotsError && (
        <section className="rounded-[28px] border border-red-300/20 bg-red-400/10 p-5 text-sm font-semibold text-red-100">
          {slotsError}
        </section>
      )}

      <TimeSlotGrid
        slots={timeSlots}
        selectedTime={selectedTime}
        onSelect={handleTimeSelect}
      />
      <BookingSummary
        service={selectedService}
        barber={selectedBarber}
        date={selectedDate}
        time={selectedTime}
        paymentMethod={selectedPaymentMethod}
        isPrepared={isBookingPrepared}
        isSubmitting={isSubmitting}
        message={bookingMessage}
        error={bookingError}
        onConfirm={handleConfirmBooking}
      />
    </div>
  )
}

export default BookAppointmentPage
