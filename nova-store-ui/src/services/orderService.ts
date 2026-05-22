import api from './api'
import type { Order, CreateOrderDto } from '../types'

export const orderService = {
  getMyOrders: () =>
    api.get<Order[]>('/orders/my').then((r) => r.data),

  getById: (id: number) =>
    api.get<Order>(`/orders/${id}`).then((r) => r.data),

  createFromCart: (data: CreateOrderDto) =>
    api.post<Order>('/orders', data).then((r) => r.data),

  getAll: () =>
    api.get<Order[]>('/orders').then((r) => r.data),

  updateStatus: (id: number, data: {
    status?: string
    paymentStatus?: string
    shippingStatus?: string
    shippedAt?: string
    deliveredAt?: string
  }) =>
    api.put<Order>(`/orders/${id}/status`, data).then((r) => r.data),

  delete: (id: number) => api.delete(`/orders/${id}`),
}
