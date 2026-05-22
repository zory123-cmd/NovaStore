import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { productService } from '../services/productService'
import { reviewService } from '../services/reviewService'
import { cartService } from '../services/cartService'
import { useAuth } from '../contexts/AuthContext'
import type { Product, Review } from '../types'
import { ProductDetailSkeleton } from '../components/Skeleton'
import toast from 'react-hot-toast'

export default function ProductDetail() {
  const { id } = useParams<{ id: string }>()
  const { isAuthenticated } = useAuth()
  const [product, setProduct] = useState<Product | null>(null)
  const [reviews, setReviews] = useState<Review[]>([])
  const [loading, setLoading] = useState(true)
  const [quantity, setQuantity] = useState(1)
  const [reviewForm, setReviewForm] = useState({ rating: 5, comment: '' })
  const [addingReview, setAddingReview] = useState(false)

  useEffect(() => {
    if (!id) return
    setLoading(true)
    Promise.all([
      productService.getById(Number(id)),
      reviewService.getByProduct(Number(id)),
    ])
      .then(([p, r]) => {
        setProduct(p)
        setReviews(r.items)
      })
      .catch(() => toast.error('Ошибка загрузки товара'))
      .finally(() => setLoading(false))
  }, [id])

  const handleAddToCart = async () => {
    if (!product) return
    try {
      await cartService.addToCart({ productId: product.id, quantity })
      toast.success('Товар добавлен в корзину!')
    } catch {
      toast.error('Ошибка при добавлении в корзину')
    }
  }

  const handleAddReview = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!product) return
    setAddingReview(true)
    try {
      const review = await reviewService.create({
        productId: product.id,
        rating: reviewForm.rating,
        comment: reviewForm.comment,
      })
      setReviews((prev) => [review, ...prev])
      setReviewForm({ rating: 5, comment: '' })
      toast.success('Отзыв добавлен!')
    } catch {
      toast.error('Ошибка при добавлении отзыва')
    } finally {
      setAddingReview(false)
    }
  }

  if (loading) return <ProductDetailSkeleton />
  if (!product) return <div className="text-center py-20">Товар не найден</div>

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      <div className="grid md:grid-cols-2 gap-8 mb-12">
        <div className="bg-gradient-to-br from-primary-50 to-primary-100 rounded-2xl flex items-center justify-center h-96">
          {product.imageUrl ? (
            <img src={product.imageUrl} alt={product.name} className="h-full w-full object-cover rounded-2xl" />
          ) : (
            <span className="text-6xl text-primary-300 font-light">
              {product.name.charAt(0).toUpperCase()}
            </span>
          )}
        </div>

        <div>
          <div className="flex items-center gap-2 text-sm text-gray-500 mb-2">
            {product.categoryName && (
              <>
                <Link to={`/?categoryId=${product.categoryId}`} className="hover:text-primary-600">
                  {product.categoryName}
                </Link>
                <span>/</span>
              </>
            )}
            {product.brand && <span className="text-gray-400">{product.brand}</span>}
          </div>

          <h1 className="text-2xl font-bold text-gray-900 mb-4">{product.name}</h1>

          <div className="flex items-center gap-3 mb-4">
            <div className="flex items-center gap-1">
              {Array.from({ length: 5 }).map((_, i) => (
                <span key={i} className={`text-lg ${i < Math.round(product.averageRating ?? 0) ? 'text-yellow-400' : 'text-gray-200'}`}>
                  ★
                </span>
              ))}
            </div>
            <span className="text-sm text-gray-500">
              {product.averageRating?.toFixed(1) ?? 'Нет оценок'} · {product.reviewsCount} отзывов
            </span>
          </div>

          <div className="text-3xl font-bold text-primary-600 mb-6">
            {product.price.toLocaleString('ru-RU')} ₽
          </div>

          <div className="mb-6">
            <h3 className="text-sm font-medium text-gray-700 mb-2">Описание</h3>
            <p className="text-gray-600 text-sm leading-relaxed">
              {product.description ?? 'Описание отсутствует'}
            </p>
          </div>

          <div className="flex items-center gap-4 mb-6">
            <div className="flex items-center border border-gray-300 rounded-lg">
              <button
                onClick={() => setQuantity((q) => Math.max(1, q - 1))}
                className="px-3 py-2 text-gray-600 hover:bg-gray-50 transition-colors"
              >
                −
              </button>
              <span className="px-4 py-2 font-medium text-sm min-w-[3rem] text-center">{quantity}</span>
              <button
                onClick={() => setQuantity((q) => Math.min(product.stockQuantity, q + 1))}
                className="px-3 py-2 text-gray-600 hover:bg-gray-50 transition-colors"
              >
                +
              </button>
            </div>

            {isAuthenticated ? (
              <button
                onClick={handleAddToCart}
                disabled={product.stockQuantity === 0}
                className="flex-1 py-2.5 px-6 bg-gradient-to-r from-primary-500 to-blue-500 text-white rounded-lg font-medium text-sm hover:from-primary-600 hover:to-blue-600 disabled:opacity-50 transition-all shadow-md shadow-primary-200"
              >
                {product.stockQuantity === 0 ? 'Нет в наличии' : 'В корзину'}
              </button>
            ) : (
              <Link
                to="/login"
                className="flex-1 py-2.5 px-6 bg-gradient-to-r from-primary-500 to-blue-500 text-white rounded-lg font-medium text-sm text-center hover:from-primary-600 hover:to-blue-600 transition-all shadow-md shadow-primary-200"
              >
                Войдите, чтобы купить
              </Link>
            )}
          </div>

          {product.stockQuantity > 0 && (
            <p className="text-sm text-green-600">В наличии: {product.stockQuantity} шт.</p>
          )}
        </div>
      </div>

      <div className="border-t border-gray-200 pt-8">
        <h2 className="text-xl font-bold text-gray-900 mb-6">Отзывы</h2>

        {isAuthenticated && (
          <form onSubmit={handleAddReview} className="bg-white rounded-xl border border-gray-100 p-5 mb-6 shadow-sm">
            <h3 className="font-medium text-gray-900 mb-3">Оставить отзыв</h3>
            <div className="flex items-center gap-1 mb-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <button
                  key={i}
                  type="button"
                  onClick={() => setReviewForm((f) => ({ ...f, rating: i + 1 }))}
                  className={`text-2xl transition-colors ${i < reviewForm.rating ? 'text-yellow-400' : 'text-gray-200'}`}
                >
                  ★
                </button>
              ))}
            </div>
            <textarea
              placeholder="Ваш комментарий..."
              value={reviewForm.comment}
              onChange={(e) => setReviewForm((f) => ({ ...f, comment: e.target.value }))}
              rows={3}
              className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none text-sm mb-3"
            />
            <button
              type="submit"
              disabled={addingReview}
              className="px-6 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 disabled:opacity-50 transition-colors"
            >
              {addingReview ? 'Отправка...' : 'Отправить'}
            </button>
          </form>
        )}

        {reviews.length === 0 ? (
          <p className="text-gray-500 text-sm">Пока нет отзывов. Будьте первым!</p>
        ) : (
          <div className="space-y-4">
            {reviews.map((review) => (
              <div key={review.id} className="bg-white rounded-xl border border-gray-100 p-5 shadow-sm">
                <div className="flex items-center justify-between mb-2">
                  <span className="font-medium text-sm text-gray-900">{review.userName ?? 'Пользователь'}</span>
                  <span className="text-xs text-gray-400">
                    {new Date(review.createdAt).toLocaleDateString('ru-RU')}
                  </span>
                </div>
                <div className="flex items-center gap-1 mb-2">
                  {Array.from({ length: 5 }).map((_, i) => (
                    <span key={i} className={`text-sm ${i < review.rating ? 'text-yellow-400' : 'text-gray-200'}`}>
                      ★
                    </span>
                  ))}
                </div>
                {review.comment && <p className="text-sm text-gray-600">{review.comment}</p>}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
