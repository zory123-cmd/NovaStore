import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { orderService } from '../services/orderService'
import { useAuth } from '../contexts/AuthContext'
import type { Order } from '../types'
import LoadingSpinner from '../components/LoadingSpinner'

const statusColors: Record<string, string> = {
  Created: 'bg-blue-100 text-blue-800',
  Processing: 'bg-yellow-100 text-yellow-800',
  Shipped: 'bg-purple-100 text-purple-800',
  Delivered: 'bg-green-100 text-green-800',
  Cancelled: 'bg-red-100 text-red-800',
}

const statusLabels: Record<string, string> = {
  Created: 'Создан',
  Processing: 'Обрабатывается',
  Shipped: 'Отправлен',
  Delivered: 'Доставлен',
  Cancelled: 'Отменён',
}

export default function MyOrders() {
  const { isAuthenticated } = useAuth()
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!isAuthenticated) {
      setLoading(false)
      return
    }
    orderService
      .getMyOrders()
      .then(setOrders)
      .catch(() => {})
      .finally(() => setLoading(false))
  }, [isAuthenticated])

  if (!isAuthenticated) {
    return (
      <div className="text-center py-20">
        <h2 className="text-lg font-medium text-gray-900 mb-2">Войдите, чтобы увидеть заказы</h2>
        <Link to="/login" className="text-primary-600 hover:text-primary-700 font-medium">Войти</Link>
      </div>
    )
  }

  if (loading) return <LoadingSpinner />

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <h1 className="text-xl font-bold text-gray-900 mb-6">Мои заказы</h1>

      {orders.length === 0 ? (
        <div className="text-center py-16">
          <div className="text-5xl mb-4">📦</div>
          <h2 className="text-lg font-medium text-gray-900 mb-2">У вас ещё нет заказов</h2>
          <Link to="/" className="text-primary-600 hover:text-primary-700 font-medium">Начать покупки</Link>
        </div>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => (
            <Link
              key={order.id}
              to={`/orders/${order.id}`}
              className="block bg-white rounded-xl border border-gray-100 shadow-sm p-5 hover:shadow-md transition-shadow"
            >
              <div className="flex items-center justify-between mb-3">
                <div>
                  <span className="text-sm text-gray-500">Заказ №{order.id}</span>
                  <span className="mx-2 text-gray-300">·</span>
                  <span className="text-sm text-gray-500">
                    {new Date(order.orderDate).toLocaleDateString('ru-RU')}
                  </span>
                </div>
                <span className={`px-3 py-1 rounded-full text-xs font-medium ${statusColors[order.status] ?? 'bg-gray-100 text-gray-800'}`}>
                  {statusLabels[order.status] ?? order.status}
                </span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex gap-2 flex-wrap">
                  {order.orderItems.slice(0, 3).map((item) => (
                    <div key={item.id} className="w-12 h-12 bg-gray-100 rounded-lg flex items-center justify-center text-xs text-gray-400">
                      {item.productName.charAt(0)}
                    </div>
                  ))}
                  {order.orderItems.length > 3 && (
                    <div className="w-12 h-12 bg-gray-50 rounded-lg flex items-center justify-center text-xs text-gray-400">
                      +{order.orderItems.length - 3}
                    </div>
                  )}
                </div>
                <span className="text-lg font-bold text-primary-600">
                  {order.totalAmount.toLocaleString('ru-RU')} ₽
                </span>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}
