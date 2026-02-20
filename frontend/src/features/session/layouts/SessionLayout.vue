<script setup lang="ts">
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/features/auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

// Menu items dinâmicos vindos da API via authStore
const menuItems = computed(() => {
  return authStore.currentUser?.views?.map(view => ({
    id: view.id,
    name: getViewDisplayName(view.name),
    path: view.route,
    icon: view.icon
  })) || []
})

function getViewDisplayName(viewName: string): string {
  const displayNames: { [key: string]: string } = {
    'home': 'Início',
    'customer_service': 'Atendimento',
    'clients': 'Clientes',
    'history': 'Histórico',
    'users': 'Usuários',
    'monitoring': 'Monitoramento',
    'tutorial': 'Tutorial'
  }
  return displayNames[viewName] || viewName
}

const currentPath = computed(() => route.path)

function isActive(path: string): boolean {
  if (path === '/session/home') {
    return currentPath.value === '/session/home' || currentPath.value === '/session'
  }
  return currentPath.value.startsWith(path)
}

function navigate(path: string) {
  router.push(path)
}

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen bg-gray-100 dark:bg-gray-900 flex">
    <!-- Sidebar -->
    <aside class="fixed inset-y-0 left-0 z-50 flex flex-col w-64 bg-gray-900 text-white border-r border-gray-800">
      <!-- Logo -->
      <div class="h-16 flex items-center px-4 border-b border-gray-800">
        <img src="/logo-sistema.png" alt="MeetConnect" class="h-8 w-auto object-contain" />
      </div>

      <!-- Menu -->
      <nav class="flex-1 py-4 overflow-y-auto">
        <ul class="space-y-1 px-2">
          <li v-for="item in menuItems" :key="item.path">
            <button
              @click="navigate(item.path)"
              :class="[
                'w-full flex items-center gap-3 px-3 py-2.5 rounded-lg transition',
                isActive(item.path)
                  ? 'bg-purple-600 text-white'
                  : 'text-gray-400 hover:bg-gray-800 hover:text-white'
              ]"
            >
              <i :class="['text-lg flex-shrink-0', `fas ${item.icon}`]"></i>
              <span class="text-sm font-medium">{{ item.name }}</span>
            </button>
          </li>
        </ul>
      </nav>

      <!-- User -->
      <div class="p-4 border-t border-gray-800">
        <div class="flex items-center gap-3">
          <div class="w-8 h-8 bg-gray-700 rounded-full flex items-center justify-center flex-shrink-0">
            <span class="text-sm">{{ authStore.currentUser?.username?.charAt(0)?.toUpperCase() || '?' }}</span>
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium truncate">{{ authStore.currentUser?.username || 'Usuário' }}</p>
            <button @click="handleLogout" class="text-xs text-gray-400 hover:text-red-400 transition">
              Sair
            </button>
          </div>
        </div>
      </div>
    </aside>

    <!-- Main Content -->
    <div class="flex-1 flex flex-col ml-64">
      <!-- Header -->
      <header class="h-16 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between px-6">
        <h1 class="text-lg font-semibold text-gray-900 dark:text-white">
          {{ menuItems.find(i => isActive(i.path))?.name || 'Sistema' }}
        </h1>
        <div class="flex items-center gap-4">
          <span class="text-sm text-gray-500 dark:text-gray-400">
            {{ authStore.currentUser?.name || authStore.currentUser?.username }}
          </span>
        </div>
      </header>

      <!-- Page Content -->
      <main class="flex-1 p-6 overflow-auto">
        <RouterView />
      </main>
    </div>
  </div>
</template>
