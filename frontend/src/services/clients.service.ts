import { api } from './api'

export interface ClientItem {
  id: number
  name: string
  email: string | null
  phone: string | null
  cpf: string | null
  conversationCount: number
  messageCount: number
  lastConversationAt: string | null
  isOnline: boolean
  createdAt: string
}

class ClientsService {
  async getAll(): Promise<ClientItem[]> {
    const res = await api.get<ClientItem[]>('/api/v1/clients')
    return res.data ?? []
  }
}

export const clientsService = new ClientsService()
