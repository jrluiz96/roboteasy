import { api } from './api'

export interface Session {
  id: number
  name: string
  username: string
  email: string | null
  avatarUrl: string | null
  gitHubLogin: string | null
  permissionId: number
  sessionAt: string | null
}

export const sessionService = {
  async get(): Promise<Session> {
    const response = await api.get<Session>('/api/v1/session')
    return response.data
  }
}
