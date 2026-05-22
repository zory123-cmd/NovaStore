import { createContext, useContext, useState, useEffect, type ReactNode } from 'react'
import { authService } from '../services/authService'
import { userService } from '../services/userService'
import type { User, LoginDto, RegisterUserDto } from '../types'
import toast from 'react-hot-toast'

interface AuthContextType {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  isAdmin: boolean
  login: (data: LoginDto) => Promise<void>
  register: (data: RegisterUserDto) => Promise<void>
  logout: () => void
  refreshUser: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [token, setToken] = useState<string | null>(() =>
    localStorage.getItem('token')
  )

  const isAuthenticated = !!token
  const isAdmin = user?.role === 'Admin'

  useEffect(() => {
    if (token) {
      userService
        .getProfile()
        .then(setUser)
        .catch(() => {
          localStorage.removeItem('token')
          localStorage.removeItem('user')
          setToken(null)
        })
    }
  }, [token])

  const login = async (data: LoginDto) => {
    const res = await authService.login(data)
    localStorage.setItem('token', res.token)
    localStorage.setItem('user', JSON.stringify(res.user))
    setToken(res.token)
    setUser(res.user)
    toast.success('Добро пожаловать!')
  }

  const register = async (data: RegisterUserDto) => {
    const res = await authService.register(data)
    localStorage.setItem('token', res.token)
    localStorage.setItem('user', JSON.stringify(res.user))
    setToken(res.token)
    setUser(res.user)
    toast.success('Регистрация прошла успешно!')
  }

  const logout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    setToken(null)
    setUser(null)
    toast.success('Вы вышли из системы')
  }

  const refreshUser = async () => {
    try {
      const u = await userService.getProfile()
      setUser(u)
    } catch {
      logout()
    }
  }

  return (
    <AuthContext.Provider
      value={{ user, token, isAuthenticated, isAdmin, login, register, logout, refreshUser }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
