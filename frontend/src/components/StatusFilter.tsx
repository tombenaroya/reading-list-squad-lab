import type { BookStatus } from '../api/books'

export type StatusFilterValue = 'all' | BookStatus

type StatusFilterProps = {
  value: StatusFilterValue
  onChange: (value: StatusFilterValue) => void
}

const options: { value: StatusFilterValue; label: string }[] = [
  { value: 'all', label: 'All' },
  { value: 'unread', label: 'Unread' },
  { value: 'read', label: 'Read' },
]

export function StatusFilter({ value, onChange }: StatusFilterProps) {
  return (
    <div className="status-filter" role="group" aria-label="Filter books">
      {options.map((option) => (
        <button
          key={option.value}
          type="button"
          className={option.value === value ? 'active' : undefined}
          aria-pressed={option.value === value}
          onClick={() => onChange(option.value)}
        >
          {option.label}
        </button>
      ))}
    </div>
  )
}
