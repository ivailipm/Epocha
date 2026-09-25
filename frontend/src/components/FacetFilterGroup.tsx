import type { FacetBucket } from '../lib/api'

interface Props {
  title: string
  buckets: FacetBucket[]
  selected: string[]
  onToggle: (value: string) => void
}

export function FacetFilterGroup({ title, buckets, selected, onToggle }: Props) {
  if (buckets.length === 0) {
    return null
  }

  return (
    <fieldset className="facet-group">
      <legend>{title}</legend>
      {buckets.map((bucket) => (
        <label key={bucket.value} className="facet-option">
          <input
            type="checkbox"
            checked={selected.includes(bucket.value)}
            onChange={() => onToggle(bucket.value)}
          />
          {bucket.value} <span className="facet-count">{bucket.count}</span>
        </label>
      ))}
    </fieldset>
  )
}
