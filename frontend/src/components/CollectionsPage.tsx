import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router'
import { useAuth } from '../context/useAuth'
import { createCollection, listCollections, type CollectionSummary } from '../lib/collections'

type Status = 'loading' | 'ready' | 'error'

export function CollectionsPage() {
  const { session } = useAuth()
  const [collections, setCollections] = useState<CollectionSummary[]>([])
  const [status, setStatus] = useState<Status>('loading')
  const [newName, setNewName] = useState('')
  const [error, setError] = useState<string | null>(null)

  // RequireAuth guarantees a session by the time this renders, so this effect only ever runs
  // its fetch once per mount, not on repeated session changes.
  useEffect(() => {
    if (!session) return

    listCollections(session.token)
      .then((data) => {
        setCollections(data)
        setStatus('ready')
      })
      .catch(() => setStatus('error'))
  }, [session])

  if (!session) {
    return null
  }

  async function handleCreate(event: FormEvent) {
    event.preventDefault()
    const name = newName.trim()
    if (!session || name.length === 0) return

    setError(null)
    try {
      const created = await createCollection(session.token, name)
      setCollections((prev) => [...prev, created].sort((a, b) => a.name.localeCompare(b.name)))
      setNewName('')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Something went wrong.')
    }
  }

  return (
    <div>
      <h2>My Collections</h2>

      <form onSubmit={handleCreate} className="new-collection-form">
        <input
          placeholder="New collection name…"
          value={newName}
          onChange={(event) => setNewName(event.target.value)}
        />
        <button type="submit">Create</button>
      </form>
      {error && <p className="auth-error">{error}</p>}

      {status === 'loading' && <p>Loading…</p>}
      {status === 'error' && <p>Couldn't load your collections.</p>}
      {status === 'ready' && collections.length === 0 && (
        <p>No collections yet — create one above, or save an artwork to get started.</p>
      )}
      {status === 'ready' && collections.length > 0 && (
        <ul className="collections-list">
          {collections.map((collection) => (
            <li key={collection.id}>
              <Link to={`/collections/${collection.id}`}>{collection.name}</Link>
              <span className="facet-count">{collection.artworkCount}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
