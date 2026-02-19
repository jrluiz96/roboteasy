<script setup lang="ts">
import { ref } from 'vue'

// Dados de exemplo para dashboard
const stats = ref([
  {
    title: 'Atendimentos Hoje',
    value: '24',
    icon: 'fa-headset',
    color: 'from-blue-500 to-blue-600'
  },
  {
    title: 'Clientes Online',
    value: '8',
    icon: 'fa-users',
    color: 'from-green-500 to-green-600'
  },
  {
    title: 'Reuniões Agendadas',
    value: '12',
    icon: 'fa-calendar',
    color: 'from-purple-500 to-purple-600'
  },
  {
    title: 'Satisfação',
    value: '97%',
    icon: 'fa-star',
    color: 'from-yellow-500 to-yellow-600'
  }
])

const recentActivity = ref([
  { time: '14:30', description: 'Reunião com cliente ABC Corp', type: 'meeting' },
  { time: '13:15', description: 'Chat finalizado com João Silva', type: 'chat' },
  { time: '12:45', description: 'Novo cliente cadastrado: Maria Santos', type: 'client' },
  { time: '11:20', description: 'Relatório mensal gerado', type: 'report' }
])
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div>
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
        Início
      </h1>
      <p class="text-sm text-gray-600 dark:text-gray-400">
        Visão geral do seu atendimento
      </p>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      <div 
        v-for="stat in stats" 
        :key="stat.title"
        class="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-sm border border-gray-200 dark:border-gray-700"
      >
        <div class="flex items-center">
          <div :class="['w-12 h-12 rounded-lg bg-gradient-to-r flex items-center justify-center', stat.color]">
            <i :class="['fas text-white', stat.icon]"></i>
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-600 dark:text-gray-400">
              {{ stat.title }}
            </h3>
            <p class="text-2xl font-bold text-gray-900 dark:text-white">
              {{ stat.value }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Recent Activity -->
      <div class="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-sm border border-gray-200 dark:border-gray-700">
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Atividades Recentes
        </h2>
        <div class="space-y-3">
          <div 
            v-for="activity in recentActivity" 
            :key="activity.time"
            class="flex items-start gap-3 p-3 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700/50 transition"
          >
            <div class="w-2 h-2 rounded-full bg-purple-500 mt-2 flex-shrink-0"></div>
            <div class="flex-1 min-w-0">
              <p class="text-sm text-gray-900 dark:text-white">
                {{ activity.description }}
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400">
                {{ activity.time }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-sm border border-gray-200 dark:border-gray-700">
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Ações Rápidas
        </h2>
        <div class="grid grid-cols-2 gap-3">
          <button class="p-4 bg-purple-50 dark:bg-purple-900/20 rounded-lg hover:bg-purple-100 dark:hover:bg-purple-900/30 transition group">
            <i class="fas fa-video text-purple-600 dark:text-purple-400 mb-2 text-xl"></i>
            <p class="text-sm font-medium text-purple-700 dark:text-purple-300">
              Nova Reunião
            </p>
          </button>
          <button class="p-4 bg-green-50 dark:bg-green-900/20 rounded-lg hover:bg-green-100 dark:hover:bg-green-900/30 transition group">
            <i class="fas fa-comments text-green-600 dark:text-green-400 mb-2 text-xl"></i>
            <p class="text-sm font-medium text-green-700 dark:text-green-300">
              Iniciar Chat
            </p>
          </button>
          <button class="p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg hover:bg-blue-100 dark:hover:bg-blue-900/30 transition group">
            <i class="fas fa-user-plus text-blue-600 dark:text-blue-400 mb-2 text-xl"></i>
            <p class="text-sm font-medium text-blue-700 dark:text-blue-300">
              Novo Cliente
            </p>
          </button>
          <button class="p-4 bg-yellow-50 dark:bg-yellow-900/20 rounded-lg hover:bg-yellow-100 dark:hover:bg-yellow-900/30 transition group">
            <i class="fas fa-chart-line text-yellow-600 dark:text-yellow-400 mb-2 text-xl"></i>
            <p class="text-sm font-medium text-yellow-700 dark:text-yellow-300">
              Ver Relatórios
            </p>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>