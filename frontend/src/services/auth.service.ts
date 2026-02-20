import { api } from './api'

export interface LoginRequest {
  username: string
  password: string
}

export interface RegisterRequest {
  name: string
  username: string
  email: string
  password: string
}

export interface LoginResponse {
  token: string
  user: {
    id: number
    name: string
    username: string
    email: string | null
    avatarUrl: string | null
    permissionId: number
  }
}

export const authService = {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await api.post<LoginResponse>('/api/v1/open/login', credentials)
    api.setToken(response.data.token)
    return response.data
  },

  async register(data: RegisterRequest): Promise<LoginResponse> {
    const response = await api.post<LoginResponse>('/api/v1/open/register', data)
    api.setToken(response.data.token)
    return response.data
  },

  logout(): void {
    api.setToken(null)
  },

  isAuthenticated(): boolean {
    return !!api.getToken()
  }
}
