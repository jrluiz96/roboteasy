<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { conversationService, type ConversationListItem, type ConversationDetail } from '@/services/conversation.service'
import { useToastStore } from '@/stores/toastStore'
import { useRouter } from 'vue-router'

const toastStore = useToastStore()
const router = useRouter()

const items = ref<ConversationListItem[]>([])
const loading = ref(false)
const search = ref('')

// Detail modal
const showDetail = ref(false)
const detailLoading = ref(false)
const detail = ref<ConversationDetail | null>(null)
const detailTab = ref<'info' | 'chat'>('info')

const filtered = computed(() => {
  const q = search.value.toLowerCase().trim()
  if (!q) return items.value
  return items.value.filter(c =>
    c.clientName.toLowerCase().includes(q) ||
    c.clientEmail?.toLowerCase().includes(q) ||
    c.attendants.some(a => a.name.toLowerCase().includes(q))
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

async function openDetail(convId: number) {
  showDetail.value = true
  detailLoading.value = true
  detailTab.value = 'info'
  detail.value = null
  try {
    detail.value = await conversationService.getById(convId)
  } catch {
    toastStore.error('Erro ao carregar conversa')
    showDetail.value = false
  } finally {
    detailLoading.value = false
  }
}

function closeDetail() {
  showDetail.value = false
  detail.value = null
}

function formatDate(d: string) {
  return new Date(d).toLocaleDateString('pt-BR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

function formatDuration(seconds: number | null) {
  if (!seconds) return '-'
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  if (h > 0) return `${h}h ${m}m ${s}s`
  return m > 0 ? `${m}m ${s}s` : `${s}s`
}

function initials(name: string) {
  return name.split(' ').map((s: string) => s[0]).slice(0, 2).join('').toUpperCase()
}

function goToAtendimento() {
  router.push({ name: 'customer-service' })
}

function isSystem(type: number) {
  return type === 6
}
</script>

<template>
  <div class="space-y-6">

    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Histórico</h1>
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
            placeholder="Buscar cliente, operador..."
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
          <!-- Avatar cliente -->
          <div class="w-9 h-9 bg-purple-100 dark:bg-purple-900/30 rounded-full flex items-center justify-center flex-shrink-0">
            <span class="text-xs font-bold text-purple-600 dark:text-purple-400">{{ initials(conv.clientName) }}</span>
          </div>

          <!-- Info cliente -->
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 dark:text-white">{{ conv.clientName }}</p>
            <p class="text-xs text-gray-500 dark:text-gray-400 truncate">{{ conv.clientEmail || '' }}</p>
          </div>

          <!-- Operadores -->
          <div class="hidden md:flex flex-1 min-w-0 items-center gap-1 flex-wrap">
            <template v-if="conv.attendants.length > 0">
              <span
                v-for="att in conv.attendants"
                :key="att.userId"
                class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-blue-100 dark:bg-blue-900/30 text-xs font-medium text-blue-700 dark:text-blue-300"
              >
                <i class="fas fa-headset text-[10px]"></i>
                {{ att.name.split(' ')[0] }}
              </span>
            </template>
            <span v-else class="text-xs text-gray-400 italic">Sem atendente</span>
          </div>

          <!-- Stats -->
          <div class="text-right flex-shrink-0 flex items-center gap-4">
            <div>
              <p class="text-xs font-medium text-gray-900 dark:text-white">{{ conv.messageCount }} msg</p>
              <p class="text-xs text-gray-400 dark:text-gray-500 mt-0.5">{{ conv.finishedAt ? formatDate(conv.finishedAt) : '' }}</p>
            </div>
            <button
              @click="openDetail(conv.id)"
              class="p-2 rounded-lg hover:bg-purple-100 dark:hover:bg-purple-900/30 text-purple-600 dark:text-purple-400 transition"
              title="Ver conversa"
            >
              <i class="fas fa-eye"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- ═══════════ Modal Detalhes da Conversa ═══════════ -->
    <div v-if="showDetail" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/60" @click="closeDetail"></div>

      <div class="relative bg-white dark:bg-gray-800 rounded-2xl shadow-2xl w-full max-w-3xl max-h-[85vh] flex flex-col overflow-hidden">

        <!-- Modal header -->
        <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">Detalhes da Conversa</h2>
          <button @click="closeDetail" class="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-400 transition">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <!-- Loading -->
        <div v-if="detailLoading" class="flex-1 flex items-center justify-center py-16">
          <i class="fas fa-spinner fa-spin text-2xl text-purple-500"></i>
        </div>

        <!-- Content -->
        <template v-else-if="detail">

          <!-- Tabs -->
          <div class="flex border-b border-gray-200 dark:border-gray-700 px-6">
            <button
              @click="detailTab = 'info'"
              :class="[
                'px-4 py-3 text-sm font-medium border-b-2 transition -mb-px',
                detailTab === 'info'
                  ? 'border-purple-500 text-purple-600 dark:text-purple-400'
                  : 'border-transparent text-gray-500 hover:text-gray-700 dark:hover:text-gray-300'
              ]"
            >
              <i class="fas fa-info-circle mr-2"></i>Informações
            </button>
            <button
              @click="detailTab = 'chat'"
              :class="[
                'px-4 py-3 text-sm font-medium border-b-2 transition -mb-px',
                detailTab === 'chat'
                  ? 'border-purple-500 text-purple-600 dark:text-purple-400'
                  : 'border-transparent text-gray-500 hover:text-gray-700 dark:hover:text-gray-300'
              ]"
            >
              <i class="fas fa-comments mr-2"></i>Mensagens
              <span class="ml-1 px-1.5 py-0.5 rounded-full bg-gray-200 dark:bg-gray-600 text-xs">{{ detail.messages.length }}</span>
            </button>
          </div>

          <!-- Tab: Info -->
          <div v-if="detailTab === 'info'" class="flex-1 overflow-y-auto p-6 space-y-6">

            <!-- Cliente -->
            <div>
              <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">Cliente</h3>
              <div class="grid grid-cols-2 gap-4">
                <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                  <p class="text-xs text-gray-500 dark:text-gray-400">Nome</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ detail.clientName }}</p>
                </div>
                <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                  <p class="text-xs text-gray-500 dark:text-gray-400">Email</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ detail.clientEmail || '-' }}</p>
                </div>
                <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                  <p class="text-xs text-gray-500 dark:text-gray-400">Telefone</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ detail.clientPhone || '-' }}</p>
                </div>
                <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                  <p class="text-xs text-gray-500 dark:text-gray-400">ID</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">#{{ detail.clientId }}</p>
                </div>
              </div>
            </div>

            <!-- Conversa -->
            <div>
              <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">Conversa</h3>
              <div class="grid grid-cols-2 gap-4">
                <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                  <p class="text-xs text-gray-500 dark:text-gray-400">Iniciada em</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ formatDate(detail.createdAt) }}</p>
                </div>
                <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                  <p class="text-xs text-gray-500 dark:text-gray-400">Finalizada em</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ detail.finishedAt ? formatDate(detail.finishedAt) : '-' }}</p>
                </div>
                <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                  <p class="text-xs text-gray-500 dark:text-gray-400">Tempo de atendimento</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ formatDuration(detail.attendanceTime) }}</p>
                </div>
                <div class="bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3">
                  <p class="text-xs text-gray-500 dark:text-gray-400">Total de mensagens</p>
                  <p class="text-sm font-medium text-gray-900 dark:text-white mt-0.5">{{ detail.messages.length }}</p>
                </div>
              </div>
            </div>

            <!-- Operadores -->
            <div>
              <h3 class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">Operadores</h3>
              <div v-if="detail.attendants.length > 0" class="space-y-2">
                <div
                  v-for="att in detail.attendants"
                  :key="att.userId"
                  class="flex items-center gap-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg p-3"
                >
                  <div class="w-8 h-8 bg-blue-100 dark:bg-blue-900/30 rounded-full flex items-center justify-center flex-shrink-0">
                    <span class="text-xs font-bold text-blue-600 dark:text-blue-400">{{ initials(att.name) }}</span>
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-900 dark:text-white">{{ att.name }}</p>
                  </div>
                </div>
              </div>
              <p v-else class="text-sm text-gray-400 italic">Nenhum operador participou</p>
            </div>
          </div>

          <!-- Tab: Chat -->
          <div v-if="detailTab === 'chat'" class="flex-1 overflow-y-auto p-6">
            <div v-if="detail.messages.length === 0" class="text-center py-8">
              <i class="fas fa-comments text-3xl text-gray-300 dark:text-gray-600 mb-2 block"></i>
              <p class="text-sm text-gray-400">Nenhuma mensagem</p>
            </div>
            <div v-else class="space-y-3">
              <div
                v-for="msg in detail.messages"
                :key="msg.id"
                :class="[
                  'max-w-[80%] rounded-xl px-4 py-2',
                  isSystem(msg.type)
                    ? 'mx-auto max-w-none text-center bg-gray-100 dark:bg-gray-700/50 text-xs text-gray-500 dark:text-gray-400 italic py-1.5 px-3 rounded-full'
                    : msg.userId
                      ? 'ml-auto bg-blue-600 text-white'
                      : 'mr-auto bg-gray-100 dark:bg-gray-700 text-gray-900 dark:text-white'
                ]"
              >
                <!-- Sender name for non-system messages -->
                <p v-if="!isSystem(msg.type) && msg.senderName" :class="[
                  'text-[11px] font-semibold mb-0.5',
                  msg.userId ? 'text-purple-200' : 'text-purple-500 dark:text-purple-400'
                ]">
                  {{ msg.senderName }}
                </p>
                <p class="text-sm whitespace-pre-wrap break-words">{{ msg.content }}</p>
                <p v-if="!isSystem(msg.type)" class="text-[10px] mt-1 opacity-60">{{ formatDate(msg.createdAt) }}</p>
              </div>
            </div>
          </div>
        </template>
      </div>
    </div>

  </div>
</template>