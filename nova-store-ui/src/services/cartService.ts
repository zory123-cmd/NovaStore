import api from './api'
import type { CartItem, AddToCartDto } from '../types'

export const cartService = {
  getCart: () =>
    api.get<CartItem[]>('/cart').then((r) => r.data),

  addToCart: (data: AddToCartDto) =>
    api.post<CartItem>('/cart', data).then((r) => r.data),

  updateQuantity: (id: number, quantity: number) =>
    api.put(`/cart/${id}`, { quantity }).then((r) => r.data),

  remove: (id: number) => api.delete(`/cart/${id}`),

  clear: () => api.delete('/cart'),
}
