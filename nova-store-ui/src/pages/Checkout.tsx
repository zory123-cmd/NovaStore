import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { cartService } from '../services/cartService'
import { orderService } from '../services/orderService'
import { userService } from '../services/userService'
import { useAuth } from '../contexts/AuthContext'
import type { Address, CartItem } from '../types'
import toast from 'react-hot-toast'

export default function Checkout() {
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()
  const [items, setItems] = useState<CartItem[]>([])
  const [addresses, setAddresses] = useState<Address[]>([])
  const [selectedAddressId, setSelectedAddressId] = useState<number | undefined>()
  const [paymentMethod, setPaymentMethod] = useState('Card')
  const [promoCode, setPromoCode] = useState('')
  const [shippingNotes, setShippingNotes] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const [newAddress, setNewAddress] = useState(false)
  const [addressForm, setAddressForm] = useState({
    fullName: '', phone: '', street: '', city: '',
    state: '', zipCode: '', country: 'Россия', isDefault: false,
  })

  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login')
      return
    }
    cartService.getCart().then((data) => {
      if (data.length === 0) {
        navigate('/cart')
        return
      }
      setItems(data)
    })
    userService.getAddresses().then((data) => {
      setAddresses(data)
      const def = data.find((a) => a.isDefault)
      if (def) setSelectedAddressId(def.id)
    })
  }, [isAuthenticated, navigate])

  const handleAddAddress = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      const addr = await userService.addAddress(addressForm)
      setAddresses((prev) => [...prev, addr])
      setSelectedAddressId(addr.id)
      setNewAddress(false)
      setAddressForm({ fullName: '', phone: '', street: '', city: '', state: '', zipCode: '', country: 'Россия', isDefault: false })
      toast.success('Адрес добавлен')
    } catch {
      toast.error('Ошибка добавления адреса')
    }
  }

  const handleSubmit = async () => {
    if (!selectedAddressId) {
      toast.error('Выберите адрес доставки')
      return
    }
    setSubmitting(true)
    try {
      const order = await orderService.createFromCart({
        shippingAddressId: selectedAddressId,
        shippingNotes: shippingNotes || undefined,
        paymentMethod,
        promoCode: promoCode || undefined,
      })
      toast.success('Заказ оформлен!')
      navigate(`/orders/${order.id}`)
    } catch (err: any) {
      toast.error(err.response?.data?.message ?? 'Ошибка оформления заказа')
    } finally {
      setSubmitting(false)
    }
  }

  const total = items.reduce((sum, i) => sum + i.subtotal, 0)

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <h1 className="text-xl font-bold text-gray-900 mb-6">Оформление заказа</h1>

      <div className="grid md:grid-cols-3 gap-6">
        <div className="md:col-span-2 space-y-6">
          <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
            <h2 className="font-semibold text-gray-900 mb-4">Адрес доставки</h2>
            {addresses.length > 0 && !newAddress ? (
              <div className="space-y-3">
                {addresses.map((addr) => (
                  <label
                    key={addr.id}
                    className={`flex items-start gap-3 p-3 rounded-lg border cursor-pointer transition-colors ${
                      selectedAddressId === addr.id
                        ? 'border-primary-500 bg-primary-50'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    <input
                      type="radio"
                      name="address"
                      checked={selectedAddressId === addr.id}
                      onChange={() => setSelectedAddressId(addr.id)}
                      className="mt-1"
                    />
                    <div className="text-sm text-gray-600">
                      <p className="font-medium text-gray-900">{addr.fullName}</p>
                      <p>{addr.street}, {addr.city}</p>
                      <p>{addr.zipCode}, {addr.country}</p>
                      <p className="text-gray-400">{addr.phone}</p>
                    </div>
                  </label>
                ))}
                <button
                  onClick={() => setNewAddress(true)}
                  className="text-sm text-primary-600 hover:text-primary-700 font-medium"
                >
                  + Добавить новый адрес
                </button>
              </div>
            ) : (
              <form onSubmit={handleAddAddress} className="space-y-3">
                <div className="grid grid-cols-2 gap-3">
                  <input placeholder="Полное имя *" value={addressForm.fullName} onChange={(e) => setAddressForm({ ...addressForm, fullName: e.target.value })} className="col-span-2 px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" required />
                  <input placeholder="Телефон *" value={addressForm.phone} onChange={(e) => setAddressForm({ ...addressForm, phone: e.target.value })} className="px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" required />
                  <input placeholder="Индекс *" value={addressForm.zipCode} onChange={(e) => setAddressForm({ ...addressForm, zipCode: e.target.value })} className="px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" required />
                </div>
                <input placeholder="Улица, дом, квартира *" value={addressForm.street} onChange={(e) => setAddressForm({ ...addressForm, street: e.target.value })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" required />
                <div className="grid grid-cols-2 gap-3">
                  <input placeholder="Город *" value={addressForm.city} onChange={(e) => setAddressForm({ ...addressForm, city: e.target.value })} className="px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" required />
                  <input placeholder="Регион" value={addressForm.state} onChange={(e) => setAddressForm({ ...addressForm, state: e.target.value })} className="px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" />
                </div>
                <div className="flex gap-3">
                  <button type="submit" className="px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors">Сохранить</button>
                  {addresses.length > 0 && (
                    <button type="button" onClick={() => setNewAddress(false)} className="px-4 py-2 text-gray-600 rounded-lg text-sm hover:bg-gray-50 transition-colors">Отмена</button>
                  )}
                </div>
              </form>
            )}
          </div>

          <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
            <h2 className="font-semibold text-gray-900 mb-3">Способ оплаты</h2>
            <div className="space-y-2">
              {['Card', 'Cash'].map((method) => (
                <label key={method} className={`flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors ${
                  paymentMethod === method ? 'border-primary-500 bg-primary-50' : 'border-gray-200'
                }`}>
                  <input type="radio" name="payment" checked={paymentMethod === method} onChange={() => setPaymentMethod(method)} />
                  <span className="text-sm font-medium text-gray-900">
                    {method === 'Card' ? 'Картой' : 'Наличными'}
                  </span>
                </label>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
            <h2 className="font-semibold text-gray-900 mb-3">Комментарий к заказу</h2>
            <textarea
              placeholder="Примечания к доставке..."
              value={shippingNotes}
              onChange={(e) => setShippingNotes(e.target.value)}
              rows={3}
              className="w-full px-4 py-2.5 rounded-lg border border-gray-300 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none text-sm"
            />
          </div>
        </div>

        <div className="md:col-span-1">
          <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 sticky top-24">
            <h2 className="font-semibold text-gray-900 mb-4">Ваш заказ</h2>
            <div className="space-y-3 mb-4 max-h-60 overflow-y-auto">
              {items.map((item) => (
                <div key={item.id} className="flex justify-between text-sm">
                  <span className="text-gray-600 line-clamp-1 flex-1">{item.productName} × {item.quantity}</span>
                  <span className="font-medium text-gray-900 ml-2">{item.subtotal.toLocaleString('ru-RU')} ₽</span>
                </div>
              ))}
            </div>

            <div className="mb-4">
              <input
                type="text"
                placeholder="Промокод"
                value={promoCode}
                onChange={(e) => setPromoCode(e.target.value)}
                className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none"
              />
            </div>

            <hr className="mb-4" />
            <div className="flex justify-between font-bold text-lg mb-6">
              <span>Итого</span>
              <span className="text-primary-600">{total.toLocaleString('ru-RU')} ₽</span>
            </div>

            <button
              onClick={handleSubmit}
              disabled={submitting || !selectedAddressId}
              className="w-full py-3 bg-gradient-to-r from-primary-500 to-blue-500 text-white rounded-lg font-medium text-sm hover:from-primary-600 hover:to-blue-600 disabled:opacity-50 transition-all shadow-md shadow-primary-200"
            >
              {submitting ? 'Оформление...' : 'Подтвердить заказ'}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
