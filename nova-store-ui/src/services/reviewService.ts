import api from './api'
import type { Review, CreateReviewDto, PagedResult } from '../types'

export const reviewService = {
  getByProduct: (productId: number, page = 1, pageSize = 20) =>
    api
      .get<PagedResult<Review>>(`/reviews/product/${productId}`, {
        params: { page, pageSize },
      })
      .then((r) => r.data),

  create: (data: CreateReviewDto) =>
    api.post<Review>('/reviews', data).then((r) => r.data),

  update: (id: number, data: CreateReviewDto) =>
    api.put<Review>(`/reviews/${id}`, data).then((r) => r.data),

  delete: (id: number) => api.delete(`/reviews/${id}`),
}
