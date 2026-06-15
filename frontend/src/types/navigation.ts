export interface NavigationItem {
  label: string
  path: string
  description?: string
}

export interface NavigationGroup {
  label: string
  items: NavigationItem[]
}

export interface PageMetadata {
  title: string
  subtitle: string
}
