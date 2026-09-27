import { useState } from 'react'
import type { FacetBucket } from '../lib/api'

interface Props {
  title: string
  buckets: FacetBucket[]
  selected: string[]
  onToggle: (value: string) => void
}

const MAX_VISIBLE = 50

// Unlike FacetFilterGroup, the bucket list here is too long to show in full (hundreds of
// artists), so a local text box narrows it client-side — no network request involved, since
// the full bucket list is already part of the search response.
export function SearchableFacetFilterGroup({ title, buckets, selected, onToggle }: Props) {
  const [search, setSearch] = useState('')

  if (buckets.length === 0) {
    return null
  }

  const term = search.trim().toLowerCase()
  const matches = term ? buckets.filter((b) => b.value.toLowerCase().includes(term)) : buckets
  const visible = matches.slice(0, MAX_VISIBLE)

  return (
    <fieldset className="facet-group">
      <legend>{title}</legend>
      <input
        type="text"
        className="facet-search"
        placeholder={`Search ${title.toLowerCase()}…`}
        value={search}
        onChange={(event) => setSearch(event.target.value)}
      />

      {visible.map((bucket) => (
        <label key={bucket.value} className="facet-option">
          <input
            type="checkbox"
            checked={selected.includes(bucket.value)}
            onChange={() => onToggle(bucket.value)}
          />
          {bucket.value} <span className="facet-count">{bucket.count}</span>
        </label>
      ))}

      {matches.length === 0 && <p className="facet-more">No matches</p>}
      {matches.length > MAX_VISIBLE && (
        <p className="facet-more">{matches.length - MAX_VISIBLE} more — keep typing to narrow</p>
      )}
    </fieldset>
  )
}
