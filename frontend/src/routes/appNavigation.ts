import type { NavigationGroup, PageMetadata } from '../types/navigation'
import type { UserRole } from '../types/auth'

export const clientNavigationGroups: NavigationGroup[] = [
  {
    label: 'Glavno',
    items: [
      { label: 'Pregled', path: '/app/dashboard' },
      { label: 'Zakazi termin', path: '/app/book-appointment' },
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
      { label: 'Frizerke', path: '/app/owner/employees' },
      { label: 'Odsustva', path: '/app/owner/time-off' },
      { label: 'Usluge', path: '/app/owner/services' },
    ],
  },
  {
    label: 'Profil',
    items: [{ label: 'Profil vlasnice', path: '/app/owner/settings' }],
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
    items: [{ label: 'Profil frizerke', path: '/app/employee/profile' }],
  },
]

export function getNavigationForRole(role: UserRole) {
  if (role === 'OWNER') {
    return {
      workspaceLabel: 'Vlasnicki prostor',
      navigationGroups: ownerNavigationGroups,
    }
  }

  if (role === 'EMPLOYEE') {
    return {
      workspaceLabel: 'Studio prostor',
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
    subtitle: 'Vas prostor za termine, usluge i brze rezervacije.',
  },
  '/app/book-appointment': {
    title: 'Zakazi termin',
    subtitle: 'Odaberite uslugu, frizerku, datum i slobodan termin.',
  },
  '/app/my-appointments': {
    title: 'Moji termini',
    subtitle: 'Pregled nadolazecih i prethodnih rezervacija.',
  },
  '/app/services': {
    title: 'Usluge',
    subtitle: 'Katalog usluga za frizure, farbanje, pramenove i njegu kose.',
  },
  '/app/profile': {
    title: 'Moj profil',
    subtitle: 'Upravljajte licnim podacima i sigurnoscu svog racuna.',
  },
  '/app/owner/dashboard': {
    title: 'Pregled salona',
    subtitle: 'Pratite termine, frizerke i poslovanje salona na jednom mjestu.',
  },
  '/app/owner/appointments': {
    title: 'Termini',
    subtitle: 'Pregled i organizacija termina u salonu.',
  },
  '/app/owner/employees': {
    title: 'Frizerke',
    subtitle: 'Tim, dostupnost i raspored frizerki.',
  },
  '/app/owner/time-off': {
    title: 'Odsustva',
    subtitle: 'Pregled i obrada zahtjeva frizerki za odsustvo.',
  },
  '/app/owner/services': {
    title: 'Usluge salona',
    subtitle: 'Upravljanje ponudom, trajanjem i cijenama usluga.',
  },
  '/app/owner/settings': {
    title: 'Profil vlasnice',
    subtitle: 'Upravljajte licnim podacima i sigurnoscu vlasnickog racuna.',
  },
  '/app/employee/dashboard': {
    title: 'Moj radni dan',
    subtitle: 'Pregled rasporeda, termina i dostupnosti.',
  },
  '/app/employee/schedule': {
    title: 'Moj raspored',
    subtitle: 'Dnevni i sedmicni pregled radnog vremena.',
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
    title: 'Profil frizerke',
    subtitle: 'Upravljajte licnim podacima i sigurnoscu svog racuna.',
  },
}
