import { useEffect, useState } from 'react'
import { searchArtworks, type ArtworkSearchResult, type ArtworkSort } from '../lib/api'
import { ArtworkCard } from './ArtworkCard'
import { FacetFilterGroup } from './FacetFilterGroup'

type Status = 'loading' | 'ready' | 'error'

const SORT_OPTIONS: { value: ArtworkSort; label: string }[] = [
  { value: 'Relevance', label: 'Relevance' },
  { value: 'DateAsc', label: 'Date (oldest first)' },
  { value: 'DateDesc', label: 'Date (newest first)' },
  { value: 'TitleAsc', label: 'Title (A–Z)' },
  { value: 'TitleDesc', label: 'Title (Z–A)' },
]

function toggle(values: string[], value: string): string[] {
  return values.includes(value) ? values.filter((v) => v !== value) : [...values, value]
}

export function Gallery() {
  const [query, setQuery] = useState('')
  const [eras, setEras] = useState<string[]>([])
  const [mediums, setMediums] = useState<string[]>([])
  const [movements, setMovements] = useState<string[]>([])
  const [sort, setSort] = useState<ArtworkSort>('Relevance')
  const [page, setPage] = useState(1)
  const [result, setResult] = useState<ArtworkSearchResult | null>(null)
  const [status, setStatus] = useState<Status>('loading')

  // Re-runs whenever the text, a filter, the sort order or the page changes. The
  // AbortController cancels a stale request if something changes again before it resolves,
  // so an earlier response can't overwrite a later one.
  useEffect(() => {
    const controller = new AbortController()

    searchArtworks(
      { q: query || undefined, era: eras, medium: mediums, movement: movements, sort, page },
      controller.signal,
    )
      .then((data) => {
        setResult(data)
        setStatus('ready')
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return
        setStatus('error')
      })

    return () => controller.abort()
  }, [query, eras, mediums, movements, sort, page])

  // Any change to what's being asked for (not the page itself) should jump back to page 1,
  // otherwise a narrower filter can leave the view stranded past the end of the new results.
  function updateFilters(apply: () => void) {
    apply()
    setPage(1)
    setStatus('loading')
  }

  const hasFilters = eras.length > 0 || mediums.length > 0 || movements.length > 0

  return (
    <div className="layout">
      <aside className="filters">
        <input
          type="search"
          placeholder="Search artworks…"
          value={query}
          onChange={(event) => {
            const value = event.target.value
            updateFilters(() => setQuery(value))
          }}
        />

        <label className="sort-select">
          Sort by
          <select
            value={sort}
            onChange={(event) => {
              const value = event.target.value as ArtworkSort
              updateFilters(() => setSort(value))
            }}
          >
            {SORT_OPTIONS.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </label>

        {hasFilters && (
          <button
            type="button"
            onClick={() =>
              updateFilters(() => {
                setEras([])
                setMediums([])
                setMovements([])
              })
            }
          >
            Clear filters
          </button>
        )}

        {result && (
          <>
            <FacetFilterGroup
              title="Era"
              buckets={result.facets.era ?? []}
              selected={eras}
              onToggle={(value) => updateFilters(() => setEras((prev) => toggle(prev, value)))}
            />
            <FacetFilterGroup
              title="Medium"
              buckets={result.facets.medium ?? []}
              selected={mediums}
              onToggle={(value) => updateFilters(() => setMediums((prev) => toggle(prev, value)))}
            />
            <FacetFilterGroup
              title="Movement"
              buckets={result.facets.movement ?? []}
              selected={movements}
              onToggle={(value) => updateFilters(() => setMovements((prev) => toggle(prev, value)))}
            />
          </>
        )}
      </aside>

      <div className="results">
        {status === 'loading' && <p>Loading…</p>}
        {status === 'error' && <p>Couldn't reach the API. Is it running?</p>}

        {status === 'ready' && result && (
          <>
            <p>{result.total} artworks</p>
            <ul className="gallery-grid">
              {result.items.map((artwork) => (
                <ArtworkCard key={artwork.id} artwork={artwork} />
              ))}
            </ul>

            {result.totalPages > 1 && (
              <div className="pagination">
                <button
                  type="button"
                  disabled={page <= 1}
                  onClick={() => {
                    setPage((p) => p - 1)
                    setStatus('loading')
                  }}
                >
                  Previous
                </button>
                <span>
                  Page {result.page} of {result.totalPages}
                </span>
                <button
                  type="button"
                  disabled={page >= result.totalPages}
                  onClick={() => {
                    setPage((p) => p + 1)
                    setStatus('loading')
                  }}
                >
                  Next
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  )
}
