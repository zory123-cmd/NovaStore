import api from './api'
import type {
  Product,
  PagedResult,
  ProductSearchParams,
  CreateProductDto,
  UpdateProductDto,
} from '../types'

export const productService = {
  search: (params?: ProductSearchParams) =>
    api
      .get<PagedResult<Product>>('/products', { params })
      .then((r) => r.data),

  getById: (id: number) =>
    api.get<Product>(`/products/${id}`).then((r) => r.data),

  getByCategory: (id: number, page = 1, pageSize = 20) =>
    api
      .get<PagedResult<Product>>(`/products/category/${id}`, {
        params: { page, pageSize },
      })
      .then((r) => r.data),

  create: (data: CreateProductDto) =>
    api.post<Product>('/products', data).then((r) => r.data),

  update: (id: number, data: UpdateProductDto) =>
    api.put<Product>(`/products/${id}`, data).then((r) => r.data),

  delete: (id: number) => api.delete(`/products/${id}`),
}
