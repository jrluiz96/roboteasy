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
    router.push('/session/home')
  }
}

function handleGitHubLogin() {
  authStore.loginWithGitHub()
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 px-4">
    <div class="max-w-md w-full space-y-8">
      <!-- Logo/Header -->
      <div class="text-center">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
          RobotEasy
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Faça login para continuar
        </p>
      </div>

      <!-- Login Form -->
      <LoginForm 
        :loading="loading"
        :error="authStore.error"
        @submit="handleLogin"
        @github-login="handleGitHubLogin"
      />
    </div>
  </div>
</template>
