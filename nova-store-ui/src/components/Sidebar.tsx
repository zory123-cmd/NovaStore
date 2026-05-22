import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { categoryService } from '../services/categoryService'
import type { Category } from '../types'

interface SidebarProps {
  selectedCategoryId?: number
  onSelectCategory?: (id?: number) => void
  minPrice?: string
  maxPrice?: string
  onMinPriceChange?: (v: string) => void
  onMaxPriceChange?: (v: string) => void
  selectedBrand?: string
  onBrandChange?: (v: string) => void
  brands?: string[]
}

export default function Sidebar({
  selectedCategoryId,
  onSelectCategory,
  minPrice,
  maxPrice,
  onMinPriceChange,
  onMaxPriceChange,
  selectedBrand,
  onBrandChange,
  brands,
}: SidebarProps) {
  const [categories, setCategories] = useState<Category[]>([])
  const [expanded, setExpanded] = useState<Record<number, boolean>>({})

  useEffect(() => {
    categoryService.getAll().then(setCategories).catch(() => {})
  }, [])

  return (
    <aside className="w-full lg:w-64 flex-shrink-0">
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 space-y-6">
        <div>
          <h3 className="font-semibold text-gray-900 mb-3 text-sm uppercase tracking-wide">Категории</h3>
          <ul className="space-y-1">
            <li>
              <button
                onClick={() => onSelectCategory?.(undefined)}
                className={`w-full text-left px-3 py-2 rounded-lg text-sm transition-colors ${
                  !selectedCategoryId
                    ? 'bg-primary-50 text-primary-700 font-medium'
                    : 'text-gray-600 hover:bg-gray-50'
                }`}
              >
                Все товары
              </button>
            </li>
            {categories.map((cat) => (
              <li key={cat.id}>
                <div className="flex items-center">
                  <button
                    onClick={() => onSelectCategory?.(cat.id)}
                    className={`flex-1 text-left px-3 py-2 rounded-lg text-sm transition-colors ${
                      selectedCategoryId === cat.id
                        ? 'bg-primary-50 text-primary-700 font-medium'
                        : 'text-gray-600 hover:bg-gray-50'
                    }`}
                  >
                    {cat.name}
                  </button>
                  {cat.subCategories.length > 0 && (
                    <button
                      onClick={() => setExpanded((p) => ({ ...p, [cat.id]: !p[cat.id] }))}
                      className="p-2 text-gray-400 hover:text-gray-600"
                    >
                      <svg className={`w-4 h-4 transition-transform ${expanded[cat.id] ? 'rotate-90' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                      </svg>
                    </button>
                  )}
                </div>
                {expanded[cat.id] && cat.subCategories.length > 0 && (
                  <ul className="ml-4 mt-1 space-y-1">
                    {cat.subCategories.map((sub) => (
                      <li key={sub.id}>
                        <button
                          onClick={() => onSelectCategory?.(sub.id)}
                          className={`w-full text-left px-3 py-1.5 rounded-lg text-sm transition-colors ${
                            selectedCategoryId === sub.id
                              ? 'bg-primary-50 text-primary-700 font-medium'
                              : 'text-gray-500 hover:bg-gray-50'
                          }`}
                        >
                          {sub.name}
                        </button>
                      </li>
                    ))}
                  </ul>
                )}
              </li>
            ))}
          </ul>
        </div>

        <div>
          <h3 className="font-semibold text-gray-900 mb-3 text-sm uppercase tracking-wide">Цена</h3>
          <div className="flex gap-2">
            <input
              type="number"
              placeholder="От"
              value={minPrice ?? ''}
              onChange={(e) => onMinPriceChange?.(e.target.value)}
              className="w-full px-3 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
            />
            <input
              type="number"
              placeholder="До"
              value={maxPrice ?? ''}
              onChange={(e) => onMaxPriceChange?.(e.target.value)}
              className="w-full px-3 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
            />
          </div>
        </div>

        {brands && brands.length > 0 && (
          <div>
            <h3 className="font-semibold text-gray-900 mb-3 text-sm uppercase tracking-wide">Бренд</h3>
            <select
              value={selectedBrand ?? ''}
              onChange={(e) => onBrandChange?.(e.target.value)}
              className="w-full px-3 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
            >
              <option value="">Все бренды</option>
              {brands.map((b) => (
                <option key={b} value={b}>{b}</option>
              ))}
            </select>
          </div>
        )}
      </div>
    </aside>
  )
}
