<script setup lang="ts">
import { ref, computed } from 'vue'

// Dados simulados de histórico
const historyItems = ref([
  {
    id: 1,
    type: 'meeting',
    title: 'Reunião com João Silva',
    description: 'Consultoria sobre implementação de automação',
    client: 'João Silva',
    agent: 'Francisco Luiz',
    date: '2026-02-18T14:30:00Z',
    duration: '45 min',
    status: 'completed',
    rating: 5,
    notes: 'Cliente muito satisfeito com a apresentação'
  },
  {
    id: 2,
    type: 'chat',
    title: 'Chat com Maria Santos',
    description: 'Dúvidas sobre pricing e funcionalidades',
    client: 'Maria Santos',
    agent: 'Admin Master',
    date: '2026-02-18T11:20:00Z',
    duration: '12 min',
    status: 'completed',
    rating: 4,
    notes: 'Enviou proposta por email'
  },
  {
    id: 3,
    type: 'meeting',
    title: 'Demo para Carlos Oliveira',
    description: 'Demonstração da plataforma completa',
    client: 'Carlos Oliveira',
    agent: 'Francisco Luiz',
    date: '2026-02-17T16:00:00Z',
    duration: '60 min',
    status: 'completed',
    rating: 5,
    notes: 'Cliente interessado em pacote Enterprise'
  },
  {
    id: 4,
    type: 'call',
    title: 'Ligação com Ana Costa',
    description: 'Suporte técnico inicial',
    client: 'Ana Costa',
    agent: 'Admin Master',
    date: '2026-02-17T09:15:00Z',
    duration: '23 min',
    status: 'completed',
    rating: 3,
    notes: 'Precisa de treinamento adicional'
  }
])

const filterType = ref('all')
const filterAgent = ref('all')
const searchQuery = ref('')
const dateRange = ref('week')

const filteredHistory = computed(() => {
  let filtered = historyItems.value

  if (filterType.value !== 'all') {
    filtered = filtered.filter(item => item.type === filterType.value)
  }

  if (filterAgent.value !== 'all') {
    filtered = filtered.filter(item => item.agent === filterAgent.value)
  }

  if (searchQuery.value) {
    filtered = filtered.filter(item =>
      item.title.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      item.client.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      item.description.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
  }

  // Filtro por data (simplificado)
  if (dateRange.value !== 'all') {
    const now = new Date()
    const daysDiff = dateRange.value === 'week' ? 7 : dateRange.value === 'month' ? 30 : 0
    filtered = filtered.filter(item => {
      const itemDate = new Date(item.date)
      const diffTime = Math.abs(now.getTime() - itemDate.getTime())
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
      return diffDays <= daysDiff
    })
  }

  return filtered.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
})

const agents = computed(() => {
  const uniqueAgents = [...new Set(historyItems.value.map(item => item.agent))]
  return uniqueAgents
})

function getTypeIcon(type: string): string {
  const icons = {
    'meeting': 'fa-video',
    'chat': 'fa-comments',
    'call': 'fa-phone'
  }
  return icons[type as keyof typeof icons] || 'fa-circle'
}

function getTypeColor(type: string): string {
  const colors = {
    'meeting': 'bg-blue-100 text-blue-800 dark:bg-blue-900/20 dark:text-blue-400',
    'chat': 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400',
    'call': 'bg-purple-100 text-purple-800 dark:bg-purple-900/20 dark:text-purple-400'
  }
  return colors[type as keyof typeof colors] || 'bg-gray-100 text-gray-800'
}

function getStatusColor(status: string): string {
  const colors = {
    'completed': 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400',
    'cancelled': 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400',
    'pending': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400'
  }
  return colors[status as keyof typeof colors] || 'bg-gray-100 text-gray-800'
}

function formatDate(date: string): string {
  return new Date(date).toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function getRatingStars(rating: number): string {
  return '★'.repeat(rating) + '☆'.repeat(5 - rating)
}

function viewDetails(itemId: number) {
  console.log('View details for:', itemId)
}

function exportHistory() {
  console.log('Export history')
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex justify-between items-center">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
          Histórico de Atendimentos
        </h1>
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Acompanhe todo o histórico de interações com clientes
        </p>
      </div>
      <button
        @click="exportHistory"
        class="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg transition"
      >
        <i class="fas fa-download mr-2"></i>
        Exportar
      </button>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-video text-blue-600 dark:text-blue-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Reuniões</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">
              {{ historyItems.filter(h => h.type === 'meeting').length }}
            </p>
          </div>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-green-100 dark:bg-green-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-comments text-green-600 dark:text-green-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Chats</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">
              {{ historyItems.filter(h => h.type === 'chat').length }}
            </p>
          </div>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-purple-100 dark:bg-purple-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-phone text-purple-600 dark:text-purple-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Ligações</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">
              {{ historyItems.filter(h => h.type === 'call').length }}
            </p>
          </div>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-yellow-100 dark:bg-yellow-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-star text-yellow-600 dark:text-yellow-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Avaliação Média</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">
              {{ (historyItems.reduce((sum, h) => sum + h.rating, 0) / historyItems.length).toFixed(1) }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Buscar</label>
          <div class="relative">
            <i class="fas fa-search absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400"></i>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Cliente, título..."
              class="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
            />
          </div>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Tipo</label>
          <select
            v-model="filterType"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
          >
            <option value="all">Todos os tipos</option>
            <option value="meeting">Reuniões</option>
            <option value="chat">Chats</option>
            <option value="call">Ligações</option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Atendente</label>
          <select
            v-model="filterAgent"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
          >
            <option value="all">Todos os atendentes</option>
            <option v-for="agent in agents" :key="agent" :value="agent">
              {{ agent }}
            </option>
          </select>
        </div>
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Período</label>
          <select
            v-model="dateRange"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
          >
            <option value="all">Todos os períodos</option>
            <option value="week">Última semana</option>
            <option value="month">Último mês</option>
          </select>
        </div>
      </div>
    </div>

    <!-- History List -->
    <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
      <div class="p-6">
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
          Histórico ({{ filteredHistory.length }} registros)
        </h2>
        <div class="space-y-4">
          <div
            v-for="item in filteredHistory"
            :key="item.id"
            class="border border-gray-200 dark:border-gray-700 rounded-lg p-4 hover:bg-gray-50 dark:hover:bg-gray-700/50 transition"
          >
            <div class="flex items-start justify-between">
              <div class="flex items-start gap-4 flex-1">
                <div :class="['w-10 h-10 rounded-lg flex items-center justify-center', getTypeColor(item.type)]">
                  <i :class="['fas', getTypeIcon(item.type)]"></i>
                </div>
                <div class="flex-1 min-w-0">
                  <div class="flex items-center gap-2 mb-2">
                    <h3 class="font-medium text-gray-900 dark:text-white">{{ item.title }}</h3>
                    <span :class="['px-2 py-1 text-xs font-medium rounded-full', getStatusColor(item.status)]">
                      {{ item.status === 'completed' ? 'Concluído' : item.status }}
                    </span>
                  </div>
                  <p class="text-sm text-gray-600 dark:text-gray-400 mb-2">{{ item.description }}</p>
                  <div class="flex items-center gap-4 text-xs text-gray-500 dark:text-gray-400">
                    <span>
                      <i class="fas fa-user mr-1"></i>
                      {{ item.client }}
                    </span>
                    <span>
                      <i class="fas fa-headset mr-1"></i>
                      {{ item.agent }}
                    </span>
                    <span>
                      <i class="fas fa-clock mr-1"></i>
                      {{ item.duration }}
                    </span>
                    <span>
                      <i class="fas fa-calendar mr-1"></i>
                      {{ formatDate(item.date) }}
                    </span>
                  </div>
                  <div v-if="item.notes" class="mt-2 text-sm text-gray-600 dark:text-gray-400 bg-gray-50 dark:bg-gray-700 p-2 rounded">
                    <i class="fas fa-sticky-note mr-1"></i>
                    {{ item.notes }}
                  </div>
                </div>
              </div>
              <div class="flex items-center gap-3 ml-4">
                <div class="text-right">
                  <div class="text-yellow-500 text-sm">
                    {{ getRatingStars(item.rating) }}
                  </div>
                  <div class="text-xs text-gray-500 dark:text-gray-400">{{ item.rating }}/5</div>
                </div>
                <button
                  @click="viewDetails(item.id)"
                  class="p-2 text-gray-600 dark:text-gray-400 hover:text-purple-600 dark:hover:text-purple-400 transition"
                  title="Ver detalhes"
                >
                  <i class="fas fa-eye"></i>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>