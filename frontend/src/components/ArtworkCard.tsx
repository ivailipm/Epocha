import type { ReactNode } from 'react'
import { Link } from 'react-router'
import type { ArtworkSummary } from '../lib/api'
import { SaveToCollectionMenu } from './SaveToCollectionMenu'

interface Props {
  artwork: ArtworkSummary
  // Extra action rendered next to "Save to collection", e.g. "Remove" on a collection's own page.
  extraAction?: ReactNode
}

export function ArtworkCard({ artwork, extraAction }: Props) {
  return (
    <li className="artwork-card">
      <Link to={`/artworks/${artwork.id}`}>
        <div className="artwork-thumb">
          {artwork.thumbnailUrl ? (
            // The museum's own image URL can't be loaded from another site (Referer-based
            // hotlink protection), so this goes through our API, which fetches it instead.
            <img src={`/api/artworks/${artwork.id}/thumbnail`} alt={artwork.title} loading="lazy" />
          ) : (
            <div className="artwork-thumb-placeholder" aria-hidden="true" />
          )}
        </div>
        <h3>{artwork.title}</h3>
        <p className="artwork-meta">
          {artwork.artistName ?? 'Unknown artist'}
          {artwork.dateDisplay ? ` · ${artwork.dateDisplay}` : ''}
        </p>
      </Link>
      <div className="artwork-card-actions">
        <SaveToCollectionMenu artworkId={artwork.id} />
        {extraAction}
      </div>
    </li>
  )
}
