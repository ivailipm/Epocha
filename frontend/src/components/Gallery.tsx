import { useEffect, useState } from 'react'
import { searchArtworks, type ArtworkSearchResult } from '../lib/api'
import { ArtworkCard } from './ArtworkCard'
import { FacetFilterGroup } from './FacetFilterGroup'

type Status = 'loading' | 'ready' | 'error'

function toggle(values: string[], value: string): string[] {
  return values.includes(value) ? values.filter((v) => v !== value) : [...values, value]
}

export function Gallery() {
  const [query, setQuery] = useState('')
  const [eras, setEras] = useState<string[]>([])
  const [mediums, setMediums] = useState<string[]>([])
  const [movements, setMovements] = useState<string[]>([])
  const [result, setResult] = useState<ArtworkSearchResult | null>(null)
  const [status, setStatus] = useState<Status>('loading')

  // Re-runs whenever the text or a filter changes. The AbortController cancels a stale
  // request if the user changes something again before it resolves, so an earlier response
  // can't overwrite a later one.
  useEffect(() => {
    const controller = new AbortController()

    searchArtworks(
      { q: query || undefined, era: eras, medium: mediums, movement: movements },
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
  }, [query, eras, mediums, movements])

  const hasFilters = eras.length > 0 || mediums.length > 0 || movements.length > 0

  return (
    <div className="layout">
      <aside className="filters">
        <input
          type="search"
          placeholder="Search artworks…"
          value={query}
          onChange={(event) => {
            setQuery(event.target.value)
            setStatus('loading')
          }}
        />

        {hasFilters && (
          <button
            type="button"
            onClick={() => {
              setEras([])
              setMediums([])
              setMovements([])
              setStatus('loading')
            }}
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
              onToggle={(value) => {
                setEras((prev) => toggle(prev, value))
                setStatus('loading')
              }}
            />
            <FacetFilterGroup
              title="Medium"
              buckets={result.facets.medium ?? []}
              selected={mediums}
              onToggle={(value) => {
                setMediums((prev) => toggle(prev, value))
                setStatus('loading')
              }}
            />
            <FacetFilterGroup
              title="Movement"
              buckets={result.facets.movement ?? []}
              selected={movements}
              onToggle={(value) => {
                setMovements((prev) => toggle(prev, value))
                setStatus('loading')
              }}
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
          </>
        )}
      </div>
    </div>
  )
}
