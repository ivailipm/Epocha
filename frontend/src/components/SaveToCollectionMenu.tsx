import { useState, type FormEvent } from 'react'
import { useLocation, useNavigate } from 'react-router'
import { useAuth } from '../context/useAuth'
import { addArtworkToCollection, createCollection, listCollections, type CollectionSummary } from '../lib/collections'

type Status = 'idle' | 'loading' | 'error'

// Adding an artwork to a collection happens here; removing it happens on the collection's own
// page instead, where its full contents are already visible. That split keeps this menu to a
// single action instead of needing to know which collections already contain the artwork.
export function SaveToCollectionMenu({ artworkId }: { artworkId: number }) {
  const { session } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [open, setOpen] = useState(false)
  const [collections, setCollections] = useState<CollectionSummary[] | null>(null)
  const [status, setStatus] = useState<Status>('idle')
  const [addedIds, setAddedIds] = useState<Set<number>>(new Set())
  const [newName, setNewName] = useState('')

  function toggleOpen(event: React.MouseEvent) {
    event.preventDefault()

    if (!session) {
      navigate('/login', { state: { from: location.pathname } })
      return
    }

    const willOpen = !open
    setOpen(willOpen)

    if (willOpen && !collections) {
      setStatus('loading')
      listCollections(session.token)
        .then((data) => {
          setCollections(data)
          setStatus('idle')
        })
        .catch(() => setStatus('error'))
    }
  }

  async function handleAdd(collectionId: number) {
    if (!session) return
    await addArtworkToCollection(session.token, collectionId, artworkId)
    setAddedIds((prev) => new Set(prev).add(collectionId))
  }

  async function handleCreateAndAdd(event: FormEvent) {
    event.preventDefault()
    const name = newName.trim()
    if (!session || name.length === 0) return

    const created = await createCollection(session.token, name)
    setCollections((prev) => [...(prev ?? []), created])
    setNewName('')
    await handleAdd(created.id)
  }

  return (
    <div className="save-menu">
      <button type="button" onClick={toggleOpen}>
        {open ? 'Close' : 'Save to collection'}
      </button>

      {open && (
        <div className="save-menu-popover">
          {status === 'loading' && <p>Loading…</p>}
          {status === 'error' && <p>Couldn't load your collections.</p>}

          {collections && collections.length > 0 && (
            <ul>
              {collections.map((collection) => (
                <li key={collection.id}>
                  <button
                    type="button"
                    onClick={() => handleAdd(collection.id)}
                    disabled={addedIds.has(collection.id)}
                  >
                    {addedIds.has(collection.id) ? '✓ ' : ''}
                    {collection.name}
                  </button>
                </li>
              ))}
            </ul>
          )}

          <form onSubmit={handleCreateAndAdd} className="save-menu-new">
            <input
              placeholder="New collection…"
              value={newName}
              onChange={(event) => setNewName(event.target.value)}
            />
            <button type="submit">Add</button>
          </form>
        </div>
      )}
    </div>
  )
}
