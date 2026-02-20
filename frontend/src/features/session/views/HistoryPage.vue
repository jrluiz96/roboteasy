<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { conversationService, type ConversationListItem } from '@/services/conversation.service'
import { useToastStore } from '@/stores/toastStore'
import { useRouter } from 'vue-router'

const toastStore = useToastStore()
const router = useRouter()

const items = ref<ConversationListItem[]>([])
const loading = ref(false)
const search = ref('')

const filtered = computed(() => {
  const q = search.value.toLowerCase().trim()
  if (!q) return items.value
  return items.value.filter(c =>
    c.clientName.toLowerCase().includes(q) ||
    c.clientEmail?.toLowerCase().includes(q) ||
    c.lastMessage?.toLowerCase().includes(q)
  )
})

onMounted(loadHistory)

async function loadHistory() {
  loading.value = true
  try {
    items.value = await conversationService.getHistory()
  } catch {
    toastStore.error('Erro ao carregar historico')
  } finally {
    loading.value = false
  }
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('pt-BR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

function formatDuration(seconds: number | null) {
  if (!seconds) return ''
  const m = Math.floor(seconds / 60)
  const s = seconds % 60
  return m > 0 ? `${m}m ${s}s` : `${s}s`
}

function initials(name: string) {
  return name.split(' ').map((s: string) => s[0]).slice(0, 2).join('').toUpperCase()
}

function goToAtendimento() {
  router.push({ name: 'customer-service' })
}
</script>

<template>
  <div class="space-y-6">

    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Historico</h1>
        <p class="text-sm text-gray-500 dark:text-gray-400">Conversas finalizadas</p>
      </div>
      <div class="flex items-center gap-3">
        <button @click="loadHistory" class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-500 transition" title="Recarregar">
          <i class="fas fa-sync-alt"></i>
        </button>
        <button @click="goToAtendimento" class="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white text-sm rounded-lg transition flex items-center gap-2">
          <i class="fas fa-headset"></i> Ir para Atendimento
        </button>
      </div>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-purple-100 dark:bg-purple-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-comments text-purple-600 dark:text-purple-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Total finalizadas</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ items.length }}</p>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-green-100 dark:bg-green-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-users text-green-600 dark:text-green-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Clientes atendidos</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">
            {{ new Set(items.map(i => i.clientId)).size }}
          </p>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-comment-dots text-blue-600 dark:text-blue-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Total de mensagens</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ items.reduce((a, c) => a + c.messageCount, 0) }}</p>
        </div>
      </div>
    </div>

    <!-- Table card -->
    <div class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">

      <!-- Search -->
      <div class="px-4 py-3 border-b border-gray-200 dark:border-gray-700 flex items-center gap-3">
        <div class="relative flex-1 max-w-xs">
          <i class="fas fa-search absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
          <input
            v-model="search"
            type="text"
            placeholder="Buscar cliente, mensagem..."
            class="w-full pl-8 pr-3 py-2 text-sm bg-gray-100 dark:bg-gray-700 rounded-lg outline-none focus:ring-2 focus:ring-purple-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>
        <span class="text-xs text-gray-400">{{ filtered.length }} registro(s)</span>
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
        <i class="fas fa-inbox text-4xl text-gray-300 dark:text-gray-600 mb-3 block"></i>
        <p class="text-sm text-gray-400 dark:text-gray-500">Nenhuma conversa encontrada</p>
      </div>

      <!-- Linhas -->
      <div v-else class="divide-y divide-gray-100 dark:divide-gray-700">
        <div
          v-for="conv in filtered"
          :key="conv.id"
          class="flex items-center gap-4 px-4 py-3 hover:bg-gray-50 dark:hover:bg-gray-700/30 transition"
        >
          <div class="w-9 h-9 bg-purple-100 dark:bg-purple-900/30 rounded-full flex items-center justify-center flex-shrink-0">
            <span class="text-xs font-bold text-purple-600 dark:text-purple-400">{{ initials(conv.clientName) }}</span>
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 dark:text-white">{{ conv.clientName }}</p>
            <p class="text-xs text-gray-500 dark:text-gray-400 truncate">{{ conv.clientEmail || '' }}</p>
          </div>
          <div class="hidden md:block flex-1 min-w-0">
            <p class="text-xs text-gray-500 dark:text-gray-400 truncate">{{ conv.lastMessage || '' }}</p>
          </div>
          <div class="text-right flex-shrink-0">
            <p class="text-xs font-medium text-gray-900 dark:text-white">{{ conv.messageCount }} msg</p>
            <p class="text-xs text-gray-400 dark:text-gray-500 mt-0.5">{{ conv.finishedAt ? formatDate(conv.finishedAt) : '' }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>