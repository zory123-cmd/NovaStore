import api from './api'
import type { Category, CreateCategoryDto } from '../types'

export const categoryService = {
  getAll: () =>
    api.get<Category[]>('/categories').then((r) => r.data),

  getRoot: () =>
    api.get<Category[]>('/categories/root').then((r) => r.data),

  getById: (id: number) =>
    api.get<Category>(`/categories/${id}`).then((r) => r.data),

  getSubcategories: (id: number) =>
    api.get<Category[]>(`/categories/${id}/subcategories`).then((r) => r.data),

  create: (data: CreateCategoryDto) =>
    api.post<Category>('/categories', data).then((r) => r.data),

  update: (id: number, data: Partial<CreateCategoryDto>) =>
    api
      .put<Category>(`/categories/${id}`, data)
      .then((r) => r.data),

  delete: (id: number) => api.delete(`/categories/${id}`),
}
