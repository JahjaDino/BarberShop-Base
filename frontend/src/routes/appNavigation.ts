import type { NavigationGroup, PageMetadata } from '../types/navigation'
import type { UserRole } from '../types/auth'

export const clientNavigationGroups: NavigationGroup[] = [
  {
    label: 'Glavno',
    items: [
      { label: 'Pregled', path: '/app/dashboard' },
      { label: 'Zakaži termin', path: '/app/book-appointment' },
      { label: 'Moji termini', path: '/app/my-appointments' },
    ],
  },
  {
    label: 'Salon',
    items: [{ label: 'Usluge', path: '/app/services' }],
  },
  {
    label: 'Profil',
    items: [{ label: 'Moj profil', path: '/app/profile' }],
  },
]

export const ownerNavigationGroups: NavigationGroup[] = [
  {
    label: 'Pregled',
    items: [{ label: 'Pregled', path: '/app/owner/dashboard' }],
  },
  {
    label: 'Salon',
    items: [
      { label: 'Termini', path: '/app/owner/appointments' },
      { label: 'Frizeri', path: '/app/owner/employees' },
      { label: 'Odsustva', path: '/app/owner/time-off' },
      { label: 'Usluge', path: '/app/owner/services' },
    ],
  },
  {
    label: 'Profil',
    items: [{ label: 'Profil vlasnika', path: '/app/owner/settings' }],
  },
]

export const employeeNavigationGroups: NavigationGroup[] = [
  {
    label: 'Pregled',
    items: [{ label: 'Pregled', path: '/app/employee/dashboard' }],
  },
  {
    label: 'Radni dan',
    items: [
      { label: 'Moj raspored', path: '/app/employee/schedule' },
      { label: 'Dodijeljeni termini', path: '/app/employee/appointments' },
      { label: 'Odsustva', path: '/app/employee/time-off' },
    ],
  },
  {
    label: 'Salon',
    items: [{ label: 'Usluge', path: '/app/employee/services' }],
  },
  {
    label: 'Profil',
    items: [{ label: 'Profil frizera', path: '/app/employee/profile' }],
  },
]

export function getNavigationForRole(role: UserRole) {
  if (role === 'OWNER') {
    return {
      workspaceLabel: 'Vlasnički prostor',
      navigationGroups: ownerNavigationGroups,
    }
  }

  if (role === 'EMPLOYEE') {
    return {
      workspaceLabel: 'Frizerski prostor',
      navigationGroups: employeeNavigationGroups,
    }
  }

  return {
    workspaceLabel: 'Moj prostor',
    navigationGroups: clientNavigationGroups,
  }
}

export const pageMetadata: Record<string, PageMetadata> = {
  '/app/dashboard': {
    title: 'Pregled',
    subtitle: 'Vaš prostor za termine, usluge i brze rezervacije.',
  },
  '/app/book-appointment': {
    title: 'Zakaži termin',
    subtitle: 'Odaberite uslugu, frizera, datum i slobodan termin.',
  },
  '/app/my-appointments': {
    title: 'Moji termini',
    subtitle: 'Pregled nadolazećih i prethodnih rezervacija.',
  },
  '/app/services': {
    title: 'Usluge',
    subtitle: 'Katalog barbershop usluga i kategorija.',
  },
  '/app/profile': {
    title: 'Moj profil',
    subtitle: 'Upravljajte ličnim podacima i sigurnošću svog računa.',
  },
  '/app/owner/dashboard': {
    title: 'Pregled salona',
    subtitle: 'Pratite termine, frizere i poslovanje salona na jednom mjestu.',
  },
  '/app/owner/appointments': {
    title: 'Termini',
    subtitle: 'Pregled i organizacija termina u salonu.',
  },
  '/app/owner/employees': {
    title: 'Frizeri',
    subtitle: 'Tim, dostupnost i raspored frizera.',
  },
  '/app/owner/time-off': {
    title: 'Odsustva',
    subtitle: 'Pregled i obrada zahtjeva frizera za odsustvo.',
  },
  '/app/owner/services': {
    title: 'Usluge salona',
    subtitle: 'Upravljanje ponudom, trajanjem i cijenama usluga.',
  },
  '/app/owner/settings': {
    title: 'Profil vlasnika',
    subtitle: 'Upravljajte ličnim podacima i sigurnošću vlasničkog računa.',
  },
  '/app/employee/dashboard': {
    title: 'Moj radni dan',
    subtitle: 'Pregled rasporeda, termina i dostupnosti.',
  },
  '/app/employee/schedule': {
    title: 'Moj raspored',
    subtitle: 'Dnevni i sedmični pregled radnog vremena.',
  },
  '/app/employee/appointments': {
    title: 'Dodijeljeni termini',
    subtitle: 'Termini koji su dodijeljeni vama.',
  },
  '/app/employee/time-off': {
    title: 'Odsustva',
    subtitle: 'Zahtjevi za odsustvo i dostupnost.',
  },
  '/app/employee/services': {
    title: 'Usluge',
    subtitle: 'Usluge koje obavljate u salonu.',
  },
  '/app/employee/profile': {
    title: 'Profil frizera',
    subtitle: 'Upravljajte ličnim podacima i sigurnošću svog računa.',
  },
}
