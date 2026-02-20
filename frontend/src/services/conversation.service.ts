import { api } from './api'

// ── Types ─────────────────────────────────────────────────────────────────────

export interface ConversationAttendant {
  userId: number
  name: string
  avatarUrl: string | null
}

export interface ConversationMessage {
  id: number
  conversationId: number
  userId: number | null
  clientId: number | null
  type: number
  content: string
  senderName: string | null
  fileUrl: string | null
  createdAt: string
}

export interface ConversationListItem {
  id: number
  clientId: number
  clientName: string
  clientEmail: string | null
  lastMessage: string | null
  lastMessageAt: string | null
  messageCount: number
  createdAt: string
  finishedAt: string | null
  status: 'waiting' | 'active' | 'finished'
  attendants: ConversationAttendant[]
}

export interface ConversationDetail {
  id: number
  clientId: number
  clientName: string
  clientEmail: string | null
  clientPhone: string | null
  createdAt: string
  finishedAt: string | null
  attendanceTime: number | null
  status: 'waiting' | 'active' | 'finished'
  messages: ConversationMessage[]
  attendants: ConversationAttendant[]
}

// ── Service ───────────────────────────────────────────────────────────────────

export const conversationService = {
  async getActive(): Promise<ConversationListItem[]> {
    const res = await api.get<ConversationListItem[]>('/api/v1/conversations')
    return res.data ?? []
  },

  async getHistory(): Promise<ConversationListItem[]> {
    const res = await api.get<ConversationListItem[]>('/api/v1/conversations/history')
    return res.data ?? []
  },

  async getById(id: number): Promise<ConversationDetail | null> {
    const res = await api.get<ConversationDetail>(`/api/v1/conversations/${id}`)
    return res.data ?? null
  },

  async finish(id: number): Promise<void> {
    await api.post(`/api/v1/conversations/${id}/finish`)
  },
  async join(id: number): Promise<void> {
    await api.post(`/api/v1/conversations/${id}/join`, {})
  },

  async leave(id: number): Promise<void> {
    await api.post(`/api/v1/conversations/${id}/leave`, {})
  },

  async invite(id: number, attendantId: number): Promise<void> {
    await api.post(`/api/v1/conversations/${id}/invite/${attendantId}`, {})
  },}
