import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { productService } from '../services/productService'
import type { Product, ProductSearchParams } from '../types'
import ProductCard from '../components/ProductCard'
import Sidebar from '../components/Sidebar'
import { ProductCardSkeleton } from '../components/Skeleton'

const PAGE_SIZE = 20

export default function Catalog() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [products, setProducts] = useState<Product[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [sortBy, setSortBy] = useState('')
  const [sortDesc, setSortDesc] = useState(false)

  const searchTerm = searchParams.get('search') ?? undefined
  const categoryId = searchParams.get('categoryId')
    ? Number(searchParams.get('categoryId'))
    : undefined
  const minPrice = searchParams.get('minPrice') ?? undefined
  const maxPrice = searchParams.get('maxPrice') ?? undefined
  const brand = searchParams.get('brand') ?? undefined

  const fetchProducts = async () => {
    setLoading(true)
    try {
      const params: ProductSearchParams = {
        searchTerm,
        categoryId,
        minPrice: minPrice ? Number(minPrice) : undefined,
        maxPrice: maxPrice ? Number(maxPrice) : undefined,
        brand,
        sortBy: sortBy || undefined,
        sortDescending: sortDesc,
        page,
        pageSize: PAGE_SIZE,
      }
      const result = await productService.search(params)
      setProducts(result.items)
      setTotalCount(result.totalCount)
    } catch {
      setProducts([])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchProducts()
  }, [searchTerm, categoryId, minPrice, maxPrice, brand, sortBy, sortDesc, page])

  const updateParam = (key: string, value: string | undefined) => {
    const params = new URLSearchParams(searchParams)
    if (value) {
      params.set(key, value)
    } else {
      params.delete(key)
    }
    setSearchParams(params)
    setPage(1)
  }

  const totalPages = Math.ceil(totalCount / PAGE_SIZE)

  const brands = [...new Set(products.map((p) => p.brand).filter(Boolean))] as string[]

  return (
    <div className="max-w-7xl mx-auto px-4 py-6">
      <div className="flex flex-col lg:flex-row gap-6">
        <Sidebar
          selectedCategoryId={categoryId}
          onSelectCategory={(id) => updateParam('categoryId', id?.toString())}
          minPrice={minPrice}
          maxPrice={maxPrice}
          onMinPriceChange={(v) => updateParam('minPrice', v || undefined)}
          onMaxPriceChange={(v) => updateParam('maxPrice', v || undefined)}
          selectedBrand={brand}
          onBrandChange={(v) => updateParam('brand', v || undefined)}
          brands={brands.length > 0 ? brands : undefined}
        />

        <div className="flex-1 min-w-0">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between mb-6 gap-3">
            <div>
              <h1 className="text-xl font-semibold text-gray-900">
                {searchTerm ? `Результаты поиска: «${searchTerm}»` : 'Каталог товаров'}
              </h1>
              <p className="text-sm text-gray-500 mt-1">
                Найдено {totalCount} товаров
              </p>
            </div>
            <select
              value={sortDesc ? `-${sortBy}` : sortBy}
              onChange={(e) => {
                const val = e.target.value
                if (val.startsWith('-')) {
                  setSortBy(val.slice(1))
                  setSortDesc(true)
                } else {
                  setSortBy(val)
                  setSortDesc(false)
                }
              }}
              className="px-3 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none"
            >
              <option value="">По умолчанию</option>
              <option value="price">Цена (по возрастанию)</option>
              <option value="-price">Цена (по убыванию)</option>
              <option value="name">Название (А-Я)</option>
              <option value="-name">Название (Я-А)</option>
              <option value="-averageRating">По рейтингу</option>
            </select>
          </div>

          {loading ? (
            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
              {Array.from({ length: 8 }).map((_, i) => (
                <ProductCardSkeleton key={i} />
              ))}
            </div>
          ) : products.length === 0 ? (
            <div className="text-center py-20">
              <div className="text-5xl mb-4">🔍</div>
              <h2 className="text-lg font-medium text-gray-900 mb-2">Товары не найдены</h2>
              <p className="text-gray-500 text-sm">Попробуйте изменить параметры поиска</p>
            </div>
          ) : (
            <>
              <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {products.map((product) => (
                  <ProductCard key={product.id} product={product} />
                ))}
              </div>

              {totalPages > 1 && (
                <div className="flex justify-center items-center gap-2 mt-8">
                  <button
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    disabled={page === 1}
                    className="px-3 py-2 rounded-lg border border-gray-300 text-sm hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    ←
                  </button>
                  {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                    <button
                      key={p}
                      onClick={() => setPage(p)}
                      className={`px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                        p === page
                          ? 'bg-primary-500 text-white'
                          : 'border border-gray-300 hover:bg-gray-50'
                      }`}
                    >
                      {p}
                    </button>
                  ))}
                  <button
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    disabled={page === totalPages}
                    className="px-3 py-2 rounded-lg border border-gray-300 text-sm hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    →
                  </button>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  )
}
