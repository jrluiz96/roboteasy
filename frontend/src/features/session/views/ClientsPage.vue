<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { clientsService, type ClientItem } from '@/services/clients.service'
import { useToastStore } from '@/stores/toastStore'

const toastStore = useToastStore()

const clients = ref<ClientItem[]>([])
const loading = ref(false)
const search = ref('')
const statusFilter = ref<'all' | 'online' | 'offline'>('all')

// Detail modal
const showDetail = ref(false)
const selectedClient = ref<ClientItem | null>(null)

const filtered = computed(() => {
  let result = clients.value
  const q = search.value.toLowerCase().trim()
  if (q) {
    result = result.filter(c =>
      c.name.toLowerCase().includes(q) ||
      c.email?.toLowerCase().includes(q) ||
      c.phone?.toLowerCase().includes(q) ||
      c.cpf?.toLowerCase().includes(q)
    )
  }
  if (statusFilter.value === 'online') result = result.filter(c => c.isOnline)
  if (statusFilter.value === 'offline') result = result.filter(c => !c.isOnline)
  return result
})

const totalConversations = computed(() => clients.value.reduce((a, c) => a + c.conversationCount, 0))
const totalMessages = computed(() => clients.value.reduce((a, c) => a + c.messageCount, 0))
const onlineCount = computed(() => clients.value.filter(c => c.isOnline).length)

onMounted(loadClients)

async function loadClients() {
  loading.value = true
  try {
    clients.value = await clientsService.getAll()
  } catch {
    toastStore.error('Erro ao carregar clientes')
  } finally {
    loading.value = false
  }
}

function openDetail(client: ClientItem) {
  selectedClient.value = client
  showDetail.value = true
}

function closeDetail() {
  showDetail.value = false
  selectedClient.value = null
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('pt-BR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

function formatDateShort(d: string) {
  return new Date(d).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' })
}

function initials(name: string) {
  return name.split(' ').map((s: string) => s[0]).slice(0, 2).join('').toUpperCase()
}
</script>

<template>
  <div class="space-y-6">

    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Clientes</h1>
        <p class="text-sm text-gray-500 dark:text-gray-400">Clientes que iniciaram conversas no chat</p>
      </div>
      <button @click="loadClients" class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-500 transition" title="Recarregar">
        <i class="fas fa-sync-alt"></i>
      </button>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-purple-100 dark:bg-purple-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-users text-purple-600 dark:text-purple-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Total de clientes</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ clients.length }}</p>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-green-100 dark:bg-green-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-circle text-green-500 dark:text-green-400 text-xs"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Online agora</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ onlineCount }}</p>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-comments text-blue-600 dark:text-blue-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Total de conversas</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ totalConversations }}</p>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-amber-100 dark:bg-amber-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-comment-dots text-amber-600 dark:text-amber-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Total de mensagens</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ totalMessages }}</p>
        </div>
      </div>
    </div>

    <!-- Table card -->
    <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">

      <!-- Search + filter -->
      <div class="px-4 py-3 border-b border-gray-200 dark:border-gray-700 flex flex-col sm:flex-row items-start sm:items-center gap-3">
        <div class="relative flex-1 max-w-xs">
          <i class="fas fa-search absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
          <input
            v-model="search"
            type="text"
            placeholder="Buscar nome, email, telefone..."
            class="w-full pl-8 pr-3 py-2 text-sm bg-gray-100 dark:bg-gray-700 rounded-lg outline-none focus:ring-2 focus:ring-purple-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>
        <div class="flex items-center gap-2">
          <button
            v-for="opt in ([
              { key: 'all', label: 'Todos' },
              { key: 'online', label: 'Online' },
              { key: 'offline', label: 'Offline' },
            ] as const)" :key="opt.key"
            @click="statusFilter = opt.key"
            :class="[
              'px-3 py-1.5 text-xs rounded-full transition font-medium',
              statusFilter === opt.key
                ? 'bg-purple-600 text-white'
                : 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
            ]"
          >
            {{ opt.label }}
          </button>
        </div>
        <span class="text-xs text-gray-400 ml-auto">{{ filtered.length }} cliente(s)</span>
      </div>

      <!-- Skeleton -->
      <div v-if="loading" class="p-6 space-y-4">
        <div v-for="i in 5" :key="i" class="flex gap-4 animate-pulse">
          <div class="w-9 h-9 bg-gray-200 dark:bg-gray-700 rounded-full"></div>
          <div class="flex-1 space-y-2 pt-1">
            <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/3"></div>
            <div class="h-2 bg-gray-200 dark:bg-gray-700 rounded w-2/3"></div>
          </div>
          <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-24"></div>
        </div>
      </div>

      <!-- Vazio -->
      <div v-else-if="filtered.length === 0" class="p-12 text-center">
        <i class="fas fa-user-slash text-4xl text-gray-300 dark:text-gray-600 mb-3 block"></i>
        <p class="text-sm text-gray-400 dark:text-gray-500">Nenhum cliente encontrado</p>
      </div>

      <!-- Linhas -->
      <div v-else class="divide-y divide-gray-100 dark:divide-gray-700">
        <div
          v-for="client in filtered"
          :key="client.id"
          class="flex items-center gap-4 px-4 py-3 hover:bg-gray-50 dark:hover:bg-gray-700/30 transition cursor-pointer"
          @click="openDetail(client)"
        >
          <!-- Avatar -->
          <div class="relative flex-shrink-0">
            <div class="w-9 h-9 bg-purple-100 dark:bg-purple-900/30 rounded-full flex items-center justify-center">
              <span class="text-xs font-bold text-purple-600 dark:text-purple-400">{{ initials(client.name) }}</span>
            </div>
            <span
              v-if="client.isOnline"
              class="absolute -bottom-0.5 -right-0.5 w-3 h-3 bg-green-500 border-2 border-white dark:border-gray-800 rounded-full"
            ></span>
          </div>

          <!-- Info -->
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 dark:text-white">{{ client.name }}</p>
            <p class="text-xs text-gray-500 dark:text-gray-400 truncate">{{ client.email || client.phone || '-' }}</p>
          </div>

          <!-- Stats -->
          <div class="hidden md:flex items-center gap-6 text-xs text-gray-500 dark:text-gray-400 flex-shrink-0">
            <div class="text-center" title="Conversas">
              <i class="fas fa-comments mr-1"></i>{{ client.conversationCount }}
            </div>
            <div class="text-center" title="Mensagens">
              <i class="fas fa-comment-dots mr-1"></i>{{ client.messageCount }}
            </div>
            <div class="w-24 text-right" title="Último contato">
              {{ client.lastConversationAt ? formatDateShort(client.lastConversationAt) : '-' }}
            </div>
          </div>

          <!-- Arrow -->
          <i class="fas fa-chevron-right text-gray-300 dark:text-gray-600 text-xs flex-shrink-0"></i>
        </div>
      </div>
    </div>

    <!-- ═══════════ Modal Detalhes do Cliente ═══════════ -->
    <div v-if="showDetail && selectedClient" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/60" @click="closeDetail"></div>

      <div class="relative bg-white dark:bg-gray-800 rounded-2xl shadow-2xl w-full max-w-lg max-h-[80vh] flex flex-col overflow-hidden">

        <!-- Modal header -->
        <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <div class="flex items-center gap-3">
            <div class="relative">
              <div class="w-10 h-10 bg-purple-100 dark:bg-purple-900/30 rounded-full flex items-center justify-center">
                <span class="text-sm font-bold text-purple-600 dark:text-purple-400">{{ initials(selectedClient.name) }}</span>
              </div>
              <span
                v-if="selectedClient.isOnline"
                class="absolute -bottom-0.5 -right-0.5 w-3 h-3 bg-green-500 border-2 border-white dark:border-gray-800 rounded-full"
              ></span>
            </div>
            <div>
              <h2 class="text-lg font-bold text-gray-900 dark:text-white">{{ selectedClient.name }}</h2>
              <span :class="['text-xs font-medium', selectedClient.isOnline ? 'text-green-500' : 'text-gray-400']">
                {{ selectedClient.isOnline ? 'Online' : 'Offline' }}
              </span>
            </div>
          </div>
          <button @click="closeDetail" class="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-400 transition">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <!-- Content -->
        <div class="flex-1 overflow-y-auto p-6 space-y-6">

          <!-- Informações de contato -->
          <div>
            <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">Contato</h3>
            <div class="grid grid-cols-2 gap-4">
              <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                <p class="text-xs text-gray-500 dark:text-gray-400">Email</p>
                <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ selectedClient.email || '-' }}</p>
              </div>
              <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                <p class="text-xs text-gray-500 dark:text-gray-400">Telefone</p>
                <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ selectedClient.phone || '-' }}</p>
              </div>
              <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                <p class="text-xs text-gray-500 dark:text-gray-400">CPF</p>
                <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ selectedClient.cpf || '-' }}</p>
              </div>
              <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                <p class="text-xs text-gray-500 dark:text-gray-400">ID</p>
                <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">#{{ selectedClient.id }}</p>
              </div>
            </div>
          </div>

          <!-- Estatísticas -->
          <div>
            <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">Estatísticas</h3>
            <div class="grid grid-cols-2 gap-4">
              <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                <p class="text-xs text-gray-500 dark:text-gray-400">Conversas</p>
                <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ selectedClient.conversationCount }}</p>
              </div>
              <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                <p class="text-xs text-gray-500 dark:text-gray-400">Mensagens enviadas</p>
                <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ selectedClient.messageCount }}</p>
              </div>
              <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                <p class="text-xs text-gray-500 dark:text-gray-400">Último contato</p>
                <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">
                  {{ selectedClient.lastConversationAt ? formatDate(selectedClient.lastConversationAt) : 'Nunca' }}
                </p>
              </div>
              <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                <p class="text-xs text-gray-500 dark:text-gray-400">Cadastrado em</p>
                <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ formatDate(selectedClient.createdAt) }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>