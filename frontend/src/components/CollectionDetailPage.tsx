import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router'
import { useAuth } from '../context/useAuth'
import {
  deleteCollection,
  getCollection,
  removeArtworkFromCollection,
  renameCollection,
  type CollectionDetail,
} from '../lib/collections'
import { ApiError } from '../lib/http'
import { ArtworkCard } from './ArtworkCard'

type Status = 'loading' | 'ready' | 'not-found' | 'error'

export function CollectionDetailPage() {
  const { id } = useParams()

  // Same reasoning as ArtworkDetailPage: the id changes via the route, not a local event, so a
  // remount (via `key`) resets state through useState's initial value instead of an effect.
  return <CollectionDetailView key={id} id={Number(id)} />
}

function CollectionDetailView({ id }: { id: number }) {
  const { session } = useAuth()
  const navigate = useNavigate()

  const [collection, setCollection] = useState<CollectionDetail | null>(null)
  const [status, setStatus] = useState<Status>('loading')
  const [isRenaming, setIsRenaming] = useState(false)
  const [name, setName] = useState('')
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!session) return

    getCollection(session.token, id)
      .then((data) => {
        setCollection(data)
        setName(data.name)
        setStatus('ready')
      })
      .catch((err: unknown) => setStatus(err instanceof ApiError && err.status === 404 ? 'not-found' : 'error'))
  }, [session, id])

  if (!session) {
    return null
  }

  async function handleRemoveArtwork(artworkId: number) {
    if (!session || !collection) return
    await removeArtworkFromCollection(session.token, collection.id, artworkId)
    setCollection({ ...collection, artworks: collection.artworks.filter((a) => a.id !== artworkId) })
  }

  async function handleRename(event: FormEvent) {
    event.preventDefault()
    const trimmed = name.trim()
    if (!session || !collection || trimmed.length === 0) return

    setError(null)
    try {
      const updated = await renameCollection(session.token, collection.id, trimmed)
      setCollection({ ...collection, name: updated.name })
      setIsRenaming(false)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong.')
    }
  }

  async function handleDelete() {
    if (!session || !collection) return
    if (!window.confirm(`Delete "${collection.name}"? This can't be undone.`)) return

    await deleteCollection(session.token, collection.id)
    navigate('/collections')
  }

  if (status === 'loading') return <p>Loading…</p>
  if (status === 'error') return <p>Couldn't reach the API. Is it running?</p>
  if (status === 'not-found' || !collection) {
    return (
      <div>
        <p>No collection found with that id.</p>
        <Link to="/collections">Back to my collections</Link>
      </div>
    )
  }

  return (
    <div>
      <Link to="/collections">← Back to my collections</Link>

      <div className="collection-header">
        {isRenaming ? (
          <form onSubmit={handleRename} className="new-collection-form">
            <input type="text" value={name} onChange={(event) => setName(event.target.value)} />
            <button type="submit">Save</button>
            <button type="button" onClick={() => setIsRenaming(false)}>
              Cancel
            </button>
          </form>
        ) : (
          <>
            <h2>{collection.name}</h2>
            <button type="button" onClick={() => setIsRenaming(true)}>
              Rename
            </button>
            <button type="button" onClick={handleDelete}>
              Delete
            </button>
          </>
        )}
      </div>
      {error && <p className="auth-error">{error}</p>}

      {collection.artworks.length === 0 ? (
        <p>No artworks here yet — use "Save to collection" on any artwork to add one.</p>
      ) : (
        <ul className="gallery-grid">
          {collection.artworks.map((artwork) => (
            <ArtworkCard
              key={artwork.id}
              artwork={artwork}
              extraAction={
                <button type="button" onClick={() => handleRemoveArtwork(artwork.id)}>
                  Remove
                </button>
              }
            />
          ))}
        </ul>
      )}
    </div>
  )
}
