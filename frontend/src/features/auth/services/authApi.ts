import { api } from '@/services/api'
import type { LoginCredentials, AuthResponse, MeResponse } from '../types'

export interface RegisterRequest {
  name: string
  username: string
  email: string
  password: string
}

export const authApi = {
  async login(credentials: LoginCredentials): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/v1/open/login', credentials)
    return response.data
  },

  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/v1/open/register', data)
    api.setToken(response.data.token)
    return response.data
  },

  async me(): Promise<MeResponse> {
    const response = await api.get<MeResponse['user']>('/api/v1/session')
    return { user: response.data }
  },

  logout(): void {
    api.setToken(null)
  },
}

