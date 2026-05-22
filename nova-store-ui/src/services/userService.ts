import api from './api'
import type {
  User,
  UpdateUserDto,
  ChangePasswordDto,
  Address,
  CreateAddressDto,
} from '../types'

export const userService = {
  getProfile: () =>
    api.get<User>('/users/profile').then((r) => r.data),

  updateProfile: (data: UpdateUserDto) =>
    api.put<User>('/users/profile', data).then((r) => r.data),

  changePassword: (data: ChangePasswordDto) =>
    api.post('/users/change-password', data).then((r) => r.data),

  getAddresses: () =>
    api.get<Address[]>('/users/addresses').then((r) => r.data),

  addAddress: (data: CreateAddressDto) =>
    api.post<Address>('/users/addresses', data).then((r) => r.data),

  updateAddress: (id: number, data: CreateAddressDto) =>
    api.put<Address>(`/users/addresses/${id}`, data).then((r) => r.data),

  deleteAddress: (id: number) => api.delete(`/users/addresses/${id}`),
}
