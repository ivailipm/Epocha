import { Link } from 'react-router'
import type { ArtworkSummary } from '../lib/api'

export function ArtworkCard({ artwork }: { artwork: ArtworkSummary }) {
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
    </li>
  )
}
