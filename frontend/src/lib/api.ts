export interface ArtworkSummary {
  id: number
  title: string
  artistName: string | null
  dateDisplay: string | null
  dateStartYear: number | null
  era: string
  mediumCategory: string
  thumbnailUrl: string | null
  isPublicDomain: boolean
}

export interface FacetBucket {
  value: string
  count: number
}

export interface ArtworkSearchResult {
  items: ArtworkSummary[]
  total: number
  page: number
  pageSize: number
  totalPages: number
  facets: Record<string, FacetBucket[]>
}

export type ArtworkSort = 'Relevance' | 'DateAsc' | 'DateDesc' | 'TitleAsc' | 'TitleDesc'

export interface SearchParams {
  q?: string
  era?: string[]
  medium?: string[]
  movement?: string[]
  artist?: string[]
  yearFrom?: number
  yearTo?: number
  sort?: ArtworkSort
  page?: number
  pageSize?: number
}

export interface ArtistSummary {
  id: number
  name: string
  nationality: string | null
  birthYear: number | null
  deathYear: number | null
}

export interface ArtworkDetail {
  id: number
  title: string
  description: string | null
  mediumDisplay: string | null
  mediumCategory: string
  department: string | null
  dimensions: string | null
  creditLine: string | null
  dateDisplay: string | null
  dateStartYear: number | null
  dateEndYear: number | null
  era: string
  imageUrl: string | null
  thumbnailUrl: string | null
  isPublicDomain: boolean
  sourceSystem: string
  sourceUrl: string | null
  artist: ArtistSummary | null
  movements: string[]
}

export async function getArtwork(id: number, signal?: AbortSignal): Promise<ArtworkDetail | null> {
  const response = await fetch(`/api/artworks/${id}`, { signal })

  if (response.status === 404) {
    return null
  }

  if (!response.ok) {
    throw new Error(`Artwork request failed with status ${response.status}`)
  }

  return (await response.json()) as ArtworkDetail
}

export async function searchArtworks(
  params: SearchParams,
  signal?: AbortSignal,
): Promise<ArtworkSearchResult> {
  const qs = new URLSearchParams()

  if (params.q) qs.set('q', params.q)
  for (const era of params.era ?? []) qs.append('era', era)
  for (const medium of params.medium ?? []) qs.append('medium', medium)
  for (const movement of params.movement ?? []) qs.append('movement', movement)
  for (const artist of params.artist ?? []) qs.append('artist', artist)
  if (params.yearFrom != null) qs.set('yearFrom', String(params.yearFrom))
  if (params.yearTo != null) qs.set('yearTo', String(params.yearTo))
  if (params.sort) qs.set('sort', params.sort)
  if (params.page) qs.set('page', String(params.page))
  if (params.pageSize) qs.set('pageSize', String(params.pageSize))

  const response = await fetch(`/api/artworks/search?${qs.toString()}`, { signal })

  if (!response.ok) {
    throw new Error(`Search request failed with status ${response.status}`)
  }

  return (await response.json()) as ArtworkSearchResult
}
