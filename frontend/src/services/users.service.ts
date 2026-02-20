import { api } from './api'

export interface User {
  id: number
  name: string
  username: string
  email: string
  avatarUrl: string | null
  permissionId: number
  permissionName: string
  sessionAt: string | null
  createdAt: string
  deletedAt: string | null
  isOnline: boolean
}

export interface Permission {
  id: number
  name: string
}

export interface CreateUserRequest {
  name: string
  username: string
  email: string
  permissionId: number
  password: string
}

export interface UpdateUserRequest {
  name?: string
  email?: string
  permissionId?: number
  password?: string
}

interface UserOptionsResponse {
  permissions: Permission[]
}

export const usersService = {
  async getAll(): Promise<User[]> {
    const response = await api.get<User[]>('/api/v1/users')
    return response.data ?? []
  },

  async create(userData: CreateUserRequest): Promise<User> {
    const response = await api.post<User>('/api/v1/users', userData)
    if (response.code === 201 && response.data) return response.data
    throw new Error(response.message || 'Erro ao criar usuário')
  },

  async update(id: number, userData: UpdateUserRequest): Promise<User> {
    const response = await api.put<User>(`/api/v1/users/${id}`, userData)
    if (response.code === 200 && response.data) return response.data
    throw new Error(response.message || 'Erro ao atualizar usuário')
  },

  async delete(id: number): Promise<void> {
    const response = await api.delete<void>(`/api/v1/users/${id}`)
    if (response.code !== 200 && response.code !== 204) {
      throw new Error(response.message || 'Erro ao deletar usuário')
    }
  },

  async restore(id: number): Promise<void> {
    const response = await api.patch<void>(`/api/v1/users/${id}/restore`)
    if (response.code !== 200) {
      throw new Error(response.message || 'Erro ao restaurar usuário')
    }
  },

  async getPermissions(): Promise<Permission[]> {
    try {
      const response = await api.get<UserOptionsResponse>('/api/v1/users/options')
      if (response.code === 200 && response.data?.permissions) {
        return response.data.permissions
      }
    } catch {
      // Falhou — retorna vazio
    }
    return []
  },

  async resetPassword(id: number): Promise<void> {
    const response = await api.post<void>(`/api/v1/users/${id}/reset-password`)
    if (response.code !== 200 && response.code !== 204) {
      throw new Error(response.message || 'Erro ao resetar senha')
    }
  },
}
