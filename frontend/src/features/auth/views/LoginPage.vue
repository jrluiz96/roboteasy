<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/authStore'
import LoginForm from '../components/LoginForm.vue'

const router = useRouter()
const authStore = useAuthStore()

const loading = ref(false)

async function handleLogin(credentials: { username: string; password: string }) {
  loading.value = true
  const success = await authStore.login(credentials)
  loading.value = false
  
  if (success) {
    // Carrega o usuário completo (com views) antes de redirecionar
    await authStore.checkAuth()
    const views = authStore.user?.views || []
    const hasHome = views.some(v => v.route === '/session/home')
    router.push(hasHome ? '/session/home' : (views[0]?.route || '/session/home'))
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
          Faça login para continuar
        </p>
      </div>

      <!-- Login Form -->
      <LoginForm 
        :loading="loading"
        :error="authStore.error"
        @submit="handleLogin"
      />

      <!-- Register Link -->
      <p class="text-center text-sm text-gray-500 dark:text-gray-400">
        Não tem uma conta?
        <router-link to="/register" class="text-purple-600 dark:text-purple-400 hover:underline font-medium">Cadastre-se</router-link>
      </p>
    </div>
  </div>
</template>
