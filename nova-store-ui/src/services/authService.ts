import api from './api'
import type { LoginDto, LoginResponse, RegisterUserDto } from '../types'

export const authService = {
  login: (data: LoginDto) =>
    api.post<LoginResponse>('/auth/login', data).then((r) => r.data),

  register: (data: RegisterUserDto) =>
    api.post<LoginResponse>('/auth/register', data).then((r) => r.data),
}
