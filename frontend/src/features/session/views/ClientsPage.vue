<script setup lang="ts">
import { ref, computed } from 'vue'

// Dados simulados de clientes
const clients = ref([
  {
    id: 1,
    name: 'João Silva',
    email: 'joao.silva@email.com',
    company: 'Tech Solutions',
    phone: '+55 11 99999-0001',
    status: 'active',
    lastContact: '2026-02-17',
    meetings: 8,
    satisfaction: 95
  },
  {
    id: 2,
    name: 'Maria Santos',
    email: 'maria.santos@empresa.com',
    company: 'Digital Corp',
    phone: '+55 11 99999-0002',
    status: 'inactive',
    lastContact: '2026-02-10',
    meetings: 3,
    satisfaction: 87
  },
  {
    id: 3,
    name: 'Carlos Oliveira',
    email: 'carlos@startup.com',
    company: 'Startup Inc',
    phone: '+55 11 99999-0003',
    status: 'active',
    lastContact: '2026-02-18',
    meetings: 12,
    satisfaction: 98
  }
])

const searchQuery = ref('')
const selectedStatus = ref('all')
const showAddClient = ref(false)

const newClient = ref({
  name: '',
  email: '',
  company: '',
  phone: ''
})

const filteredClients = computed(() => {
  let filtered = clients.value

  if (searchQuery.value) {
    filtered = filtered.filter(client =>
      client.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      client.email.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      client.company.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
  }

  if (selectedStatus.value !== 'all') {
    filtered = filtered.filter(client => client.status === selectedStatus.value)
  }

  return filtered
})

function getStatusColor(status: string) {
  return status === 'active'
    ? 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400'
    : 'bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400'
}

function addClient() {
  if (newClient.value.name && newClient.value.email) {
    clients.value.push({
      id: Date.now(),
      ...newClient.value,
      status: 'active',
      lastContact: new Date().toISOString().split('T')[0],
      meetings: 0,
      satisfaction: 0
    })
    newClient.value = { name: '', email: '', company: '', phone: '' }
    showAddClient.value = false
  }
}

function editClient(clientId: number) {
  console.log('Edit client:', clientId)
}

function deleteClient(clientId: number) {
  clients.value = clients.value.filter(c => c.id !== clientId)
}

function startMeeting(clientId: number) {
  console.log('Start meeting with client:', clientId)
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex justify-between items-center">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
          Clientes
        </h1>
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Gerencie seus clientes e relacionamentos
        </p>
      </div>
      <button
        @click="showAddClient = true"
        class="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition"
      >
        <i class="fas fa-plus mr-2"></i>
        Novo Cliente
      </button>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-users text-blue-600 dark:text-blue-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Total de Clientes</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ clients.length }}</p>
          </div>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-green-100 dark:bg-green-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-user-check text-green-600 dark:text-green-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Ativos</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ clients.filter(c => c.status === 'active').length }}</p>
          </div>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-purple-100 dark:bg-purple-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-video text-purple-600 dark:text-purple-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Reuniões Este Mês</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ clients.reduce((sum, c) => sum + c.meetings, 0) }}</p>
          </div>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-yellow-100 dark:bg-yellow-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-star text-yellow-600 dark:text-yellow-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Satisfação Média</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">
              {{ Math.round(clients.reduce((sum, c) => sum + c.satisfaction, 0) / clients.length) }}%
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
      <div class="flex flex-col md:flex-row gap-4">
        <div class="flex-1">
          <div class="relative">
            <i class="fas fa-search absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400"></i>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Buscar por nome, email ou empresa..."
              class="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
            />
          </div>
        </div>
        <div class="w-full md:w-48">
          <select
            v-model="selectedStatus"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
          >
            <option value="all">Todos os Status</option>
            <option value="active">Ativos</option>
            <option value="inactive">Inativos</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Clients Table -->
    <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full">
          <thead class="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Cliente</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Empresa</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Status</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Último Contato</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Reuniões</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Satisfação</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Ações</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
            <tr v-for="client in filteredClients" :key="client.id" class="hover:bg-gray-50 dark:hover:bg-gray-700/50">
              <td class="px-6 py-4">
                <div>
                  <div class="font-medium text-gray-900 dark:text-white">{{ client.name }}</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">{{ client.email }}</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">{{ client.phone }}</div>
                </div>
              </td>
              <td class="px-6 py-4 text-sm text-gray-900 dark:text-white">
                {{ client.company }}
              </td>
              <td class="px-6 py-4">
                <span :class="['px-2 py-1 text-xs font-medium rounded-full', getStatusColor(client.status)]">
                  {{ client.status === 'active' ? 'Ativo' : 'Inativo' }}
                </span>
              </td>
              <td class="px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
                {{ new Date(client.lastContact).toLocaleDateString('pt-BR') }}
              </td>
              <td class="px-6 py-4 text-sm text-gray-900 dark:text-white">
                {{ client.meetings }}
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center">
                  <span class="text-sm font-medium text-gray-900 dark:text-white mr-2">{{ client.satisfaction }}%</span>
                  <div class="w-16 bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                    <div
                      class="bg-gradient-to-r from-green-400 to-green-600 h-2 rounded-full"
                      :style="{ width: `${client.satisfaction}%` }"
                    ></div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 text-right space-x-2">
                <button
                  @click="startMeeting(client.id)"
                  class="text-green-600 dark:text-green-400 hover:text-green-900 dark:hover:text-green-300"
                  title="Iniciar reunião"
                >
                  <i class="fas fa-video"></i>
                </button>
                <button
                  @click="editClient(client.id)"
                  class="text-purple-600 dark:text-purple-400 hover:text-purple-900 dark:hover:text-purple-300"
                  title="Editar"
                >
                  <i class="fas fa-edit"></i>
                </button>
                <button
                  @click="deleteClient(client.id)"
                  class="text-red-600 dark:text-red-400 hover:text-red-900 dark:hover:text-red-300"
                  title="Excluir"
                >
                  <i class="fas fa-trash"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add Client Modal -->
    <div v-if="showAddClient" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md mx-4">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Novo Cliente</h3>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Nome</label>
            <input
              v-model="newClient.name"
              type="text"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
              required
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Email</label>
            <input
              v-model="newClient.email"
              type="email"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
              required
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Empresa</label>
            <input
              v-model="newClient.company"
              type="text"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Telefone</label>
            <input
              v-model="newClient.phone"
              type="tel"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
            />
          </div>
        </div>
        <div class="flex gap-3 mt-6">
          <button
            @click="addClient"
            class="flex-1 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition"
          >
            Adicionar
          </button>
          <button
            @click="showAddClient = false"
            class="flex-1 px-4 py-2 bg-gray-300 hover:bg-gray-400 text-gray-700 rounded-lg transition"
          >
            Cancelar
          </button>
        </div>
      </div>
    </div>
  </div>
</template>