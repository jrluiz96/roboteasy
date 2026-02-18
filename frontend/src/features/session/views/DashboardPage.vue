<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/features/auth'

const authStore = useAuthStore()
const greeting = ref('')

onMounted(() => {
  const hour = new Date().getHours()
  if (hour < 12) greeting.value = 'Bom dia'
  else if (hour < 18) greeting.value = 'Boa tarde'
  else greeting.value = 'Boa noite'
})

const stats = [
  { label: 'Robôs Ativos', value: '12', icon: '🤖', color: 'bg-purple-500' },
  { label: 'Clientes', value: '48', icon: '👥', color: 'bg-blue-500' },
  { label: 'Execuções Hoje', value: '156', icon: '⚡', color: 'bg-green-500' },
  { label: 'Erros', value: '3', icon: '⚠️', color: 'bg-red-500' },
]

const recentActivities = [
  { id: 1, action: 'Robô "Extrator" executado', time: 'há 5 minutos', status: 'success' },
  { id: 2, action: 'Novo cliente cadastrado', time: 'há 1 hora', status: 'info' },
  { id: 3, action: 'Erro no robô "Validador"', time: 'há 2 horas', status: 'error' },
  { id: 4, action: 'Relatório gerado', time: 'há 3 horas', status: 'success' },
]
</script>

<template>
  <div class="space-y-6">
    <!-- Welcome -->
    <div class="bg-gradient-to-r from-purple-600 to-pink-600 rounded-2xl p-6 text-white">
      <h2 class="text-2xl font-bold">
        {{ greeting }}, {{ authStore.currentUser?.name || authStore.currentUser?.username }}! 👋
      </h2>
      <p class="mt-1 text-purple-100">
        Aqui está um resumo do seu sistema hoje.
      </p>
    </div>

    <!-- Stats Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
      <div
        v-for="stat in stats"
        :key="stat.label"
        class="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-sm"
      >
        <div class="flex items-center justify-between">
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">{{ stat.label }}</p>
            <p class="text-3xl font-bold text-gray-900 dark:text-white mt-1">{{ stat.value }}</p>
          </div>
          <div :class="[stat.color, 'w-12 h-12 rounded-xl flex items-center justify-center']">
            <span class="text-2xl">{{ stat.icon }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Content Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Recent Activity -->
      <div class="bg-white dark:bg-gray-800 rounded-xl shadow-sm">
        <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h3 class="font-semibold text-gray-900 dark:text-white">Atividade Recente</h3>
        </div>
        <ul class="divide-y divide-gray-200 dark:divide-gray-700">
          <li
            v-for="activity in recentActivities"
            :key="activity.id"
            class="px-6 py-4 flex items-center justify-between"
          >
            <div class="flex items-center gap-3">
              <div
                :class="[
                  'w-2 h-2 rounded-full',
                  activity.status === 'success' ? 'bg-green-500' :
                  activity.status === 'error' ? 'bg-red-500' : 'bg-blue-500'
                ]"
              ></div>
              <span class="text-sm text-gray-700 dark:text-gray-300">{{ activity.action }}</span>
            </div>
            <span class="text-xs text-gray-500 dark:text-gray-400">{{ activity.time }}</span>
          </li>
        </ul>
      </div>

      <!-- Quick Actions -->
      <div class="bg-white dark:bg-gray-800 rounded-xl shadow-sm">
        <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h3 class="font-semibold text-gray-900 dark:text-white">Ações Rápidas</h3>
        </div>
        <div class="p-6 grid grid-cols-2 gap-4">
          <button class="px-4 py-3 bg-purple-50 dark:bg-purple-900/20 text-purple-600 dark:text-purple-400 rounded-xl hover:bg-purple-100 dark:hover:bg-purple-900/30 transition text-sm font-medium">
            + Novo Robô
          </button>
          <button class="px-4 py-3 bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400 rounded-xl hover:bg-blue-100 dark:hover:bg-blue-900/30 transition text-sm font-medium">
            + Novo Cliente
          </button>
          <button class="px-4 py-3 bg-green-50 dark:bg-green-900/20 text-green-600 dark:text-green-400 rounded-xl hover:bg-green-100 dark:hover:bg-green-900/30 transition text-sm font-medium">
            📊 Gerar Relatório
          </button>
          <button class="px-4 py-3 bg-gray-50 dark:bg-gray-700 text-gray-600 dark:text-gray-400 rounded-xl hover:bg-gray-100 dark:hover:bg-gray-600 transition text-sm font-medium">
            ⚙️ Configurações
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
