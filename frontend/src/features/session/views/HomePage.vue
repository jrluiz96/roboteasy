<script setup lang="ts">
import { useAuthStore } from '@/features/auth'

const authStore = useAuthStore()

const updates = [
  { version: '1.0.0', date: '20/02/2026', items: [
    'Sistema de Tutorial para novos operadores',
    'Monitoramento de conversas em tempo real',
    'Chat de atendimento ao cliente via widget',
    'Conferência entre operadores na mesma conversa',
    'Histórico completo de conversas finalizadas',
  ]},
]

const roadmap = [
  { label: 'Implementar anexos nas conversas', icon: 'fa-paperclip' },
  { label: 'Implementar área de atualização dos dados pessoais dos operadores', icon: 'fa-user-edit' },
  { label: 'Implementar temas', icon: 'fa-palette' },
  { label: 'Implementar Login OAuth', icon: 'fa-key' },
  { label: 'Reset de senha na tela de login enviando link pro e-mail', icon: 'fa-envelope' },
]
</script>

<template>
  <div class="space-y-8 max-w-5xl mx-auto">

    <!-- Welcome -->
    <div class="flex items-center gap-4">
      <div class="w-14 h-14 bg-gray-800 rounded-full flex items-center justify-center flex-shrink-0">
        <span class="text-2xl font-bold text-white">
          {{ authStore.currentUser?.username?.charAt(0)?.toUpperCase() || '?' }}
        </span>
      </div>
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
          Bem-vindo, {{ authStore.currentUser?.name || authStore.currentUser?.username || 'Usuário' }}!
        </h1>
        <p class="text-gray-500 dark:text-gray-400 text-sm">
          Bom ter você de volta ao MeetConnect.
        </p>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">

      <!-- Notas de Atualização -->
      <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
        <div class="px-5 py-4 border-b border-gray-200 dark:border-gray-700 flex items-center gap-2">
          <i class="fas fa-bullhorn text-purple-500"></i>
          <h2 class="text-base font-semibold text-gray-900 dark:text-white">Notas de Atualização</h2>
        </div>
        <div class="p-5 space-y-5 max-h-[400px] overflow-y-auto">
          <div v-for="update in updates" :key="update.version">
            <div class="flex items-center gap-2 mb-2">
              <span class="text-xs font-bold text-purple-600 dark:text-purple-400 bg-purple-100 dark:bg-purple-900/30 px-2 py-0.5 rounded-full">
                v{{ update.version }}
              </span>
              <span class="text-xs text-gray-400">{{ update.date }}</span>
            </div>
            <ul class="space-y-1.5">
              <li v-for="item in update.items" :key="item" class="flex items-start gap-2 text-sm text-gray-600 dark:text-gray-300">
                <i class="fas fa-check text-green-500 mt-0.5 text-xs"></i>
                {{ item }}
              </li>
            </ul>
          </div>
        </div>
      </div>

      <!-- Melhorias Planejadas -->
      <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">
        <div class="px-5 py-4 border-b border-gray-200 dark:border-gray-700 flex items-center gap-2">
          <i class="fas fa-rocket text-blue-500"></i>
          <h2 class="text-base font-semibold text-gray-900 dark:text-white">Melhorias Planejadas</h2>
        </div>
        <div class="p-5 max-h-[400px] overflow-y-auto">
          <ul class="space-y-3">
            <li v-for="(item, idx) in roadmap" :key="idx" class="flex items-start gap-3 p-3 rounded-lg bg-gray-50 dark:bg-gray-700/30">
              <div class="w-8 h-8 rounded-lg bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center flex-shrink-0">
                <i :class="['fas text-blue-500 text-sm', item.icon]"></i>
              </div>
              <span class="text-sm text-gray-700 dark:text-gray-300 pt-1">{{ item.label }}</span>
            </li>
          </ul>
        </div>
      </div>

    </div>
  </div>
</template>