import { useEffect, useState } from 'react'
import { searchArtworks, type ArtworkSearchResult } from '../lib/api'
import { ArtworkCard } from './ArtworkCard'

type Status = 'loading' | 'ready' | 'error'

export function Gallery() {
  const [query, setQuery] = useState('')
  const [result, setResult] = useState<ArtworkSearchResult | null>(null)
  const [status, setStatus] = useState<Status>('loading')

  // Re-runs whenever `query` changes. The AbortController cancels a stale request if the
  // user types again before it resolves, so an earlier response can't overwrite a later one.
  useEffect(() => {
    const controller = new AbortController()

    searchArtworks({ q: query || undefined }, controller.signal)
      .then((data) => {
        setResult(data)
        setStatus('ready')
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return
        setStatus('error')
      })

    return () => controller.abort()
  }, [query])

  return (
    <div>
      <input
        type="search"
        placeholder="Search artworks…"
        value={query}
        onChange={(event) => {
          setQuery(event.target.value)
          setStatus('loading')
        }}
      />

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
  )
}
