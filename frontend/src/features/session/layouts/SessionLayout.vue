<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/features/auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const sidebarCollapsed = ref(false)

const menuItems = [
  { name: 'Dashboard', path: '/session', icon: '📊' },
  { name: 'Robôs', path: '/session/robots', icon: '🤖' },
  { name: 'Clientes', path: '/session/clients', icon: '👥' },
  { name: 'Relatórios', path: '/session/reports', icon: '📈' },
  { name: 'Configurações', path: '/session/settings', icon: '⚙️' },
]

const currentPath = computed(() => route.path)

function isActive(path: string): boolean {
  if (path === '/session') {
    return currentPath.value === '/session'
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

function toggleSidebar() {
  sidebarCollapsed.value = !sidebarCollapsed.value
}
</script>

<template>
  <div class="min-h-screen bg-gray-100 dark:bg-gray-900 flex">
    <!-- Sidebar -->
    <aside
      :class="[
        'fixed inset-y-0 left-0 z-50 flex flex-col bg-gray-900 text-white transition-all duration-300',
        sidebarCollapsed ? 'w-16' : 'w-64'
      ]"
    >
      <!-- Logo -->
      <div class="h-16 flex items-center justify-between px-4 border-b border-gray-800">
        <div class="flex items-center gap-2">
          <div class="w-8 h-8 bg-gradient-to-br from-purple-500 to-pink-500 rounded-lg flex-shrink-0"></div>
          <span v-if="!sidebarCollapsed" class="text-lg font-bold">RobotEasy</span>
        </div>
        <button
          @click="toggleSidebar"
          class="p-1 hover:bg-gray-800 rounded transition"
        >
          <span v-if="sidebarCollapsed">→</span>
          <span v-else>←</span>
        </button>
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
              <span class="text-xl flex-shrink-0">{{ item.icon }}</span>
              <span v-if="!sidebarCollapsed" class="text-sm font-medium">{{ item.name }}</span>
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
          <div v-if="!sidebarCollapsed" class="flex-1 min-w-0">
            <p class="text-sm font-medium truncate">{{ authStore.currentUser?.username || 'Usuário' }}</p>
            <button @click="handleLogout" class="text-xs text-gray-400 hover:text-red-400 transition">
              Sair
            </button>
          </div>
        </div>
      </div>
    </aside>

    <!-- Main Content -->
    <div
      :class="[
        'flex-1 flex flex-col transition-all duration-300',
        sidebarCollapsed ? 'ml-16' : 'ml-64'
      ]"
    >
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
