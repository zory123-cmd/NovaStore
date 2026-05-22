import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { orderService } from '../services/orderService'
import type { Order } from '../types'
import LoadingSpinner from '../components/LoadingSpinner'
import toast from 'react-hot-toast'

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

export default function OrderDetail() {
  const { id } = useParams<{ id: string }>()
  const [order, setOrder] = useState<Order | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!id) return
    orderService
      .getById(Number(id))
      .then(setOrder)
      .catch(() => toast.error('Заказ не найден'))
      .finally(() => setLoading(false))
  }, [id])

  if (loading) return <LoadingSpinner />

  if (!order) {
    return (
      <div className="text-center py-20">
        <h2 className="text-lg font-medium text-gray-900">Заказ не найден</h2>
        <Link to="/orders" className="text-primary-600 hover:text-primary-700 font-medium mt-2 inline-block">Мои заказы</Link>
      </div>
    )
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <Link to="/orders" className="text-sm text-primary-600 hover:text-primary-700 mb-4 inline-block">
        ← Мои заказы
      </Link>

      <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-6 mb-6">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-xl font-bold text-gray-900">Заказ №{order.id}</h1>
          <span className={`px-3 py-1 rounded-full text-xs font-medium ${statusColors[order.status] ?? 'bg-gray-100'}`}>
            {statusLabels[order.status] ?? order.status}
          </span>
        </div>

        <div className="grid sm:grid-cols-2 gap-4 text-sm mb-6">
          <div>
            <p className="text-gray-500">Дата заказа</p>
            <p className="font-medium text-gray-900">
              {new Date(order.orderDate).toLocaleDateString('ru-RU', {
                year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit',
              })}
            </p>
          </div>
          <div>
            <p className="text-gray-500">Способ оплаты</p>
            <p className="font-medium text-gray-900">{order.paymentMethod === 'Card' ? 'Картой' : 'Наличными'}</p>
          </div>
          <div>
            <p className="text-gray-500">Статус оплаты</p>
            <p className="font-medium text-gray-900">{order.paymentStatus}</p>
          </div>
          <div>
            <p className="text-gray-500">Статус доставки</p>
            <p className="font-medium text-gray-900">{order.shippingStatus}</p>
          </div>
          {order.promoCode && (
            <div>
              <p className="text-gray-500">Промокод</p>
              <p className="font-medium text-gray-900">{order.promoCode}</p>
            </div>
          )}
        </div>

        {order.shippingNotes && (
          <div className="mb-6">
            <p className="text-sm text-gray-500">Комментарий</p>
            <p className="text-sm text-gray-900 mt-1">{order.shippingNotes}</p>
          </div>
        )}
      </div>

      <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-6 mb-6">
        <h2 className="font-semibold text-gray-900 mb-4">Товары в заказе</h2>
        <div className="space-y-4">
          {order.orderItems.map((item) => (
            <div key={item.id} className="flex items-center gap-4">
              <div className="w-16 h-16 bg-gray-100 rounded-lg flex items-center justify-center flex-shrink-0">
                {item.productImageUrl ? (
                  <img src={item.productImageUrl} alt={item.productName} className="h-full w-full object-cover rounded-lg" />
                ) : (
                  <span className="text-lg text-gray-300">{item.productName.charAt(0)}</span>
                )}
              </div>
              <div className="flex-1 min-w-0">
                <Link to={`/products/${item.productId}`} className="text-sm font-medium text-gray-900 hover:text-primary-600 line-clamp-1">
                  {item.productName}
                </Link>
                <p className="text-sm text-gray-500">{item.unitPrice.toLocaleString('ru-RU')} ₽ × {item.quantity}</p>
              </div>
              <span className="font-medium text-gray-900">{item.subtotal.toLocaleString('ru-RU')} ₽</span>
            </div>
          ))}
        </div>

        <hr className="my-4" />
        <div className="flex justify-between items-center">
          <span className="font-semibold text-gray-900">Итого</span>
          <span className="text-xl font-bold text-primary-600">
            {order.totalAmount.toLocaleString('ru-RU')} ₽
          </span>
        </div>
      </div>
    </div>
  )
}
