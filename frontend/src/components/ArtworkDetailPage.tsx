import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router'
import { getArtwork, type ArtworkDetail } from '../lib/api'
import { SaveToCollectionMenu } from './SaveToCollectionMenu'

type Status = 'loading' | 'ready' | 'not-found' | 'error'

export function ArtworkDetailPage() {
  const { id } = useParams()

  // Remounting on id change (via `key`) lets `status` start fresh at 'loading' through
  // useState's initial value instead of an effect resetting it, which is what let the
  // gallery's search box avoid a synchronous setState inside an effect. A route param has
  // no single local event to hook that reset into, so a remount serves the same purpose.
  return <ArtworkDetailView key={id} id={Number(id)} />
}

function ArtworkDetailView({ id }: { id: number }) {
  const [artwork, setArtwork] = useState<ArtworkDetail | null>(null)
  const [status, setStatus] = useState<Status>('loading')

  useEffect(() => {
    const controller = new AbortController()

    getArtwork(id, controller.signal)
      .then((data) => {
        if (data === null) {
          setStatus('not-found')
          return
        }
        setArtwork(data)
        setStatus('ready')
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return
        setStatus('error')
      })

    return () => controller.abort()
  }, [id])

  if (status === 'loading') return <p>Loading…</p>
  if (status === 'error') return <p>Couldn't reach the API. Is it running?</p>
  if (status === 'not-found' || !artwork) {
    return (
      <div>
        <p>No artwork found with that id.</p>
        <Link to="/">Back to gallery</Link>
      </div>
    )
  }

  return (
    <article className="artwork-detail">
      <Link to="/">← Back to gallery</Link>

      <div className="artwork-detail-layout">
        {artwork.imageUrl ? (
          <img
            className="artwork-detail-image"
            src={`/api/artworks/${artwork.id}/image`}
            alt={artwork.title}
          />
        ) : (
          <div className="artwork-thumb-placeholder artwork-detail-image" aria-hidden="true" />
        )}

        <div className="artwork-detail-info">
          <h2>{artwork.title}</h2>
          <p className="artwork-meta">
            {artwork.artist?.name ?? 'Unknown artist'}
            {artwork.dateDisplay ? ` · ${artwork.dateDisplay}` : ''}
          </p>

          <SaveToCollectionMenu artworkId={artwork.id} />

          {artwork.movements.length > 0 && (
            <p className="artwork-tags">{artwork.movements.join(' · ')}</p>
          )}

          {artwork.description && <p>{artwork.description}</p>}

          <dl className="artwork-facts">
            {artwork.mediumDisplay && (
              <>
                <dt>Medium</dt>
                <dd>{artwork.mediumDisplay}</dd>
              </>
            )}
            {artwork.dimensions && (
              <>
                <dt>Dimensions</dt>
                <dd>{artwork.dimensions}</dd>
              </>
            )}
            {artwork.department && (
              <>
                <dt>Department</dt>
                <dd>{artwork.department}</dd>
              </>
            )}
            {artwork.creditLine && (
              <>
                <dt>Credit line</dt>
                <dd>{artwork.creditLine}</dd>
              </>
            )}
          </dl>

          {artwork.sourceUrl && (
            <p>
              <a href={artwork.sourceUrl} target="_blank" rel="noreferrer">
                View on the museum's site ↗
              </a>
            </p>
          )}
        </div>
      </div>
    </article>
  )
}
