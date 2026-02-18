import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authApi } from '../services/authApi'
import { api } from '@/services/api'
import { useToastStore } from '@/stores/toastStore'
import type { User, LoginCredentials } from '../types'

export const useAuthStore = defineStore('auth', () => {
  const toastStore = useToastStore()
  
  // State
  const user = ref<User | null>(null)
  const token = ref<string | null>(api.getToken())
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const isAuthenticated = computed(() => !!token.value)
  const currentUser = computed(() => user.value)

  // Actions
  async function login(credentials: LoginCredentials) {
    loading.value = true
    error.value = null
    
    try {
      const response = await authApi.login(credentials)
      token.value = response.token
      user.value = response.user
      api.setToken(response.token)
      toastStore.success('Login realizado com sucesso!')
      return true
    } catch (e: unknown) {
      const err = e as { message?: string }
      error.value = err.message || 'Erro ao fazer login'
      return false
    } finally {
      loading.value = false
    }
  }

  async function loginWithGitHub() {
    window.location.href = '/api/v1/open/github/login'
  }

  function logout() {
    logoutWithMessage()
  }

  async function checkAuth(): Promise<{ valid: boolean; expired?: boolean }> {
    if (!token.value) return { valid: false }
    
    try {
      const response = await authApi.me()
      user.value = response.user
      return { valid: true }
    } catch (e: unknown) {
      const err = e as { code?: number }
      const isExpired = err.code === 401
      clearSession()
      return { valid: false, expired: isExpired }
    }
  }

  function clearSession() {
    user.value = null
    token.value = null
    authApi.logout()
  }

  function logoutWithMessage() {
    clearSession()
    toastStore.info('Você saiu do sistema')
  }

  return {
    // State
    user,
    token,
    loading,
    error,
    // Getters
    isAuthenticated,
    currentUser,
    // Actions
    login,
    loginWithGitHub,
    logout,
    checkAuth
  }
})
