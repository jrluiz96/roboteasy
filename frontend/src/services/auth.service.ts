import { api } from './api'

export interface LoginRequest {
  username: string
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

  getGitHubLoginUrl(): string {
    return `${import.meta.env.VITE_API_URL || 'http://localhost:8080'}/api/v1/open/github/login`
  },

  logout(): void {
    api.setToken(null)
  },

  isAuthenticated(): boolean {
    return !!api.getToken()
  }
}
