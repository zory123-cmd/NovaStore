import { Link } from 'react-router-dom'
import type { Product } from '../types'

interface ProductCardProps {
  product: Product
}

export default function ProductCard({ product }: ProductCardProps) {
  return (
    <Link
      to={`/products/${product.id}`}
      className="group bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden hover:shadow-md transition-all duration-200 flex flex-col"
    >
      <div className="h-48 bg-gradient-to-br from-primary-50 to-primary-100 flex items-center justify-center overflow-hidden">
        {product.imageUrl ? (
          <img
            src={product.imageUrl}
            alt={product.name}
            className="h-full w-full object-cover group-hover:scale-105 transition-transform duration-300"
          />
        ) : (
          <div className="text-4xl text-primary-300 font-light">
            {product.name.charAt(0).toUpperCase()}
          </div>
        )}
      </div>
      <div className="p-4 flex-1 flex flex-col">
        {product.brand && (
          <p className="text-xs text-gray-400 uppercase tracking-wide mb-1">
            {product.brand}
          </p>
        )}
        <h3 className="font-medium text-gray-900 text-sm leading-snug mb-2 line-clamp-2 flex-1">
          {product.name}
        </h3>
        <div className="flex items-center justify-between mt-auto">
          <span className="text-lg font-bold text-primary-600">
            {product.price.toLocaleString('ru-RU')} ₽
          </span>
          <div className="flex items-center gap-1">
            <span className="text-yellow-400 text-sm">★</span>
            <span className="text-xs text-gray-500">
              {product.averageRating?.toFixed(1) ?? '—'}
            </span>
          </div>
        </div>
      </div>
    </Link>
  )
}
