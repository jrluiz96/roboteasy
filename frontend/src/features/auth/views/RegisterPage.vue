<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import { useToastStore } from '@/stores/toastStore'
import { authService } from '@/services/auth.service'

const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()

const name = ref('')
const username = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const loading = ref(false)
const error = ref<string | null>(null)

async function handleRegister() {
  error.value = null

  if (!name.value.trim() || !username.value.trim() || !email.value.trim() || !password.value) {
    error.value = 'Preencha todos os campos'
    return
  }

  if (password.value.length < 4) {
    error.value = 'A senha deve ter no mínimo 4 caracteres'
    return
  }

  if (password.value !== confirmPassword.value) {
    error.value = 'As senhas não coincidem'
    return
  }

  loading.value = true
  try {
    const result = await authService.register({
      name: name.value.trim(),
      username: username.value.trim(),
      email: email.value.trim(),
      password: password.value,
    })

    // Salva token e carrega sessão
    localStorage.setItem('token', result.token)
    await authStore.checkAuth()

    toastStore.success('Cadastro realizado com sucesso!')
    const views = authStore.user?.views || []
    const hasHome = views.some(v => v.route === '/session/home')
    router.push(hasHome ? '/session/home' : (views[0]?.route || '/session/home'))
  } catch (err: any) {
    error.value = err?.response?.data?.message || 'Erro ao cadastrar. Tente novamente.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 px-4">
    <div class="max-w-md w-full space-y-8">
      <!-- Logo/Header -->
      <div class="flex flex-col items-center">
        <img src="/logo-sistema.png" alt="MeetConnect" class="h-12 w-auto object-contain mb-2" />
        <p class="text-gray-600 dark:text-gray-400">
          Crie sua conta para começar
        </p>
      </div>

      <!-- Register Form -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-8">
        <form @submit.prevent="handleRegister" class="space-y-5">
          <!-- Error Alert -->
          <div
            v-if="error"
            class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 text-red-700 dark:text-red-400 px-4 py-3 rounded-lg text-sm"
          >
            {{ error }}
          </div>

          <!-- Name -->
          <div>
            <label for="name" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Nome completo
            </label>
            <input
              id="name"
              v-model="name"
              type="text"
              required
              class="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 focus:ring-2 focus:ring-purple-500 focus:border-transparent transition"
              placeholder="Seu nome"
            />
          </div>

          <!-- Username -->
          <div>
            <label for="username" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Usuário
            </label>
            <input
              id="username"
              v-model="username"
              type="text"
              required
              class="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 focus:ring-2 focus:ring-purple-500 focus:border-transparent transition"
              placeholder="Escolha um nome de usuário"
            />
          </div>

          <!-- Email -->
          <div>
            <label for="email" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              E-mail
            </label>
            <input
              id="email"
              v-model="email"
              type="email"
              required
              class="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 focus:ring-2 focus:ring-purple-500 focus:border-transparent transition"
              placeholder="seu@email.com"
            />
          </div>

          <!-- Password -->
          <div>
            <label for="password" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Senha
            </label>
            <input
              id="password"
              v-model="password"
              type="password"
              required
              class="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 focus:ring-2 focus:ring-purple-500 focus:border-transparent transition"
              placeholder="Mínimo 4 caracteres"
            />
          </div>

          <!-- Confirm Password -->
          <div>
            <label for="confirmPassword" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
              Confirmar senha
            </label>
            <input
              id="confirmPassword"
              v-model="confirmPassword"
              type="password"
              required
              class="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 focus:ring-2 focus:ring-purple-500 focus:border-transparent transition"
              placeholder="Repita a senha"
            />
          </div>

          <!-- Submit Button -->
          <button
            type="submit"
            :disabled="loading"
            class="w-full flex justify-center py-3 px-4 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-purple-600 hover:bg-purple-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500 disabled:opacity-50 disabled:cursor-not-allowed transition"
          >
            <svg v-if="loading" class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            {{ loading ? 'Cadastrando...' : 'Criar conta' }}
          </button>
        </form>
      </div>

      <!-- Login Link -->
      <p class="text-center text-sm text-gray-500 dark:text-gray-400">
        Já tem uma conta?
        <router-link to="/login" class="text-purple-600 dark:text-purple-400 hover:underline font-medium">Entrar</router-link>
      </p>
    </div>
  </div>
</template>
