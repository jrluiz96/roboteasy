import { api, type ApiResponse } from './api'

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

export interface UserOptionsResponse {
  permissions: Permission[]
}

export const usersService = {
  // GET ALL - Busca todos os usuários
  async getAll(): Promise<User[]> {
    try {
      console.log('🔄 Buscando todos os usuários...')
      const response = await api.get<User[]>('/api/v1/users')
      console.log('📦 Resposta da API de usuários:', response)
      
      if (response.code === 200 && response.data) {
        console.log('✅ Usuários diretos da API:', response.data)
        return response.data
      }
      
      console.log('⚠️ Resposta inesperada da API:', response)
      return []
    } catch (error) {
      console.error('❌ Erro ao buscar usuários:', error)
      throw error
    }
  },

  // CREATE - Criar novo usuário
  async create(userData: CreateUserRequest): Promise<User> {
    try {
      console.log('🔄 Criando usuário:', userData)
      const response = await api.post<User>('/api/v1/users', userData)
      console.log('📦 Usuário criado:', response)
      
      if (response.code === 201 && response.data) {
        return response.data
      }
      
      throw new Error(response.message || 'Erro ao criar usuário')
    } catch (error) {
      console.error('❌ Erro ao criar usuário:', error)
      throw error
    }
  },

  // UPDATE - Atualizar usuário existente
  async update(id: number, userData: UpdateUserRequest): Promise<User> {
    try {
      console.log(`🔄 Atualizando usuário ${id}:`, userData)
      const response = await api.put<User>(`/api/v1/users/${id}`, userData)
      console.log('📦 Usuário atualizado:', response)
      
      if (response.code === 200 && response.data) {
        return response.data
      }
      
      throw new Error(response.message || 'Erro ao atualizar usuário')
    } catch (error) {
      console.error('❌ Erro ao atualizar usuário:', error)
      throw error
    }
  },

  // DELETE - Soft delete do usuário
  async delete(id: number): Promise<void> {
    try {
      console.log(`🔄 Deletando usuário ${id}...`)
      const response = await api.delete<void>(`/api/v1/users/${id}`)
      console.log('📦 Usuário deletado:', response)
      
      if (response.code !== 200 && response.code !== 204) {
        throw new Error(response.message || 'Erro ao deletar usuário')
      }
    } catch (error) {
      console.error('❌ Erro ao deletar usuário:', error)  
      throw error
    }
  },

  // RESTORE - Restaurar usuário deletado
  async restore(id: number): Promise<void> {
    try {
      console.log(`🔄 Restaurando usuário ${id}...`)
      const response = await api.patch<void>(`/api/v1/users/${id}/restore`)
      console.log('📦 Usuário restaurado:', response)

      if (response.code !== 200) {
        throw new Error(response.message || 'Erro ao restaurar usuário')
      }
    } catch (error) {
      console.error('❌ Erro ao restaurar usuário:', error)
      throw error
    }
  },

  // OPTIONS - Busca permissões disponíveis do endpoint específico
  async getPermissions(): Promise<Permission[]> {
    try {
      console.log('🔄 Buscando permissões...')
      const response = await api.get<UserOptionsResponse>('/api/v1/users/options')
      console.log('📦 Resposta da API de permissões:', response)
      
      if (response.code === 200 && response.data?.permissions) {
        console.log('✅ Permissões diretas da API:', response.data.permissions)
        return response.data.permissions
      }
    } catch (error) {
      console.error('Erro ao buscar permissões:', error)
    }
    
    // Sem fallback - se falhou, retorna vazio
    console.log('❌ Retornando array vazio de permissões')
    return []
  },

  // RESET PASSWORD - Resetar senha do usuário  
  async resetPassword(id: number): Promise<void> {
    try {
      console.log(`🔄 Resetando senha do usuário ${id}...`)
      const response = await api.post<void>(`/api/v1/users/${id}/reset-password`)
      console.log('📦 Senha resetada:', response)
      
      if (response.code !== 200 && response.code !== 204) {
        throw new Error(response.message || 'Erro ao resetar senha')
      }
    } catch (error) {
      console.error('❌ Erro ao resetar senha:', error)
      throw error
    }
  }
}
