import { useEffect, useId, useRef, useState } from 'react'
import type { KeyboardEvent } from 'react'

export interface AppSelectOption<T extends string = string> {
  label: string
  value: T
}

interface AppSelectProps<T extends string = string> {
  disabled?: boolean
  error?: string
  label?: string
  onChange: (value: T) => void
  options: AppSelectOption<T>[]
  placeholder?: string
  value: T
}

function AppSelect<T extends string = string>({
  disabled = false,
  error,
  label,
  onChange,
  options,
  placeholder = 'Odaberite',
  value,
}: AppSelectProps<T>) {
  const [isOpen, setIsOpen] = useState(false)
  const rootRef = useRef<HTMLDivElement>(null)
  const listId = useId()
  const selectedOption = options.find((option) => option.value === value)

  useEffect(() => {
    const closeOnOutsideClick = (event: MouseEvent) => {
      if (!rootRef.current?.contains(event.target as Node)) {
        setIsOpen(false)
      }
    }

    document.addEventListener('mousedown', closeOnOutsideClick)
    return () => document.removeEventListener('mousedown', closeOnOutsideClick)
  }, [])

  const handleKeyDown = (event: KeyboardEvent<HTMLButtonElement>) => {
    if (disabled) return

    if (event.key === 'Escape') {
      setIsOpen(false)
      return
    }

    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault()
      setIsOpen((current) => !current)
      return
    }

    const currentIndex = Math.max(
      0,
      options.findIndex((option) => option.value === value),
    )

    if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault()
      const offset = event.key === 'ArrowDown' ? 1 : -1
      const nextIndex = (currentIndex + offset + options.length) % options.length
      onChange(options[nextIndex].value)
      setIsOpen(true)
    }
  }

  return (
    <div ref={rootRef} className="relative grid gap-2 text-sm font-semibold text-stone-300">
      {label && <span>{label}</span>}
      <button
        type="button"
        aria-controls={listId}
        aria-expanded={isOpen}
        disabled={disabled}
        onClick={() => setIsOpen((current) => !current)}
        onKeyDown={handleKeyDown}
        className={`flex w-full items-center justify-between gap-3 rounded-2xl border bg-black/25 px-4 py-3 text-left text-stone-100 outline-none transition disabled:cursor-not-allowed disabled:opacity-50 ${
          error
            ? 'border-red-300/35'
            : isOpen
              ? 'border-amber-200/40'
              : 'border-amber-200/10 hover:border-amber-200/30'
        }`}
      >
        <span>{selectedOption?.label ?? placeholder}</span>
        <span className={`text-amber-100 transition ${isOpen ? 'rotate-180' : ''}`}>
          ˅
        </span>
      </button>

      {isOpen && !disabled && (
        <div
          id={listId}
          role="listbox"
          className="absolute left-0 right-0 top-full z-30 mt-2 overflow-hidden rounded-2xl border border-amber-200/15 bg-[#07110d] p-1 shadow-[0_18px_45px_rgba(0,0,0,0.42)]"
        >
          {options.map((option) => {
            const isSelected = option.value === value

            return (
              <button
                key={option.value}
                type="button"
                role="option"
                aria-selected={isSelected}
                onClick={() => {
                  onChange(option.value)
                  setIsOpen(false)
                }}
                className={`w-full rounded-xl px-3 py-2.5 text-left text-sm transition ${
                  isSelected
                    ? 'bg-amber-100/12 text-amber-100'
                    : 'text-stone-300 hover:bg-amber-100/8 hover:text-amber-100'
                }`}
              >
                {option.label}
              </button>
            )
          })}
        </div>
      )}

      {error && <span className="text-xs text-red-200">{error}</span>}
    </div>
  )
}

export default AppSelect
