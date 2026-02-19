export interface User {
  id: number
  username: string
  email: string
  name?: string
  avatarUrl?: string
  permissionName?: string
  permissionId?: number
  views?: Array<{
    id: number
    name: string
    route: string
    icon: string
  }>
}

export interface LoginCredentials {
  username: string
  password: string
}

export interface AuthResponse {
  token: string
  user: User
}

export interface MeResponse {
  user: User
}
