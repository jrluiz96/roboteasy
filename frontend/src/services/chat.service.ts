import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'
import { api } from './api'

// Em dev o Vite proxia /hubs → localhost:8080 (evita CORS).
// Em prod, defina VITE_HUB_URL com a URL completa do hub.
const HUB_URL = import.meta.env.VITE_HUB_URL || '/hubs/chat'

// ── Event name constants (must match backend ChatEvents.cs) ──────────────────

export const ChatEvents = {
  UserOnline:     'UserOnline',
  UserOffline:    'UserOffline',
  ReceiveMessage: 'ReceiveMessage',
  Typing:         'Typing',
  StopTyping:     'StopTyping',
  MessageRead:    'MessageRead',
} as const

export type ChatEvent = (typeof ChatEvents)[keyof typeof ChatEvents]

// ── Types ────────────────────────────────────────────────────────────────────

export interface ChatMessage {
  id: number
  conversationId: number
  userId: number | null
  clientId: number | null
  type: string
  content: string
  createdAt: string
}

export interface TypingPayload {
  conversationId: number
  userId: number | null
  clientId: number | null
}

export interface MessageReadPayload {
  conversationId: number
  userId: number | null
  clientId: number | null
  lastMessageId: number
}

export interface UserStatusPayload {
  userId: number
  connectionId?: string
}

// ── REST ─────────────────────────────────────────────────────────────────────

export interface ChatStartRequest {
  name: string
  email?: string
  phone?: string
  cpf?: string
}

export interface ChatStartResponse {
  clientId: number
  clientToken: string
  conversationId: number
  isNewConversation: boolean
  messages: ChatMessage[]
}

// ── ChatService ───────────────────────────────────────────────────────────────

class ChatService {
  private connection: HubConnection | null = null

  // ── REST ───────────────────────────────────────────────────────────────────

  async start(data: ChatStartRequest): Promise<ChatStartResponse> {
    const response = await api.post<ChatStartResponse>('/api/v1/open/chat/start', data)
    if (response.code === 200 && response.data) return response.data
    throw new Error(response.message || 'Erro ao iniciar atendimento')
  }

  // ── Connection ─────────────────────────────────────────────────────────────

  /**
   * Conecta como atendente (usuário autenticado via JWT).
   * @param accessToken  JWT obtido no login
   */
  async connectAsUser(accessToken: string): Promise<void> {
    const url = `${HUB_URL}?access_token=${encodeURIComponent(accessToken)}`
    await this._connect(url)
  }

  /**
   * Conecta como cliente externo (token gerado em POST /api/v1/open/chat/start).
   * @param clientToken  Token opaco gerado pelo backend
   */
  async connectAsClient(clientToken: string): Promise<void> {
    const url = `${HUB_URL}?client_token=${encodeURIComponent(clientToken)}`
    await this._connect(url)
  }

  async disconnect(): Promise<void> {
    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) {
      await this.connection.stop()
    }
    this.connection = null
  }

  get isConnected(): boolean {
    return this.connection?.state === HubConnectionState.Connected
  }

  // ── Conversation groups ────────────────────────────────────────────────────

  async joinConversation(conversationId: number): Promise<void> {
    await this._invoke('JoinConversation', conversationId)
  }

  async leaveConversation(conversationId: number): Promise<void> {
    await this._invoke('LeaveConversation', conversationId)
  }

  // ── Messaging ─────────────────────────────────────────────────────────────

  async sendMessage(conversationId: number, content: string): Promise<void> {
    await this._invoke('SendMessage', conversationId, content)
  }

  // ── Typing ────────────────────────────────────────────────────────────────

  async typing(conversationId: number): Promise<void> {
    await this._invoke('Typing', conversationId)
  }

  async stopTyping(conversationId: number): Promise<void> {
    await this._invoke('StopTyping', conversationId)
  }

  // ── Read receipts ─────────────────────────────────────────────────────────

  async markAsRead(conversationId: number, lastMessageId: number): Promise<void> {
    await this._invoke('MarkAsRead', conversationId, lastMessageId)
  }

  // ── Event listeners ───────────────────────────────────────────────────────

  onMessage(handler: (msg: ChatMessage) => void): void {
    this._on(ChatEvents.ReceiveMessage, handler)
  }

  onTyping(handler: (payload: TypingPayload) => void): void {
    this._on(ChatEvents.Typing, handler)
  }

  onStopTyping(handler: (payload: TypingPayload) => void): void {
    this._on(ChatEvents.StopTyping, handler)
  }

  onMessageRead(handler: (payload: MessageReadPayload) => void): void {
    this._on(ChatEvents.MessageRead, handler)
  }

  onUserOnline(handler: (payload: UserStatusPayload) => void): void {
    this._on(ChatEvents.UserOnline, handler)
  }

  onUserOffline(handler: (payload: UserStatusPayload) => void): void {
    this._on(ChatEvents.UserOffline, handler)
  }

  /** Remove all listeners for an event (useful on component unmount). */
  off(event: ChatEvent): void {
    this.connection?.off(event)
  }

  // ── Private helpers ───────────────────────────────────────────────────────

  private async _connect(url: string): Promise<void> {
    if (this.connection) {
      await this.disconnect()
    }

    this.connection = new HubConnectionBuilder()
      .withUrl(url)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    await this.connection.start()
  }

  private async _invoke(method: string, ...args: unknown[]): Promise<void> {
    if (!this.connection || this.connection.state !== HubConnectionState.Connected) {
      console.warn(`[ChatService] invoke('${method}') called while not connected.`)
      return
    }
    await this.connection.invoke(method, ...args)
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private _on(event: string, handler: (...args: any[]) => void): void {
    if (!this.connection) {
      console.warn(`[ChatService] on('${event}') called before connect().`)
      return
    }
    this.connection.on(event, handler)
  }
}

export const chatService = new ChatService()
