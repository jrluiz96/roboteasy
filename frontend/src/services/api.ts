const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8080'

export interface ApiResponse<T> {
  code: number
  message: string
  data: T
}

export interface ApiError {
  code: number
  message: string
  data: null
}

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'

class ApiClient {
  private baseUrl: string
  private token: string | null = null

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl
    this.token = localStorage.getItem('token')
  }

  setToken(token: string | null): void {
    this.token = token
    if (token) {
      localStorage.setItem('token', token)
    } else {
      localStorage.removeItem('token')
    }
  }

  getToken(): string | null {
    return this.token
  }

  private async request<T>(
    method: HttpMethod,
    endpoint: string,
    body?: unknown
  ): Promise<ApiResponse<T>> {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    }

    if (this.token) {
      headers['Authorization'] = `Bearer ${this.token}`
    }

    const config: RequestInit = {
      method,
      headers,
    }

    if (body && method !== 'GET') {
      config.body = JSON.stringify(body)
    }

    const response = await fetch(`${this.baseUrl}${endpoint}`, config)
    const data = await response.json()

    if (!response.ok) {
      throw data as ApiError
    }

    return data as ApiResponse<T>
  }

  async get<T>(endpoint: string): Promise<ApiResponse<T>> {
    return this.request<T>('GET', endpoint)
  }

  async post<T>(endpoint: string, body?: unknown): Promise<ApiResponse<T>> {
    return this.request<T>('POST', endpoint, body)
  }

  async put<T>(endpoint: string, body?: unknown): Promise<ApiResponse<T>> {
    return this.request<T>('PUT', endpoint, body)
  }

  async patch<T>(endpoint: string, body?: unknown): Promise<ApiResponse<T>> {
    return this.request<T>('PATCH', endpoint, body)
  }

  async delete<T>(endpoint: string): Promise<ApiResponse<T>> {
    return this.request<T>('DELETE', endpoint)
  }
}

export const api = new ApiClient(API_BASE_URL)
