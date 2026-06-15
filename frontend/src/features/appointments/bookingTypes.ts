export interface BookingServiceOption {
  id: string
  shopId: number
  name: string
  duration: string
  price: string
  description: string
}

export interface BarberOption {
  id: string
  name: string
  specialty: string
}

export interface DateOption {
  value: string
  label: string
  day: string
}

export interface TimeSlot {
  value: string
  startTime: string
  endTime: string
  status: 'available' | 'busy'
}

export type PaymentMethod = 'CASH_ON_SITE'
