import { api } from './api'

export interface User {
  id: number
  name: string
  username: string
  email: string | null
  avatarUrl: string | null
  permissionId: number
  createdAt: string
  updatedAt: string | null
}

export interface CreateUserRequest {
  name: string
  username: string
  password: string
  email?: string
  permissionId: number
}

export interface UpdateUserRequest {
  name?: string
  email?: string
  password?: string
  permissionId?: number
}

export const usersService = {
  async getAll(): Promise<User[]> {
    const response = await api.get<User[]>('/api/v1/users')
    return response.data
  },

  async getById(id: number): Promise<User> {
    const response = await api.get<User>(`/api/v1/users/${id}`)
    return response.data
  },

  async create(data: CreateUserRequest): Promise<User> {
    const response = await api.post<User>('/api/v1/users', data)
    return response.data
  },

  async update(id: number, data: UpdateUserRequest): Promise<User> {
    const response = await api.put<User>(`/api/v1/users/${id}`, data)
    return response.data
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/api/v1/users/${id}`)
  }
}
