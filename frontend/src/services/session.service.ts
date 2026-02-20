import { api } from './api'

export interface View {
  id: number
  name: string
  route: string
  icon: string
}

export interface Session {
  id: number
  name: string
  username: string
  email: string | null
  avatarUrl: string | null
  permissionId: number
  sessionAt: string | null
  views: View[]
}

export const sessionService = {
  async get(): Promise<Session> {
    const response = await api.get<Session>('/api/v1/session')
    return response.data
  },

  async finishTutorial(): Promise<void> {
    await api.post('/api/v1/session/finish-tutorial', {})
  }
}
