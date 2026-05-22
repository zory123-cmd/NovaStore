import { useEffect, useState } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { productService } from '../services/productService'
import { categoryService } from '../services/categoryService'
import { orderService } from '../services/orderService'
import type { Product, Category, Order, CreateProductDto } from '../types'
import toast from 'react-hot-toast'

type Tab = 'products' | 'categories' | 'orders'

export default function AdminDashboard() {
  const { isAdmin } = useAuth()
  const [tab, setTab] = useState<Tab>('products')

  if (!isAdmin) {
    return <div className="text-center py-20 text-gray-500">Доступ запрещён</div>
  }

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <h1 className="text-xl font-bold text-gray-900 mb-6">Панель администратора</h1>

      <div className="flex gap-2 mb-6 border-b border-gray-200 pb-2">
        {(['products', 'categories', 'orders'] as Tab[]).map((t) => (
          <button
            key={t}
            onClick={() => setTab(t)}
            className={`px-4 py-2 text-sm font-medium rounded-t-lg transition-colors ${
              tab === t
                ? 'bg-primary-50 text-primary-700 border-b-2 border-primary-500'
                : 'text-gray-500 hover:text-gray-700'
            }`}
          >
            {t === 'products' ? 'Товары' : t === 'categories' ? 'Категории' : 'Заказы'}
          </button>
        ))}
      </div>

      {tab === 'products' && <AdminProducts />}
      {tab === 'categories' && <AdminCategories />}
      {tab === 'orders' && <AdminOrders />}
    </div>
  )
}

function AdminProducts() {
  const [products, setProducts] = useState<Product[]>([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [categories, setCategories] = useState<Category[]>([])
  const [form, setForm] = useState<CreateProductDto>({
    name: '', description: '', price: 0, stockQuantity: 0,
    categoryId: 0, imageUrl: '', brand: '',
  })

  const fetchProducts = () => {
    setLoading(true)
    productService.search({ pageSize: 100 }).then((r) => setProducts(r.items)).catch(() => {}).finally(() => setLoading(false))
  }

  useEffect(() => {
    fetchProducts()
    categoryService.getAll().then(setCategories).catch(() => {})
  }, [])

  const resetForm = () => {
    setForm({ name: '', description: '', price: 0, stockQuantity: 0, categoryId: 0, imageUrl: '', brand: '' })
    setEditingId(null)
    setShowForm(false)
  }

  const handleEdit = (p: Product) => {
    setForm({
      name: p.name, description: p.description ?? '', price: p.price,
      stockQuantity: p.stockQuantity, categoryId: p.categoryId,
      imageUrl: p.imageUrl ?? '', brand: p.brand ?? '',
    })
    setEditingId(p.id)
    setShowForm(true)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editingId) {
        await productService.update(editingId, form)
        toast.success('Товар обновлён')
      } else {
        await productService.create(form)
        toast.success('Товар создан')
      }
      resetForm()
      fetchProducts()
    } catch {
      toast.error('Ошибка сохранения товара')
    }
  }

  const handleDelete = async (id: number) => {
    if (!confirm('Удалить товар?')) return
    try {
      await productService.delete(id)
      toast.success('Товар удалён')
      fetchProducts()
    } catch {
      toast.error('Ошибка удаления')
    }
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-4">
        <h2 className="font-semibold text-gray-900">Управление товарами</h2>
        <button onClick={() => { resetForm(); setShowForm(true) }} className="px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors">
          + Добавить товар
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 mb-6 space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">Название *</label>
              <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" required />
            </div>
            <div className="col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">Описание</label>
              <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} rows={3} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Цена *</label>
              <input type="number" step="0.01" value={form.price} onChange={(e) => setForm({ ...form, price: Number(e.target.value) })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" required />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Количество</label>
              <input type="number" value={form.stockQuantity} onChange={(e) => setForm({ ...form, stockQuantity: Number(e.target.value) })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Категория *</label>
              <select value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: Number(e.target.value) })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" required>
                <option value={0}>Выберите категорию</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Бренд</label>
              <input value={form.brand} onChange={(e) => setForm({ ...form, brand: e.target.value })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" />
            </div>
            <div className="col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">URL изображения</label>
              <input value={form.imageUrl} onChange={(e) => setForm({ ...form, imageUrl: e.target.value })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" />
            </div>
          </div>
          <div className="flex gap-2">
            <button type="submit" className="px-6 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors">
              {editingId ? 'Обновить' : 'Создать'}
            </button>
            <button type="button" onClick={resetForm} className="px-6 py-2 text-gray-600 rounded-lg text-sm hover:bg-gray-50 transition-colors">Отмена</button>
          </div>
        </form>
      )}

      <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-600">ID</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Название</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Цена</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Кол-во</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Категория</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Действия</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {products.map((p) => (
                <tr key={p.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 text-gray-500">{p.id}</td>
                  <td className="px-4 py-3 font-medium text-gray-900 max-w-xs truncate">{p.name}</td>
                  <td className="px-4 py-3">{p.price.toLocaleString('ru-RU')} ₽</td>
                  <td className="px-4 py-3">{p.stockQuantity}</td>
                  <td className="px-4 py-3 text-gray-500">{p.categoryName}</td>
                  <td className="px-4 py-3">
                    <div className="flex gap-2">
                      <button onClick={() => handleEdit(p)} className="text-primary-600 hover:text-primary-700 text-xs font-medium">Ред.</button>
                      <button onClick={() => handleDelete(p.id)} className="text-red-500 hover:text-red-600 text-xs font-medium">Уд.</button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {products.length === 0 && <p className="text-center py-8 text-gray-500">Нет товаров</p>}
      </div>
    </div>
  )
}

function AdminCategories() {
  const [categories, setCategories] = useState<Category[]>([])
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState({ name: '', description: '', imageUrl: '', parentCategoryId: 0 })

  const fetchCategories = () => {
    categoryService.getAll().then(setCategories).catch(() => {})
  }

  useEffect(() => { fetchCategories() }, [])

  const resetForm = () => {
    setForm({ name: '', description: '', imageUrl: '', parentCategoryId: 0 })
    setEditingId(null)
    setShowForm(false)
  }

  const handleEdit = (c: Category) => {
    setForm({ name: c.name, description: c.description ?? '', imageUrl: c.imageUrl ?? '', parentCategoryId: c.parentCategoryId ?? 0 })
    setEditingId(c.id)
    setShowForm(true)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editingId) {
        await categoryService.update(editingId, form)
        toast.success('Категория обновлена')
      } else {
        await categoryService.create(form)
        toast.success('Категория создана')
      }
      resetForm()
      fetchCategories()
    } catch {
      toast.error('Ошибка сохранения категории')
    }
  }

  const handleDelete = async (id: number) => {
    if (!confirm('Удалить категорию?')) return
    try {
      await categoryService.delete(id)
      toast.success('Категория удалена')
      fetchCategories()
    } catch {
      toast.error('Ошибка удаления')
    }
  }

  const renderCategory = (cat: Category, depth = 0) => (
    <tr key={cat.id} className="hover:bg-gray-50">
      <td className="px-4 py-3 text-gray-500">{cat.id}</td>
      <td className={`px-4 py-3 font-medium text-gray-900`} style={{ paddingLeft: `${16 + depth * 20}px` }}>
        {cat.name}
      </td>
      <td className="px-4 py-3 text-gray-500 text-xs max-w-xs truncate">{cat.description}</td>
      <td className="px-4 py-3">
        <div className="flex gap-2">
          <button onClick={() => handleEdit(cat)} className="text-primary-600 hover:text-primary-700 text-xs font-medium">Ред.</button>
          <button onClick={() => handleDelete(cat.id)} className="text-red-500 hover:text-red-600 text-xs font-medium">Уд.</button>
        </div>
      </td>
    </tr>
  )

  const flattenCategories = (cats: Category[], depth = 0): { cat: Category; depth: number }[] => {
    const result: { cat: Category; depth: number }[] = []
    for (const c of cats) {
      result.push({ cat: c, depth })
      if (c.subCategories) {
        result.push(...flattenCategories(c.subCategories, depth + 1))
      }
    }
    return result
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-4">
        <h2 className="font-semibold text-gray-900">Управление категориями</h2>
        <button onClick={() => { resetForm(); setShowForm(true) }} className="px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors">
          + Добавить категорию
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="bg-white rounded-xl border border-gray-100 shadow-sm p-5 mb-6 space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Название *</label>
              <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" required />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Родительская категория</label>
              <select value={form.parentCategoryId} onChange={(e) => setForm({ ...form, parentCategoryId: Number(e.target.value) })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none">
                <option value={0}>— Корневая —</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>
            <div className="col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">Описание</label>
              <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} rows={2} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" />
            </div>
            <div className="col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">URL изображения</label>
              <input value={form.imageUrl} onChange={(e) => setForm({ ...form, imageUrl: e.target.value })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" />
            </div>
          </div>
          <div className="flex gap-2">
            <button type="submit" className="px-6 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors">
              {editingId ? 'Обновить' : 'Создать'}
            </button>
            <button type="button" onClick={resetForm} className="px-6 py-2 text-gray-600 rounded-lg text-sm hover:bg-gray-50 transition-colors">Отмена</button>
          </div>
        </form>
      )}

      <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="text-left px-4 py-3 font-medium text-gray-600">ID</th>
              <th className="text-left px-4 py-3 font-medium text-gray-600">Название</th>
              <th className="text-left px-4 py-3 font-medium text-gray-600">Описание</th>
              <th className="text-left px-4 py-3 font-medium text-gray-600">Действия</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100">
            {flattenCategories(categories).map(({ cat, depth }) => renderCategory(cat, depth))}
          </tbody>
        </table>
        {categories.length === 0 && <p className="text-center py-8 text-gray-500">Нет категорий</p>}
      </div>
    </div>
  )
}

function AdminOrders() {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    orderService.getAll().then(setOrders).catch(() => {}).finally(() => setLoading(false))
  }, [])

  const handleStatus = async (id: number, status: string) => {
    try {
      await orderService.updateStatus(id, { status })
      setOrders((prev) => prev.map((o) => (o.id === id ? { ...o, status } : o)))
      toast.success('Статус обновлён')
    } catch {
      toast.error('Ошибка обновления статуса')
    }
  }

  const statuses = ['Created', 'Processing', 'Shipped', 'Delivered', 'Cancelled']

  return (
    <div>
      <h2 className="font-semibold text-gray-900 mb-4">Управление заказами</h2>
      <div className="bg-white rounded-xl border border-gray-100 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-600">№</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Клиент</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Дата</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Сумма</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Статус</th>
                <th className="text-left px-4 py-3 font-medium text-gray-600">Действия</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {orders.map((order) => (
                <tr key={order.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium">{order.id}</td>
                  <td className="px-4 py-3 text-gray-600">{order.customerName}</td>
                  <td className="px-4 py-3 text-gray-500 text-xs">
                    {new Date(order.orderDate).toLocaleDateString('ru-RU')}
                  </td>
                  <td className="px-4 py-3 font-medium">{order.totalAmount.toLocaleString('ru-RU')} ₽</td>
                  <td className="px-4 py-3">{order.status}</td>
                  <td className="px-4 py-3">
                    <select
                      value={order.status}
                      onChange={(e) => handleStatus(order.id, e.target.value)}
                      className="px-2 py-1 rounded border border-gray-300 text-xs focus:border-primary-500 outline-none"
                    >
                      {statuses.map((s) => (
                        <option key={s} value={s}>{s}</option>
                      ))}
                    </select>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {orders.length === 0 && !loading && <p className="text-center py-8 text-gray-500">Нет заказов</p>}
      </div>
    </div>
  )
}
