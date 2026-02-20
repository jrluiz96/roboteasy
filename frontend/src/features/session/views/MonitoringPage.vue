<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import {
  conversationService,
  type ConversationListItem,
  type ConversationDetail,
  type ConversationMessage,
} from '@/services/conversation.service'
import { chatService, type ChatMessage } from '@/services/chat.service'
import { api } from '@/services/api'
import { usersService, type User } from '@/services/users.service'
import { useToastStore } from '@/stores/toastStore'
import { useFormatters } from '@/composables/useFormatters'

const toastStore = useToastStore()
const { formatDate, formatTime, formatPreview, initials } = useFormatters()

// ── State ────────────────────────────────────────────────────────────────────
const activeTab     = ref<'conversations' | 'operators'>('conversations')
const conversations = ref<ConversationListItem[]>([])
const loading       = ref(false)
const search        = ref('')
const statusFilter  = ref<'all' | 'waiting' | 'active'>('all')
const wsStatus      = ref<'connected' | 'reconnecting' | 'disconnected'>('disconnected')

// Operators tab
const operators        = ref<User[]>([])
const loadingOperators = ref(false)
const onlineUserIds    = ref<Set<number>>(new Set())
const operatorSearch   = ref('')
const operatorFilter   = ref<'all' | 'online' | 'offline'>('all')

// Detail modal
const showDetail       = ref(false)
const selectedConv     = ref<ConversationDetail | null>(null)
const loadingDetail    = ref(false)
const messagesEl       = ref<HTMLElement | null>(null)

// Invite modal
const showInviteModal  = ref(false)
const allUsers         = ref<User[]>([])
const loadingUsers     = ref(false)

// ── Computed ─────────────────────────────────────────────────────────────────
const filtered = computed(() => {
  let result = conversations.value
  const q = search.value.toLowerCase().trim()
  if (q) {
    result = result.filter(c =>
      c.clientName.toLowerCase().includes(q) ||
      c.clientEmail?.toLowerCase().includes(q) ||
      c.id.toString().includes(q)
    )
  }
  if (statusFilter.value === 'waiting') result = result.filter(c => c.status === 'waiting')
  if (statusFilter.value === 'active')  result = result.filter(c => c.status === 'active')
  return result
})

const totalActive  = computed(() => conversations.value.length)
const waitingCount = computed(() => conversations.value.filter(c => c.status === 'waiting').length)
const activeCount  = computed(() => conversations.value.filter(c => c.status === 'active').length)
const totalMsgs    = computed(() => conversations.value.reduce((a, c) => a + c.messageCount, 0))

// Atendentes no detalhe aberto (para filtrar no invite)
const detailAttendants = computed(() => selectedConv.value?.attendants ?? [])
const invitableUsers   = computed(() => {
  const linked = new Set(detailAttendants.value.map(a => a.userId))
  return allUsers.value.filter(u => !linked.has(u.id))
})

// ── Operators computed ───────────────────────────────────────────────────────
// Conta quantas conversas ativas cada operador está atendendo
const chatCountByUser = computed(() => {
  const map = new Map<number, number>()
  for (const conv of conversations.value) {
    if (!conv.attendants) continue
    for (const att of conv.attendants) {
      map.set(att.userId, (map.get(att.userId) ?? 0) + 1)
    }
  }
  return map
})

const filteredOperators = computed(() => {
  let result = operators.value.filter(u => !u.deletedAt)
  const q = operatorSearch.value.toLowerCase().trim()
  if (q) {
    result = result.filter(u =>
      u.name.toLowerCase().includes(q) ||
      u.username.toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q)
    )
  }
  if (operatorFilter.value === 'online')  result = result.filter(u => onlineUserIds.value.has(u.id))
  if (operatorFilter.value === 'offline') result = result.filter(u => !onlineUserIds.value.has(u.id))
  return result
})

const onlineOperatorsCount  = computed(() => operators.value.filter(u => !u.deletedAt && onlineUserIds.value.has(u.id)).length)
const busyOperatorsCount    = computed(() => {
  let count = 0
  for (const u of operators.value) {
    if (!u.deletedAt && (chatCountByUser.value.get(u.id) ?? 0) > 0) count++
  }
  return count
})

// ── Lifecycle ────────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadConversations(), loadOperators()])
  await connectWs()
})

onUnmounted(async () => {
  await chatService.disconnect()
})

// ── Data ─────────────────────────────────────────────────────────────────────
async function loadConversations() {
  loading.value = true
  try {
    conversations.value = await conversationService.getMonitor()
  } catch {
    toastStore.error('Erro ao carregar conversas')
  } finally {
    loading.value = false
  }
}

async function loadOperators() {
  loadingOperators.value = true
  try {
    operators.value = await usersService.getAll()
    // Popula o estado inicial de online a partir do campo isOnline retornado pela API
    onlineUserIds.value = new Set(operators.value.filter(u => u.isOnline).map(u => u.id))
  } catch {
    toastStore.error('Erro ao carregar operadores')
  } finally {
    loadingOperators.value = false
  }
}

// ── WebSocket ────────────────────────────────────────────────────────────────
async function connectWs() {
  const token = api.getToken()
  if (!token) return
  try {
    await chatService.connectAsMonitor(token)
    wsStatus.value = 'connected'

    // Entra em todos os grupos existentes
    for (const conv of conversations.value) {
      await chatService.joinConversation(conv.id)
    }
    setupListeners()
  } catch {
    wsStatus.value = 'disconnected'
    toastStore.warning('WebSocket indisponível — tempo real desativado')
  }
}

function setupListeners() {
  // Nova mensagem → atualiza preview e contador na lista
  chatService.onMessage((msg: ChatMessage) => {
    const item = conversations.value.find(c => c.id === msg.conversationId)
    if (item) {
      item.lastMessage   = msg.content
      item.lastMessageAt = msg.createdAt
      item.messageCount += 1
      if (msg.userId !== null && item.status !== 'active') item.status = 'active'
    }
    // Atualiza detalhe aberto em tempo real
    if (selectedConv.value && selectedConv.value.id === msg.conversationId) {
      const exists = selectedConv.value.messages.some(m => m.id === msg.id)
      if (!exists) {
        selectedConv.value.messages.push({
          id: msg.id,
          conversationId: msg.conversationId,
          userId: msg.userId,
          clientId: msg.clientId,
          type: msg.type,
          content: msg.content,
          senderName: msg.senderName,
          fileUrl: msg.fileUrl,
          createdAt: msg.createdAt,
        })
        scrollBottom()
      }
    }
  })

  // Nova conversa criada por cliente
  chatService.onConversationCreated(async (payload) => {
    if (conversations.value.some(c => c.id === payload.id)) return
    await chatService.joinConversation(payload.id)
    conversations.value.unshift({
      id:            payload.id,
      clientId:      payload.clientId,
      clientName:    payload.clientName,
      clientEmail:   payload.clientEmail,
      lastMessage:   null,
      lastMessageAt: null,
      messageCount:  0,
      createdAt:     payload.createdAt,
      finishedAt:    null,
      status:        'waiting',
      attendants:    [],
    })
  })

  // Conversa finalizada → remove da lista
  chatService.onConversationFinished(({ conversationId: convId }) => {
    conversations.value = conversations.value.filter(c => c.id !== convId)
    if (selectedConv.value?.id === convId) {
      selectedConv.value.status = 'finished'
      selectedConv.value.finishedAt = new Date().toISOString()
    }
  })

  // Atendente saiu → atualiza detalhe
  chatService.onAttendantLeft(({ conversationId: convId, userId: uid }) => {
    if (selectedConv.value && selectedConv.value.id === convId) {
      selectedConv.value.attendants = selectedConv.value.attendants.filter(a => a.userId !== uid)
    }
  })

  // Convite aceito → atualiza attendants e lista
  chatService.onConversationInvited(async ({ conversationId: convId }: { conversationId: number; invitedUserId?: number }) => {
    await chatService.joinConversation(convId)

    // Atualiza detalhe aberto (attendants mudaram)
    if (selectedConv.value && selectedConv.value.id === convId) {
      const detail = await conversationService.getById(convId)
      if (detail) selectedConv.value = detail
    }

    // Atualiza item da lista
    const existing = conversations.value.find(c => c.id === convId)
    if (existing) {
      const detail = selectedConv.value?.id === convId
        ? selectedConv.value
        : await conversationService.getById(convId)
      if (detail) {
        existing.status     = detail.status
        existing.attendants = detail.attendants
      }
    } else {
      const detail = await conversationService.getById(convId)
      if (!detail) return
      conversations.value.unshift({
        id:            detail.id,
        clientId:      detail.clientId,
        clientName:    detail.clientName,
        clientEmail:   detail.clientEmail,
        lastMessage:   detail.messages.length > 0 ? detail.messages[detail.messages.length - 1].content : null,
        lastMessageAt: detail.messages.length > 0 ? detail.messages[detail.messages.length - 1].createdAt : null,
        messageCount:  detail.messages.length,
        createdAt:     detail.createdAt,
        finishedAt:    detail.finishedAt,
        status:        detail.status,
        attendants:    detail.attendants,
      })
    }
  })

  // Online/Offline tracking
  chatService.onUserOnline(({ userId }) => {
    onlineUserIds.value = new Set([...onlineUserIds.value, userId])
  })
  chatService.onUserOffline(({ userId }) => {
    const next = new Set(onlineUserIds.value)
    next.delete(userId)
    onlineUserIds.value = next
  })

  // Reconexão
  chatService.onReconnecting(() => { wsStatus.value = 'reconnecting' })
  chatService.onReconnected(async () => {
    wsStatus.value = 'connected'
    for (const conv of conversations.value) {
      await chatService.joinConversation(conv.id)
    }
  })
  chatService.onClose(() => { wsStatus.value = 'disconnected' })
}

// ── Detail modal ─────────────────────────────────────────────────────────────
async function openDetail(convId: number) {
  showDetail.value   = true
  loadingDetail.value = true
  selectedConv.value  = null
  try {
    selectedConv.value = await conversationService.getById(convId)
    await nextTick()
    scrollBottom()
  } catch {
    toastStore.error('Erro ao abrir conversa')
  } finally {
    loadingDetail.value = false
  }
}

function closeDetail() {
  showDetail.value  = false
  selectedConv.value = null
}

// ── Invite modal ─────────────────────────────────────────────────────────────
async function openInviteModal() {
  showInviteModal.value = true
  if (allUsers.value.length > 0) return
  loadingUsers.value = true
  try {
    allUsers.value = await usersService.getAll()
  } catch {
    toastStore.error('Erro ao carregar atendentes')
  } finally {
    loadingUsers.value = false
  }
}

async function doInvite(user: User) {
  if (!selectedConv.value) return
  try {
    await conversationService.invite(selectedConv.value.id, user.id)
    const already = selectedConv.value.attendants.some(a => a.userId === user.id)
    if (!already) {
      selectedConv.value.attendants.push({ userId: user.id, name: user.name, avatarUrl: user.avatarUrl })
    }
    // Atualiza lista principal também
    const item = conversations.value.find(c => c.id === selectedConv.value!.id)
    if (item) item.status = 'active'
    toastStore.success(`${user.name} adicionado à conversa`)
  } catch {
    toastStore.error('Erro ao convidar atendente')
  }
}

// ── Helpers ──────────────────────────────────────────────────────────────────
function scrollBottom() {
  nextTick(() => { if (messagesEl.value) messagesEl.value.scrollTop = messagesEl.value.scrollHeight })
}
function duration(d: string) {
  const mins = Math.floor((Date.now() - new Date(d).getTime()) / 60000)
  if (mins < 1) return 'agora'
  if (mins < 60) return `${mins}min`
  const h = Math.floor(mins / 60)
  return `${h}h${mins % 60}min`
}
function isAgentMsg(msg: ConversationMessage) { return msg.userId !== null }
</script>

<template>
  <div class="space-y-6">

    <!-- ═══════ Header ═══════ -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Monitoramento</h1>
        <p class="text-sm text-gray-500 dark:text-gray-400">Acompanhe todas as conversas em tempo real</p>
      </div>
      <div class="flex items-center gap-3 text-xs">
        <span v-if="wsStatus === 'connected'" class="flex items-center gap-1.5 text-green-500 font-medium">
          <span class="w-2 h-2 bg-green-500 rounded-full animate-pulse"></span>
          Tempo real ativo
        </span>
        <span v-else-if="wsStatus === 'reconnecting'" class="flex items-center gap-1.5 text-yellow-400 font-medium">
          <span class="w-2 h-2 bg-yellow-400 rounded-full animate-ping"></span>
          Reconectando...
        </span>
        <span v-else class="flex items-center gap-1.5 text-gray-400">
          <span class="w-2 h-2 bg-gray-400 rounded-full"></span>
          Offline
        </span>
        <button @click="loadConversations" class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-500 transition" title="Recarregar">
          <i class="fas fa-sync-alt"></i>
        </button>
      </div>
    </div>

    <!-- ═══════ Stats cards ═══════ -->
    <!-- Conversations stats -->
    <div v-if="activeTab === 'conversations'" class="grid grid-cols-1 md:grid-cols-4 gap-4">
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-purple-100 dark:bg-purple-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-comments text-purple-600 dark:text-purple-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Conversas ativas</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ totalActive }}</p>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-yellow-100 dark:bg-yellow-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-clock text-yellow-600 dark:text-yellow-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Aguardando atendente</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ waitingCount }}</p>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-green-100 dark:bg-green-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-headset text-green-600 dark:text-green-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Em atendimento</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ activeCount }}</p>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-comment-dots text-blue-600 dark:text-blue-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Total de mensagens</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ totalMsgs }}</p>
        </div>
      </div>
    </div>
    <!-- Operators stats -->
    <div v-else class="grid grid-cols-1 md:grid-cols-4 gap-4">
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-purple-100 dark:bg-purple-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-users text-purple-600 dark:text-purple-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Total de operadores</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ operators.filter(u => !u.deletedAt).length }}</p>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-green-100 dark:bg-green-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-circle text-green-500 dark:text-green-400 text-xs"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Online agora</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ onlineOperatorsCount }}</p>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-headset text-blue-600 dark:text-blue-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Atendendo agora</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ busyOperatorsCount }}</p>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-xl border border-gray-200 dark:border-gray-700 flex items-center gap-4">
        <div class="w-10 h-10 bg-yellow-100 dark:bg-yellow-900/20 rounded-lg flex items-center justify-center">
          <i class="fas fa-comments text-yellow-600 dark:text-yellow-400"></i>
        </div>
        <div>
          <p class="text-xs text-gray-500 dark:text-gray-400">Conversas ativas</p>
          <p class="text-xl font-bold text-gray-900 dark:text-white">{{ totalActive }}</p>
        </div>
      </div>
    </div>

    <!-- ═══════ Tabs ═══════ -->
    <div class="flex border-b border-gray-200 dark:border-gray-700">
      <button
        @click="activeTab = 'conversations'"
        :class="[
          'px-5 py-2.5 text-sm font-medium transition border-b-2 -mb-px',
          activeTab === 'conversations'
            ? 'border-purple-500 text-purple-600 dark:text-purple-400'
            : 'border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400'
        ]"
      >
        <i class="fas fa-comments mr-1.5"></i>Conversas
        <span class="ml-1.5 bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 text-xs px-1.5 py-0.5 rounded-full">{{ totalActive }}</span>
      </button>
      <button
        @click="activeTab = 'operators'"
        :class="[
          'px-5 py-2.5 text-sm font-medium transition border-b-2 -mb-px',
          activeTab === 'operators'
            ? 'border-purple-500 text-purple-600 dark:text-purple-400'
            : 'border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400'
        ]"
      >
        <i class="fas fa-user-tie mr-1.5"></i>Operadores
        <span class="ml-1.5 bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 text-xs px-1.5 py-0.5 rounded-full">{{ onlineOperatorsCount }}</span>
      </button>
    </div>

    <!-- ═══════ Conversations list ═══════ -->
    <div v-if="activeTab === 'conversations'" class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">

      <!-- Search + filter -->
      <div class="px-4 py-3 border-b border-gray-200 dark:border-gray-700 flex flex-col sm:flex-row items-start sm:items-center gap-3">
        <div class="relative flex-1 max-w-xs">
          <i class="fas fa-search absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
          <input
            v-model="search"
            type="text"
            placeholder="Buscar cliente, email, ID..."
            class="w-full pl-8 pr-3 py-2 text-sm bg-gray-100 dark:bg-gray-700 rounded-lg outline-none focus:ring-2 focus:ring-purple-500 text-gray-900 dark:text-white placeholder-gray-400"
          />
        </div>
        <div class="flex items-center gap-2">
          <button
            v-for="opt in ([
              { key: 'all', label: 'Todas' },
              { key: 'waiting', label: 'Aguardando' },
              { key: 'active', label: 'Em atendimento' },
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
        <span class="text-xs text-gray-400 ml-auto">{{ filtered.length }} conversa(s)</span>
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

      <!-- Empty -->
      <div v-else-if="filtered.length === 0" class="p-12 text-center">
        <i class="fas fa-satellite-dish text-4xl text-gray-300 dark:text-gray-600 mb-3 block"></i>
        <p class="text-sm text-gray-400 dark:text-gray-500">Nenhuma conversa ativa no momento</p>
      </div>

      <!-- Rows -->
      <div v-else class="divide-y divide-gray-100 dark:divide-gray-700">
        <div
          v-for="conv in filtered"
          :key="conv.id"
          class="flex items-center gap-4 px-4 py-3 hover:bg-gray-50 dark:hover:bg-gray-700/30 transition cursor-pointer"
          @click="openDetail(conv.id)"
        >
          <!-- Avatar -->
          <div class="relative flex-shrink-0">
            <div class="w-9 h-9 bg-purple-100 dark:bg-purple-900/30 rounded-full flex items-center justify-center">
              <span class="text-xs font-bold text-purple-600 dark:text-purple-400">{{ initials(conv.clientName) }}</span>
            </div>
          </div>

          <!-- Info -->
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-0.5">
              <p class="text-sm font-medium text-gray-900 dark:text-white truncate">{{ conv.clientName }}</p>
              <span :class="[
                'text-xs px-1.5 py-0.5 rounded-full flex-shrink-0 font-medium',
                conv.status === 'active'
                  ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                  : 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400'
              ]">
                {{ conv.status === 'active' ? 'Ativo' : 'Aguardando' }}
              </span>
            </div>
            <p class="text-xs text-gray-500 dark:text-gray-400 truncate">{{ conv.lastMessage || 'Sem mensagens' }}</p>
          </div>

          <!-- Attendants -->
          <div v-if="conv.attendants && conv.attendants.length > 0" class="hidden md:flex items-center -space-x-2 flex-shrink-0">
            <div
              v-for="att in conv.attendants.slice(0, 3)"
              :key="att.userId"
              :title="att.name"
              class="w-6 h-6 rounded-full bg-indigo-500 border-2 border-white dark:border-gray-800 flex items-center justify-center"
            >
              <img v-if="att.avatarUrl" :src="att.avatarUrl" :alt="att.name" class="w-full h-full rounded-full object-cover" />
              <span v-else class="text-[9px] font-bold text-white leading-none">{{ att.name[0].toUpperCase() }}</span>
            </div>
            <span v-if="conv.attendants.length > 3" class="text-[10px] text-gray-400 ml-1">+{{ conv.attendants.length - 3 }}</span>
          </div>

          <!-- Meta -->
          <div class="hidden md:flex items-center gap-6 text-xs text-gray-500 dark:text-gray-400 flex-shrink-0">
            <div title="Mensagens">
              <i class="fas fa-comment-dots mr-1"></i>{{ conv.messageCount }}
            </div>
            <div class="w-16 text-right" title="Duração">
              <i class="fas fa-clock mr-1"></i>{{ duration(conv.createdAt) }}
            </div>
          </div>

          <!-- Arrow -->
          <i class="fas fa-chevron-right text-gray-300 dark:text-gray-600 text-xs flex-shrink-0"></i>
        </div>
      </div>
    </div>

    <!-- ═══════ Operators list ═══════ -->
    <div v-if="activeTab === 'operators'" class="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden">

      <!-- Search + filter -->
      <div class="px-4 py-3 border-b border-gray-200 dark:border-gray-700 flex flex-col sm:flex-row items-start sm:items-center gap-3">
        <div class="relative flex-1 max-w-xs">
          <i class="fas fa-search absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 text-sm"></i>
          <input
            v-model="operatorSearch"
            type="text"
            placeholder="Buscar nome, username, email..."
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
            @click="operatorFilter = opt.key"
            :class="[
              'px-3 py-1.5 text-xs rounded-full transition font-medium',
              operatorFilter === opt.key
                ? 'bg-purple-600 text-white'
                : 'bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
            ]"
          >
            {{ opt.label }}
          </button>
        </div>
        <span class="text-xs text-gray-400 ml-auto">{{ filteredOperators.length }} operador(es)</span>
      </div>

      <!-- Skeleton -->
      <div v-if="loadingOperators" class="p-6 space-y-4">
        <div v-for="i in 4" :key="i" class="flex gap-4 animate-pulse">
          <div class="w-10 h-10 bg-gray-200 dark:bg-gray-700 rounded-full"></div>
          <div class="flex-1 space-y-2 pt-1">
            <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-1/4"></div>
            <div class="h-2 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
          </div>
          <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-20"></div>
        </div>
      </div>

      <!-- Empty -->
      <div v-else-if="filteredOperators.length === 0" class="p-12 text-center">
        <i class="fas fa-user-slash text-4xl text-gray-300 dark:text-gray-600 mb-3 block"></i>
        <p class="text-sm text-gray-400 dark:text-gray-500">Nenhum operador encontrado</p>
      </div>

      <!-- Rows -->
      <div v-else class="divide-y divide-gray-100 dark:divide-gray-700">
        <div
          v-for="user in filteredOperators"
          :key="user.id"
          class="flex items-center gap-4 px-4 py-3 hover:bg-gray-50 dark:hover:bg-gray-700/30 transition"
        >
          <!-- Avatar + online dot -->
          <div class="relative flex-shrink-0">
            <div class="w-10 h-10 bg-indigo-100 dark:bg-indigo-900/30 rounded-full flex items-center justify-center">
              <img v-if="user.avatarUrl" :src="user.avatarUrl" :alt="user.name" class="w-full h-full rounded-full object-cover" />
              <span v-else class="text-sm font-bold text-indigo-600 dark:text-indigo-400">{{ initials(user.name) }}</span>
            </div>
            <span
              :class="[
                'absolute -bottom-0.5 -right-0.5 w-3 h-3 border-2 border-white dark:border-gray-800 rounded-full',
                onlineUserIds.has(user.id) ? 'bg-green-500' : 'bg-gray-400'
              ]"
            ></span>
          </div>

          <!-- Info -->
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-0.5">
              <p class="text-sm font-medium text-gray-900 dark:text-white truncate">{{ user.name }}</p>
              <span :class="[
                'text-xs px-1.5 py-0.5 rounded-full font-medium',
                onlineUserIds.has(user.id)
                  ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                  : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'
              ]">
                {{ onlineUserIds.has(user.id) ? 'Online' : 'Offline' }}
              </span>
            </div>
            <p class="text-xs text-gray-500 dark:text-gray-400 truncate">{{ user.email }} &middot; @{{ user.username }}</p>
          </div>

          <!-- Permission -->
          <div class="hidden md:block flex-shrink-0">
            <span class="text-xs px-2 py-1 rounded-full bg-purple-100 dark:bg-purple-900/20 text-purple-700 dark:text-purple-400 font-medium">
              {{ user.permissionName }}
            </span>
          </div>

          <!-- Chat count -->
          <div class="flex-shrink-0 text-center min-w-[80px]">
            <div v-if="(chatCountByUser.get(user.id) ?? 0) > 0" class="flex items-center gap-1.5 text-sm font-semibold text-blue-600 dark:text-blue-400">
              <i class="fas fa-headset text-xs"></i>
              {{ chatCountByUser.get(user.id) }} chat{{ (chatCountByUser.get(user.id) ?? 0) > 1 ? 's' : '' }}
            </div>
            <div v-else class="text-xs text-gray-400 dark:text-gray-500">
              Livre
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>

  <!-- ═══════════ Modal: Detalhe da Conversa (read-only) ═══════════ -->
  <Teleport to="body">
    <div v-if="showDetail" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/60" @click="closeDetail"></div>

      <div class="relative bg-white dark:bg-gray-800 rounded-2xl shadow-2xl w-full max-w-2xl max-h-[85vh] flex flex-col overflow-hidden">

        <!-- Header -->
        <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700 flex-shrink-0">
          <div v-if="selectedConv" class="flex items-center gap-3 min-w-0">
            <div class="w-10 h-10 bg-purple-100 dark:bg-purple-900/30 rounded-full flex items-center justify-center flex-shrink-0">
              <span class="text-sm font-bold text-purple-600 dark:text-purple-400">{{ initials(selectedConv.clientName) }}</span>
            </div>
            <div class="min-w-0">
              <h2 class="text-lg font-bold text-gray-900 dark:text-white truncate">{{ selectedConv.clientName }}</h2>
              <p class="text-xs text-gray-500 dark:text-gray-400">
                {{ selectedConv.clientEmail || selectedConv.clientPhone || '' }}
                &middot; Conversa #{{ selectedConv.id }}
                &middot;
                <span :class="selectedConv.status === 'active' ? 'text-green-500' : selectedConv.status === 'waiting' ? 'text-yellow-500' : 'text-gray-400'">
                  {{ selectedConv.status === 'active' ? 'Ativo' : selectedConv.status === 'waiting' ? 'Aguardando' : 'Finalizado' }}
                </span>
              </p>
            </div>
          </div>
          <div class="flex items-center gap-2 flex-shrink-0">
            <!-- Atendentes -->
            <div v-if="selectedConv && selectedConv.attendants.length > 0" class="flex items-center -space-x-2 mr-2">
              <div
                v-for="att in selectedConv.attendants"
                :key="att.userId"
                :title="att.name"
                class="w-7 h-7 rounded-full bg-indigo-500 border-2 border-white dark:border-gray-800 flex items-center justify-center"
              >
                <img v-if="att.avatarUrl" :src="att.avatarUrl" :alt="att.name" class="w-full h-full rounded-full object-cover" />
                <span v-else class="text-[10px] font-bold text-white leading-none">{{ att.name[0].toUpperCase() }}</span>
              </div>
            </div>
            <!-- Chamar atendente -->
            <button
              v-if="selectedConv && selectedConv.status !== 'finished'"
              @click="openInviteModal"
              class="px-2.5 py-1.5 text-xs bg-indigo-500 hover:bg-indigo-600 text-white rounded-lg transition flex items-center gap-1.5 font-medium"
              title="Chamar atendente"
            >
              <i class="fas fa-user-plus"></i>
              <span class="hidden sm:inline">Chamar</span>
            </button>
            <button @click="closeDetail" class="p-1.5 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-400 transition">
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>

        <!-- Loading -->
        <div v-if="loadingDetail" class="flex-1 flex items-center justify-center py-20">
          <i class="fas fa-spinner fa-spin text-purple-500 text-2xl"></i>
        </div>

        <!-- Messages (read-only) -->
        <div v-else-if="selectedConv" ref="messagesEl" class="flex-1 overflow-y-auto p-4 space-y-2 min-h-0">
          <!-- Info row -->
          <div class="flex flex-wrap gap-4 mb-4 p-3 bg-gray-50 dark:bg-gray-700/40 rounded-xl text-xs text-gray-500 dark:text-gray-400">
            <div><i class="fas fa-calendar mr-1"></i>Início: {{ formatDate(selectedConv.createdAt) }}</div>
            <div><i class="fas fa-comment-dots mr-1"></i>{{ selectedConv.messages.length }} mensagens</div>
            <div><i class="fas fa-clock mr-1"></i>Duração: {{ duration(selectedConv.createdAt) }}</div>
            <div v-if="selectedConv.attendants.length > 0">
              <i class="fas fa-headset mr-1"></i>
              {{ selectedConv.attendants.map(a => a.name).join(', ') }}
            </div>
          </div>

          <div v-if="selectedConv.messages.length === 0" class="text-center text-sm text-gray-400 dark:text-gray-500 mt-10">
            <i class="fas fa-comment-dots text-2xl mb-2 block opacity-30"></i>
            Nenhuma mensagem ainda
          </div>
          <div
            v-for="msg in selectedConv.messages"
            :key="msg.id"
            :class="['flex', isAgentMsg(msg) ? 'justify-end' : 'justify-start']"
          >
            <div :class="[
              'max-w-xs lg:max-w-sm px-3 py-2 rounded-2xl text-sm shadow-sm',
              isAgentMsg(msg)
                ? 'bg-purple-600 text-white rounded-br-sm'
                : 'bg-gray-100 dark:bg-gray-700 text-gray-900 dark:text-white rounded-bl-sm',
            ]">
              <p v-if="msg.senderName" :class="['text-[11px] font-semibold mb-0.5', isAgentMsg(msg) ? 'text-purple-200' : 'text-purple-500 dark:text-purple-400']">
                {{ msg.senderName }}
              </p>
              <p class="break-words leading-relaxed whitespace-pre-wrap">{{ msg.content }}</p>
              <p :class="['text-xs mt-1 text-right', isAgentMsg(msg) ? 'text-purple-200' : 'text-gray-400']">
                {{ formatTime(msg.createdAt) }}
              </p>
            </div>
          </div>
        </div>

        <!-- Footer info -->
        <div v-if="selectedConv" class="flex-shrink-0 px-6 py-3 border-t border-gray-200 dark:border-gray-700 text-center text-xs text-gray-400 dark:text-gray-500">
          <i class="fas fa-eye mr-1"></i> Modo de monitoramento — somente leitura
        </div>
      </div>
    </div>
  </Teleport>

  <!-- ═══════════ Modal: Chamar Atendente ═══════════ -->
  <Teleport to="body">
    <div
      v-if="showInviteModal"
      class="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 backdrop-blur-sm"
      @click.self="showInviteModal = false"
    >
      <div class="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl w-full max-w-sm mx-4 overflow-hidden">
        <!-- Header -->
        <div class="flex items-center justify-between px-5 py-4 border-b border-gray-200 dark:border-gray-700">
          <h3 class="text-base font-semibold text-gray-900 dark:text-white">
            <i class="fas fa-user-plus mr-2 text-indigo-500"></i>Chamar atendente
          </h3>
          <button @click="showInviteModal = false" class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <!-- Já na conversa -->
        <div v-if="detailAttendants.length > 0" class="px-5 pt-4">
          <p class="text-xs text-gray-500 dark:text-gray-400 uppercase font-semibold mb-2">Na conversa</p>
          <div class="flex flex-wrap gap-2 mb-3">
            <span
              v-for="att in detailAttendants"
              :key="att.userId"
              class="flex items-center gap-1.5 text-xs bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300 px-2.5 py-1 rounded-full"
            >
              <i class="fas fa-check-circle"></i>{{ att.name }}
            </span>
          </div>
        </div>

        <!-- Lista para convidar -->
        <div class="px-5 py-3 max-h-72 overflow-y-auto">
          <p class="text-xs text-gray-500 dark:text-gray-400 uppercase font-semibold mb-2">Convidar</p>
          <div v-if="loadingUsers" class="text-center py-6 text-gray-400">
            <i class="fas fa-spinner fa-spin text-xl"></i>
          </div>
          <div v-else-if="invitableUsers.length === 0" class="text-center py-6 text-sm text-gray-400">
            Todos os atendentes já estão na conversa
          </div>
          <button
            v-else
            v-for="user in invitableUsers"
            :key="user.id"
            @click="doInvite(user)"
            class="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl hover:bg-gray-100 dark:hover:bg-gray-700 transition text-left"
          >
            <div class="w-8 h-8 rounded-full bg-indigo-500 flex items-center justify-center flex-shrink-0">
              <img v-if="user.avatarUrl" :src="user.avatarUrl" class="w-full h-full rounded-full object-cover" />
              <span v-else class="text-xs font-bold text-white">{{ user.name[0].toUpperCase() }}</span>
            </div>
            <div>
              <p class="text-sm font-medium text-gray-900 dark:text-white">{{ user.name }}</p>
              <p class="text-xs text-gray-400">{{ user.username }}</p>
            </div>
          </button>
        </div>

        <div class="px-5 py-3 border-t border-gray-200 dark:border-gray-700 flex justify-end">
          <button @click="showInviteModal = false" class="px-4 py-2 text-sm bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-600 transition">
            Fechar
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>