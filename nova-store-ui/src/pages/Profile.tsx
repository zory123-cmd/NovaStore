import { useEffect, useState } from 'react'
import { useAuth } from '../contexts/AuthContext'
import { userService } from '../services/userService'
import type { Address } from '../types'
import toast from 'react-hot-toast'

export default function Profile() {
  const { user, isAuthenticated, refreshUser } = useAuth()
  const [addresses, setAddresses] = useState<Address[]>([])
  const [profileForm, setProfileForm] = useState({
    fullName: '', phone: '', avatarUrl: '',
  })
  const [passwordForm, setPasswordForm] = useState({
    currentPassword: '', newPassword: '',
  })
  const [saving, setSaving] = useState(false)
  const [changingPassword, setChangingPassword] = useState(false)
  const [newAddress, setNewAddress] = useState(false)
  const [addressForm, setAddressForm] = useState({
    fullName: '', phone: '', street: '', city: '',
    state: '', zipCode: '', country: 'Россия', isDefault: false,
  })

  useEffect(() => {
    if (!isAuthenticated) return
    userService.getAddresses().then(setAddresses).catch(() => {})
  }, [isAuthenticated])

  useEffect(() => {
    if (user) {
      setProfileForm({
        fullName: user.fullName ?? '',
        phone: user.phone ?? '',
        avatarUrl: user.avatarUrl ?? '',
      })
    }
  }, [user])

  const handleSaveProfile = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      await userService.updateProfile(profileForm)
      await refreshUser()
      toast.success('Профиль обновлён')
    } catch {
      toast.error('Ошибка обновления профиля')
    } finally {
      setSaving(false)
    }
  }

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault()
    setChangingPassword(true)
    try {
      await userService.changePassword(passwordForm)
      setPasswordForm({ currentPassword: '', newPassword: '' })
      toast.success('Пароль изменён')
    } catch (err: any) {
      toast.error(err.response?.data?.message ?? 'Ошибка смены пароля')
    } finally {
      setChangingPassword(false)
    }
  }

  const handleAddAddress = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      const addr = await userService.addAddress(addressForm)
      setAddresses((prev) => [...prev, addr])
      setNewAddress(false)
      setAddressForm({ fullName: '', phone: '', street: '', city: '', state: '', zipCode: '', country: 'Россия', isDefault: false })
      toast.success('Адрес добавлен')
    } catch {
      toast.error('Ошибка добавления адреса')
    }
  }

  const handleDeleteAddress = async (id: number) => {
    if (!confirm('Удалить адрес?')) return
    try {
      await userService.deleteAddress(id)
      setAddresses((prev) => prev.filter((a) => a.id !== id))
      toast.success('Адрес удалён')
    } catch {
      toast.error('Ошибка удаления адреса')
    }
  }

  if (!isAuthenticated || !user) {
    return <div className="text-center py-20">Войдите, чтобы увидеть профиль</div>
  }

  return (
    <div className="max-w-3xl mx-auto px-4 py-8 space-y-6">
      <h1 className="text-xl font-bold text-gray-900">Профиль</h1>

      <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-6">
        <h2 className="font-semibold text-gray-900 mb-4">Личные данные</h2>
        <form onSubmit={handleSaveProfile} className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Имя пользователя</label>
              <input value={user.username} disabled className="w-full px-4 py-2 rounded-lg border border-gray-200 text-sm bg-gray-50 text-gray-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input value={user.email} disabled className="w-full px-4 py-2 rounded-lg border border-gray-200 text-sm bg-gray-50 text-gray-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Полное имя</label>
              <input value={profileForm.fullName} onChange={(e) => setProfileForm({ ...profileForm, fullName: e.target.value })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Телефон</label>
              <input value={profileForm.phone} onChange={(e) => setProfileForm({ ...profileForm, phone: e.target.value })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">URL аватара</label>
            <input value={profileForm.avatarUrl} onChange={(e) => setProfileForm({ ...profileForm, avatarUrl: e.target.value })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" />
          </div>
          <button type="submit" disabled={saving} className="px-6 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 disabled:opacity-50 transition-colors">
            {saving ? 'Сохранение...' : 'Сохранить'}
          </button>
        </form>
      </div>

      <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-6">
        <h2 className="font-semibold text-gray-900 mb-4">Смена пароля</h2>
        <form onSubmit={handleChangePassword} className="space-y-4 max-w-sm">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Текущий пароль</label>
            <input type="password" value={passwordForm.currentPassword} onChange={(e) => setPasswordForm({ ...passwordForm, currentPassword: e.target.value })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" required />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Новый пароль</label>
            <input type="password" value={passwordForm.newPassword} onChange={(e) => setPasswordForm({ ...passwordForm, newPassword: e.target.value })} className="w-full px-4 py-2 rounded-lg border border-gray-300 text-sm focus:border-primary-500 outline-none" required minLength={8} />
          </div>
          <button type="submit" disabled={changingPassword} className="px-6 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 disabled:opacity-50 transition-colors">
            {changingPassword ? 'Смена...' : 'Изменить пароль'}
          </button>
        </form>
      </div>

      <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="font-semibold text-gray-900">Адреса доставки</h2>
          <button onClick={() => setNewAddress(true)} className="text-sm text-primary-600 hover:text-primary-700 font-medium">
            + Добавить
          </button>
        </div>

        {newAddress && (
          <form onSubmit={handleAddAddress} className="mb-6 p-4 bg-gray-50 rounded-lg space-y-3">
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
            <div className="flex gap-2">
              <button type="submit" className="px-4 py-2 bg-primary-500 text-white rounded-lg text-sm font-medium hover:bg-primary-600 transition-colors">Сохранить</button>
              <button type="button" onClick={() => setNewAddress(false)} className="px-4 py-2 text-gray-600 rounded-lg text-sm hover:bg-gray-100 transition-colors">Отмена</button>
            </div>
          </form>
        )}

        {addresses.length === 0 ? (
          <p className="text-sm text-gray-500">Нет сохранённых адресов</p>
        ) : (
          <div className="space-y-3">
            {addresses.map((addr) => (
              <div key={addr.id} className="flex items-start justify-between p-4 border border-gray-200 rounded-lg">
                <div className="text-sm text-gray-600">
                  <p className="font-medium text-gray-900">{addr.fullName}</p>
                  <p>{addr.street}, {addr.city}</p>
                  <p>{addr.zipCode}, {addr.country}</p>
                  <p className="text-gray-400">{addr.phone}</p>
                  {addr.isDefault && <span className="text-xs text-primary-600 font-medium">По умолчанию</span>}
                </div>
                <button onClick={() => handleDeleteAddress(addr.id)} className="text-sm text-red-500 hover:text-red-600 flex-shrink-0">Удалить</button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
