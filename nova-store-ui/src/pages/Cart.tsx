import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { cartService } from '../services/cartService'
import { useAuth } from '../contexts/AuthContext'
import type { CartItem } from '../types'
import { CartItemSkeleton } from '../components/Skeleton'
import toast from 'react-hot-toast'

export default function Cart() {
  const { isAuthenticated } = useAuth()
  const [items, setItems] = useState<CartItem[]>([])
  const [loading, setLoading] = useState(true)
  const [promoCode, setPromoCode] = useState('')
  const [discount, setDiscount] = useState(0)

  const fetchCart = () => {
    if (!isAuthenticated) {
      setLoading(false)
      return
    }
    cartService
      .getCart()
      .then(setItems)
      .catch(() => {})
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    fetchCart()
  }, [isAuthenticated])

  const handleQuantity = async (id: number, quantity: number) => {
    if (quantity < 1) return
    try {
      await cartService.updateQuantity(id, quantity)
      setItems((prev) =>
        prev.map((item) =>
          item.id === id
            ? { ...item, quantity, subtotal: item.unitPrice * quantity }
            : item
        )
      )
    } catch {
      toast.error('Ошибка обновления количества')
    }
  }

  const handleRemove = async (id: number) => {
    try {
      await cartService.remove(id)
      setItems((prev) => prev.filter((item) => item.id !== id))
      toast.success('Товар удалён из корзины')
    } catch {
      toast.error('Ошибка удаления')
    }
  }

  const handleClear = async () => {
    if (!confirm('Очистить корзину?')) return
    try {
      await cartService.clear()
      setItems([])
      toast.success('Корзина очищена')
    } catch {
      toast.error('Ошибка очистки корзины')
    }
  }

  const handleApplyPromo = () => {
    if (promoCode.toUpperCase() === 'NOVA10') {
      setDiscount(10)
      toast.success('Промокод применён! Скидка 10%')
    } else {
      toast.error('Неверный промокод')
    }
  }

  const subtotal = items.reduce((sum, item) => sum + item.subtotal, 0)
  const total = discount > 0 ? subtotal * (1 - discount / 100) : subtotal

  if (!isAuthenticated) {
    return (
      <div className="text-center py-20">
        <div className="text-5xl mb-4">🛒</div>
        <h2 className="text-lg font-medium text-gray-900 mb-2">Войдите, чтобы увидеть корзину</h2>
        <Link to="/login" className="text-primary-600 hover:text-primary-700 font-medium">Войти</Link>
      </div>
    )
  }

  if (loading) {
    return (
      <div className="max-w-4xl mx-auto px-4 py-8 space-y-4">
        {Array.from({ length: 3 }).map((_, i) => <CartItemSkeleton key={i} />)}
      </div>
    )
  }

  if (items.length === 0) {
    return (
      <div className="text-center py-20">
        <div className="text-5xl mb-4">🛒</div>
        <h2 className="text-lg font-medium text-gray-900 mb-2">Корзина пуста</h2>
        <p className="text-gray-500 text-sm mb-4">Добавьте товары из каталога</p>
        <Link to="/" className="text-primary-600 hover:text-primary-700 font-medium">Перейти в каталог</Link>
      </div>
    )
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-bold text-gray-900">Корзина ({items.length})</h1>
        <button onClick={handleClear} className="text-sm text-red-500 hover:text-red-600 transition-colors">
          Очистить
        </button>
      </div>

      <div className="space-y-4 mb-8">
        {items.map((item) => (
          <div
            key={item.id}
            className="bg-white rounded-xl border border-gray-100 shadow-sm p-4 flex items-center gap-4"
          >
            <div className="w-20 h-20 bg-gray-100 rounded-lg flex items-center justify-center flex-shrink-0">
              {item.productImageUrl ? (
                <img src={item.productImageUrl} alt={item.productName} className="h-full w-full object-cover rounded-lg" />
              ) : (
                <span className="text-xl text-gray-300">{item.productName?.charAt(0)}</span>
              )}
            </div>
            <div className="flex-1 min-w-0">
              <Link to={`/products/${item.productId}`} className="text-sm font-medium text-gray-900 hover:text-primary-600 line-clamp-1">
                {item.productName}
              </Link>
              <p className="text-sm text-gray-500 mt-1">
                {item.unitPrice.toLocaleString('ru-RU')} ₽ × {item.quantity}
              </p>
              <p className="text-sm font-semibold text-primary-600 mt-1">
                {item.subtotal.toLocaleString('ru-RU')} ₽
              </p>
            </div>
            <div className="flex items-center gap-2">
              <div className="flex items-center border border-gray-300 rounded-lg">
                <button
                  onClick={() => handleQuantity(item.id, item.quantity - 1)}
                  className="px-2 py-1 text-gray-600 hover:bg-gray-50 text-sm"
                >
                  −
                </button>
                <span className="px-3 py-1 text-sm font-medium">{item.quantity}</span>
                <button
                  onClick={() => handleQuantity(item.id, item.quantity + 1)}
                  className="px-2 py-1 text-gray-600 hover:bg-gray-50 text-sm"
                >
                  +
                </button>
              </div>
              <button
                onClick={() => handleRemove(item.id)}
                className="p-2 text-gray-400 hover:text-red-500 transition-colors"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </button>
            </div>
          </div>
        ))}
      </div>

      <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
        <div className="flex items-center gap-2 mb-4">
          <input
            type="text"
            placeholder="Промокод"
            value={promoCode}
            onChange={(e) => setPromoCode(e.target.value)}
            className="flex-1 px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
          />
          <button
            onClick={handleApplyPromo}
            className="px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors"
          >
            Применить
          </button>
        </div>

        <div className="space-y-2 text-sm">
          <div className="flex justify-between text-gray-600">
            <span>Товары ({items.length})</span>
            <span>{subtotal.toLocaleString('ru-RU')} ₽</span>
          </div>
          {discount > 0 && (
            <div className="flex justify-between text-green-600">
              <span>Скидка ({discount}%)</span>
              <span>-{(subtotal * discount / 100).toLocaleString('ru-RU')} ₽</span>
            </div>
          )}
          <hr className="border-gray-200" />
          <div className="flex justify-between font-bold text-gray-900 text-base">
            <span>Итого</span>
            <span className="text-primary-600">{total.toLocaleString('ru-RU')} ₽</span>
          </div>
        </div>

        <Link
          to="/checkout"
          className="mt-4 block w-full py-3 bg-gradient-to-r from-primary-500 to-blue-500 text-white rounded-lg font-medium text-sm text-center hover:from-primary-600 hover:to-blue-600 transition-all shadow-md shadow-primary-200"
        >
          Оформить заказ
        </Link>
      </div>
    </div>
  )
}
