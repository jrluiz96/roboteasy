<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

function goHome() {
  router.push('/')
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 px-4">
    <div class="text-center">
      <h1 class="text-9xl font-extrabold text-gray-200 dark:text-gray-700">404</h1>
      <p class="text-2xl font-semibold text-gray-900 dark:text-white mt-4">
        Página não encontrada
      </p>
      <p class="text-gray-600 dark:text-gray-400 mt-2">
        A página que você está procurando não existe ou foi movida.
      </p>
      <button
        @click="goHome"
        class="mt-8 px-6 py-3 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition"
      >
        Voltar para Home
      </button>
    </div>
  </div>
</template>
