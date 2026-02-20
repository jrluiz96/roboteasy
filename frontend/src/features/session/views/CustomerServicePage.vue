<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import {
  conversationService,
  type ConversationListItem,
  type ConversationDetail,
  type ConversationMessage,
  type ConversationAttendant,
} from '@/services/conversation.service'
import { chatService } from '@/services/chat.service'
import { api } from '@/services/api'
import { usersService, type User } from '@/services/users.service'
import { useAuthStore } from '@/features/auth/stores/authStore'
import { useToastStore } from '@/stores/toastStore'

const toastStore = useToastStore()
const authStore  = useAuthStore()

// State
const activeTab     = ref<'chats' | 'history'>('chats')
const conversations = ref<ConversationListItem[]>([])
const history       = ref<ConversationListItem[]>([])
const selectedId    = ref<number | null>(null)
const conversation  = ref<ConversationDetail | null>(null)
const messageInput  = ref('')
const messagesEl    = ref<HTMLElement | null>(null)
const loadingList   = ref(false)
const loadingChat   = ref(false)
const sending       = ref(false)
const wsStatus      = ref<'connected' | 'reconnecting' | 'disconnected'>('disconnected')
// Mensagens não lidas por conversa (conversas que não estão selecionadas)
const unreadCounts  = ref<Record<number, number>>({})
// Modal de convite de atendente
const showInviteModal = ref(false)
const allUsers        = ref<User[]>([])
const loadingUsers    = ref(false)
const pullingId       = ref<number | null>(null) // id em processo de "puxar"

const visibleList = computed(() =>
  activeTab.value === 'chats' ? conversations.value : history.value
)

// Atendentes já na conversa para filtrar no modal
const currentAttendants = computed(() => conversation.value?.attendants ?? [])

// True quando o operador logado já entrou (puxou) esta conversa
const isMember = computed(() =>
  currentAttendants.value.some(a => a.userId === authStore.currentUser?.id)
)
// Usuários que ainda não estão na conversa (para o modal de convite)
const invitableUsers = computed(() => {
  const linked = new Set(currentAttendants.value.map(a => a.userId))
  return allUsers.value.filter(u => !linked.has(u.id))
})

// Lifecycle
onMounted(async () => {
  await loadLists()
  await connectWs()
  if (wsStatus.value === 'connected') {
    // Entra em todos os grupos para receber notificações de qualquer conversa
    await joinAllConversations()
    setupGlobalMessageListener()
  }
})

onUnmounted(async () => {
  await chatService.disconnect()
})

// Data
async function loadLists() {
  loadingList.value = true
  try {
    ;[conversations.value, history.value] = await Promise.all([
      conversationService.getActive(),
      conversationService.getHistory(),
    ])
  } catch {
    toastStore.error('Erro ao carregar conversas')
  } finally {
    loadingList.value = false
  }
}

async function joinAllConversations() {
  for (const conv of conversations.value) {
    await chatService.joinConversation(conv.id)
  }
}

// Listener único global — registrado uma vez, trata mensagens de TODAS as conversas
function setupGlobalMessageListener() {
  chatService.onMessage((msg: ConversationMessage) => {
    // Atualiza preview e status na lista
    const item = conversations.value.find(c => c.id === msg.conversationId)
    if (item) {
      item.lastMessage    = msg.content
      item.lastMessageAt  = msg.createdAt
      if (msg.clientId !== null) {
        // Cliente enviou → só marca "aguardando" se ainda não há atendente vinculado
        if (item.status !== 'active') item.status = 'waiting'
      } else if (msg.userId !== null) {
        // Atendente respondeu → em atendimento
        item.status = 'active'
      }
    }

    if (msg.conversationId === selectedId.value && conversation.value) {
      // Conversa aberta: adiciona mensagem e rola
      const exists = conversation.value.messages.some(m => m.id === msg.id)
      if (!exists) conversation.value.messages.push(msg)
      if (msg.userId !== null) conversation.value.status = 'active'
      scrollBottom()
    } else if (msg.clientId !== null) {
      // Outra conversa: incrementa badge de não lidas
      unreadCounts.value[msg.conversationId] =
        (unreadCounts.value[msg.conversationId] ?? 0) + 1
    }
  })

  chatService.onAttendantLeft(({ conversationId: convId, userId: uid, userName }) => {
    // Remove da barra de atendentes
    if (conversation.value && conversation.value.id === convId) {
      conversation.value.attendants = conversation.value.attendants.filter(a => a.userId !== uid)
    }
    // Injeta mensagem de sistema na conversa aberta
    if (selectedId.value === convId && conversation.value) {
      pushSystemMessage(convId, `${userName} saiu da conversa`)
    }
  })

  chatService.onConversationInvited(async ({ conversationId: convId }) => {
    // Entra no grupo SignalR para receber mensagens em tempo real
    await chatService.joinConversation(convId)
    // Busca o detalhe e monta um item da lista
    const detail = await conversationService.getById(convId)
    if (!detail) return
    // Evita duplicatas
    if (conversations.value.some(c => c.id === convId)) return
    conversations.value.unshift({
      id:            detail.id,
      clientId:      detail.clientId,
      clientName:    detail.clientName,
      clientEmail:   detail.clientEmail,
      lastMessage:   detail.messages.at(-1)?.content ?? null,
      lastMessageAt: detail.messages.at(-1)?.createdAt ?? null,
      messageCount:  detail.messages.length,
      createdAt:     detail.createdAt,
      finishedAt:    detail.finishedAt,
      status:        detail.status,
    })
  })

  chatService.onConversationCreated(async (payload) => {
    // Evita duplicatas
    if (conversations.value.some(c => c.id === payload.id)) return
    // Entra no grupo para monitorar em tempo real
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
    })
  })
}

async function selectConversation(id: number) {
  if (selectedId.value === id) return
  loadingChat.value = true
  selectedId.value = id
  // Limpa badge de não lidas desta conversa
  unreadCounts.value[id] = 0
  conversation.value = null
  try {
    conversation.value = await conversationService.getById(id)
    await nextTick()
    scrollBottom()
  } catch {
    toastStore.error('Erro ao abrir conversa')
  } finally {
    loadingChat.value = false
  }
}

// Messaging
async function sendMessage() {
  const content = messageInput.value.trim()
  if (!content || !selectedId.value || sending.value || !isMember.value) return
  sending.value = true
  messageInput.value = ''
  try {
    await chatService.sendMessage(selectedId.value, content)
    // Marca localmente como ativo ao responder (o listener confirmará via WS)
    const item = conversations.value.find(c => c.id === selectedId.value)
    if (item) item.status = 'active'
    if (conversation.value) conversation.value.status = 'active'
  } catch {
    toastStore.error('Erro ao enviar mensagem')
    messageInput.value = content
  } finally {
    sending.value = false
  }
}

// Pull (puxar conversa para si)
async function pullConversation(id: number, e: Event) {
  e.stopPropagation() // evita também disparar selectConversation no item
  pullingId.value = id
  try {
    await conversationService.join(id)
    // Entra no grupo SignalR se ainda não entrou
    if (wsStatus.value === 'connected') await chatService.joinConversation(id)
    // Atualiza status local
    const item = conversations.value.find(c => c.id === id)
    if (item) item.status = 'active'
    await selectConversation(id)
    toastStore.success('Conversa puxada com sucesso')
  } catch {
    toastStore.error('Erro ao puxar conversa')
  } finally {
    pullingId.value = null
  }
}

// Modal de convite
async function openInviteModal() {
  showInviteModal.value = true
  if (allUsers.value.length > 0) return
  loadingUsers.value = true
  try {
    const res = await usersService.getAll()
    allUsers.value = res
  } catch {
    toastStore.error('Erro ao carregar atendentes')
  } finally {
    loadingUsers.value = false
  }
}

async function doInvite(attendant: User) {
  if (!selectedId.value || !conversation.value) return
  try {
    await conversationService.invite(selectedId.value, attendant.id)
    // Adiciona localmente para feedback imediato
    const already = conversation.value.attendants.some(a => a.userId === attendant.id)
    if (!already) {
      conversation.value.attendants.push({ userId: attendant.id, name: attendant.name, avatarUrl: attendant.avatarUrl })
    }
    toastStore.success(`${attendant.name} adicionado à conversa`)
  } catch {
    toastStore.error('Erro ao convidar atendente')
  }
}

// Finish / Leave
// - Último atendente (ou sem atendente): finaliza toda a conversa
// - Múltiplos atendentes: apenas sai da conversa
async function finishConversation() {
  if (!selectedId.value || !conversation.value) return
  const isLastAttendant = conversation.value.attendants.length <= 1

  try {
    if (isLastAttendant) {
      // Finaliza a conversa inteira
      await conversationService.finish(selectedId.value)
      toastStore.success('Conversa finalizada')
      const idx = conversations.value.findIndex(c => c.id === selectedId.value)
      if (idx !== -1) {
        const finished = { ...conversations.value[idx], status: 'finished' as const, finishedAt: new Date().toISOString() }
        conversations.value.splice(idx, 1)
        history.value.unshift(finished)
      }
      conversation.value.status = 'finished'
      conversation.value.finishedAt = new Date().toISOString()
    } else {
      // Sai da conversa (outros atendentes continuam)
      await conversationService.leave(selectedId.value)
      toastStore.success('Você saiu da conversa')
      // Remove da lista (não é mais visível para este atendente)
      conversations.value = conversations.value.filter(c => c.id !== selectedId.value)
      selectedId.value = null
      conversation.value = null
    }
  } catch {
    toastStore.error('Erro ao finalizar')
  }
}

// SignalR
async function connectWs() {
  const token = api.getToken()
  if (!token) return
  try {
    await chatService.connectAsUser(token)
    wsStatus.value = 'connected'

    chatService.onReconnecting(() => {
      wsStatus.value = 'reconnecting'
    })
    chatService.onReconnected(async () => {
      wsStatus.value = 'connected'
      // Re-entra nos grupos após reconexão
      for (const conv of conversations.value) {
        await chatService.joinConversation(conv.id)
      }
    })
    chatService.onClose(() => {
      wsStatus.value = 'disconnected'
    })
  } catch {
    wsStatus.value = 'disconnected'
    toastStore.warning('WebSocket indisponivel - tempo real desativado')
  }
}

// Helpers
// Injeta mensagem de sistema local (não salva no banco, apenas UI)
function pushSystemMessage(convId: number, text: string) {
  if (!conversation.value || conversation.value.id !== convId) return
  conversation.value.messages.push({
    id: -Date.now(),          // id negativo → nunca conflita com ids reais
    conversationId: convId,
    userId: null,
    clientId: null,
    type: 99,                 // 99 = system (local only)
    content: text,
    fileUrl: null,
    createdAt: new Date().toISOString(),
  })
  scrollBottom()
}

function scrollBottom() {
  nextTick(() => { if (messagesEl.value) messagesEl.value.scrollTop = messagesEl.value.scrollHeight })
}
function isMyMsg(msg: ConversationMessage) { return msg.userId !== null }
function formatTime(d: string) {
  return new Date(d).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}
function formatPreview(d: string) {
  const now = new Date(); const dt = new Date(d)
  return dt.toDateString() === now.toDateString()
    ? formatTime(d)
    : dt.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' })
}
function initials(name: string) {
  return name.split(' ').map((s: string) => s[0]).slice(0, 2).join('').toUpperCase()
}
</script>

<template>
  <div class="flex flex-col gap-4" style="height: calc(100vh - 120px)">

    <!-- Header -->
    <div class="flex-shrink-0 flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Atendimento</h1>
        <p class="text-sm text-gray-500 dark:text-gray-400">Chat em tempo real com clientes</p>
      </div>
      <div class="flex items-center gap-3 text-xs">
        <span v-if="wsStatus === 'connected'" class="flex items-center gap-1.5 text-green-500 font-medium">
          <span class="w-2 h-2 bg-green-500 rounded-full animate-pulse"></span>
          WebSocket conectado
        </span>
        <span v-else-if="wsStatus === 'reconnecting'" class="flex items-center gap-1.5 text-yellow-400 font-medium">
          <span class="w-2 h-2 bg-yellow-400 rounded-full animate-ping"></span>
          Reconectando...
        </span>
        <span v-else class="flex items-center gap-1.5 text-gray-400">
          <span class="w-2 h-2 bg-gray-400 rounded-full"></span>
          Offline
        </span>
        <button @click="loadLists" class="p-1.5 rounded hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-500 transition" title="Recarregar">
          <i class="fas fa-sync-alt text-sm"></i>
        </button>
      </div>
    </div>

    <!-- Split Panel -->
    <div class="flex-1 flex bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden min-h-0">

      <!-- LEFT: Lista-->
      <div class="w-72 xl:w-80 flex-shrink-0 border-r border-gray-200 dark:border-gray-700 flex flex-col">

        <!-- Tabs -->
        <div class="flex flex-shrink-0 border-b border-gray-200 dark:border-gray-700">
          <button
            @click="activeTab = 'chats'"
            :class="[
              'flex-1 py-3 text-sm font-medium transition',
              activeTab === 'chats'
                ? 'border-b-2 border-purple-500 text-purple-600 dark:text-purple-400'
                : 'text-gray-500 hover:text-gray-700 dark:text-gray-400',
            ]"
          >
            <i class="fas fa-comments mr-1"></i>Chats
            <span class="ml-1 bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300 text-xs px-1.5 py-0.5 rounded-full">
              {{ conversations.length }}
            </span>
          </button>
          <button
            @click="activeTab = 'history'"
            :class="[
              'flex-1 py-3 text-sm font-medium transition',
              activeTab === 'history'
                ? 'border-b-2 border-purple-500 text-purple-600 dark:text-purple-400'
                : 'text-gray-500 hover:text-gray-700 dark:text-gray-400',
            ]"
          >
            <i class="fas fa-history mr-1"></i>Historico
          </button>
        </div>

        <!-- Lista de conversas -->
        <div class="flex-1 overflow-y-auto">
          <div v-if="loadingList" class="p-4 space-y-3">
            <div v-for="i in 4" :key="i" class="flex gap-3 animate-pulse">
              <div class="w-9 h-9 bg-gray-200 dark:bg-gray-700 rounded-full flex-shrink-0"></div>
              <div class="flex-1 space-y-2 pt-1">
                <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-2/3"></div>
                <div class="h-2 bg-gray-200 dark:bg-gray-700 rounded w-full"></div>
              </div>
            </div>
          </div>
          <div v-else-if="visibleList.length === 0" class="p-8 text-center">
            <i class="fas fa-inbox text-3xl text-gray-300 dark:text-gray-600 mb-3 block"></i>
            <p class="text-sm text-gray-400 dark:text-gray-500">Nenhuma conversa</p>
          </div>
          <button
            v-else
            v-for="conv in visibleList"
            :key="conv.id"
            @click="selectConversation(conv.id)"
            :class="[
              'w-full text-left px-4 py-3 border-b border-gray-100 dark:border-gray-700/60 transition cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700/40',
              selectedId === conv.id
                ? 'bg-purple-50 dark:bg-purple-900/20 border-l-[3px] border-l-purple-500 pl-[13px]'
                : unreadCounts[conv.id] > 0
                  ? 'bg-gray-700/60 border-l-[3px] border-l-red-500 pl-[13px]'
                  : '',
            ]"
          >
            <div class="flex items-center gap-3">
              <!-- Avatar com badge de não lidas -->
              <div class="relative flex-shrink-0">
                <div class="w-9 h-9 bg-purple-100 dark:bg-purple-900/30 rounded-full flex items-center justify-center">
                  <span class="text-xs font-bold text-purple-600 dark:text-purple-400">{{ initials(conv.clientName) }}</span>
                </div>
                <span
                  v-if="unreadCounts[conv.id] > 0"
                  class="absolute -top-1 -right-1 min-w-[16px] h-4 px-1 bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center leading-none"
                >
                  {{ unreadCounts[conv.id] > 9 ? '9+' : unreadCounts[conv.id] }}
                </span>
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center justify-between gap-1 mb-0.5">
                  <span :class="['text-sm font-medium truncate', unreadCounts[conv.id] > 0 ? 'text-white' : 'text-gray-900 dark:text-white']">{{ conv.clientName }}</span>
                  <span :class="[
                    'text-xs px-1.5 py-0.5 rounded-full flex-shrink-0 font-medium',
                    conv.status === 'active'   ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                    : conv.status === 'waiting' ? 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400'
                    :                             'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'
                  ]">
                    {{ conv.status === 'active' ? 'Ativo' : conv.status === 'waiting' ? 'Aguardando' : 'Finalizado' }}
                  </span>
                </div>
                <p :class="['text-xs truncate', unreadCounts[conv.id] > 0 ? 'text-gray-200 font-medium' : 'text-gray-500 dark:text-gray-400']">{{ conv.lastMessage || 'Sem mensagens' }}</p>
                <p class="text-xs text-gray-400 dark:text-gray-500 mt-0.5">
                  {{ conv.lastMessageAt ? formatPreview(conv.lastMessageAt) : formatPreview(conv.createdAt) }}
                </p>
              </div>
            </div>
            <!-- Botão Puxar — aparece para conversas que ainda não têm atendente -->
            <div v-if="conv.status === 'waiting' && activeTab === 'chats'" class="mt-2 flex justify-end">
              <button
                @click.stop="pullConversation(conv.id, $event)"
                :disabled="pullingId === conv.id"
                class="text-xs px-2.5 py-1 bg-purple-600 hover:bg-purple-700 disabled:opacity-50 text-white rounded-lg transition font-medium flex items-center gap-1"
              >
                <i :class="pullingId === conv.id ? 'fas fa-spinner fa-spin' : 'fas fa-hand-pointer'"></i>
                {{ pullingId === conv.id ? 'Puxando...' : 'Puxar' }}
              </button>
            </div>
          </button>
        </div>
      </div>

      <!-- RIGHT: Chat Panel -->
      <div class="flex-1 flex flex-col min-w-0">

        <!-- Placeholder -->
        <div v-if="!loadingChat && !conversation" class="flex-1 flex flex-col items-center justify-center gap-3 text-gray-400 dark:text-gray-500">
          <i class="fas fa-comments text-5xl opacity-20"></i>
          <p class="text-sm">Selecione uma conversa para comecar</p>
        </div>

        <!-- Loading -->
        <div v-else-if="loadingChat" class="flex-1 flex items-center justify-center">
          <i class="fas fa-spinner fa-spin text-purple-500 text-2xl"></i>
        </div>

        <!-- Chat -->
        <template v-else-if="conversation">

          <!-- Header do chat -->
          <div class="flex-shrink-0 px-4 py-3 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between gap-2">
            <div class="flex items-center gap-3 min-w-0">
              <div class="w-10 h-10 bg-purple-100 dark:bg-purple-900/30 rounded-full flex items-center justify-center flex-shrink-0">
                <span class="text-sm font-bold text-purple-600 dark:text-purple-400">{{ initials(conversation.clientName) }}</span>
              </div>
              <div class="min-w-0">
                <p class="text-sm font-semibold text-gray-900 dark:text-white truncate">{{ conversation.clientName }}</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">
                  {{ conversation.clientEmail || conversation.clientPhone || '' }}
                  &nbsp;&middot;&nbsp;
                  <span :class="conversation.status === 'finished' ? 'text-gray-400' : conversation.status === 'active' ? 'text-green-500' : 'text-yellow-500'">
                    {{ conversation.status === 'active' ? 'Ativo' : conversation.status === 'waiting' ? 'Aguardando' : 'Finalizado' }}
                  </span>
                </p>
              </div>
            </div>

            <div class="flex items-center gap-2 flex-shrink-0">
              <!-- Avatares dos atendentes vinculados -->
              <div v-if="conversation.attendants.length > 0" class="flex items-center -space-x-2">
                <div
                  v-for="att in conversation.attendants"
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
                v-if="conversation.status !== 'finished'"
                @click="openInviteModal"
                :disabled="!isMember"
                class="px-2.5 py-1.5 text-xs bg-indigo-500 hover:bg-indigo-600 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition flex items-center gap-1.5 font-medium"
                :title="isMember ? 'Chamar atendente' : 'Puxe a conversa primeiro'"
              >
                <i class="fas fa-user-plus"></i>
                <span class="hidden sm:inline">Chamar</span>
              </button>

              <!-- Finalizar / Sair -->
              <button
                v-if="conversation.status !== 'finished'"
                @click="finishConversation"
                :disabled="!isMember"
                :class="[
                  'px-3 py-1.5 text-xs text-white rounded-lg transition flex items-center gap-1.5 font-medium disabled:opacity-40 disabled:cursor-not-allowed',
                  conversation.attendants.length > 1
                    ? 'bg-orange-500 hover:bg-orange-600'
                    : 'bg-red-500 hover:bg-red-600'
                ]"
                :title="!isMember ? 'Puxe a conversa primeiro' : conversation.attendants.length > 1 ? 'Sair da conversa (outros atendentes permanecem)' : 'Finalizar conversa com o cliente'"
              >
                <i :class="conversation.attendants.length > 1 ? 'fas fa-sign-out-alt' : 'fas fa-times-circle'"></i>
                {{ conversation.attendants.length > 1 ? 'Sair' : 'Finalizar' }}
              </button>
              <span v-else class="text-xs text-gray-400 flex items-center gap-1">
                <i class="fas fa-lock"></i> Finalizada
              </span>
            </div>
          </div>

          <!-- Mensagens -->
          <div ref="messagesEl" class="flex-1 overflow-y-auto p-4 space-y-2">
            <div v-if="conversation.messages.length === 0" class="text-center text-sm text-gray-400 dark:text-gray-500 mt-10">
              <i class="fas fa-comment-dots text-2xl mb-2 block opacity-30"></i>
              Nenhuma mensagem ainda
            </div>
            <div
              v-for="msg in conversation.messages"
              :key="msg.id"
              :class="['flex', msg.type === 99 ? 'justify-center' : isMyMsg(msg) ? 'justify-end' : 'justify-start']"
            >
              <!-- Mensagem de sistema -->
              <div v-if="msg.type === 99" class="text-xs text-gray-400 dark:text-gray-500 italic bg-gray-100 dark:bg-gray-700/50 px-3 py-1 rounded-full">
                {{ msg.content }}
              </div>
              <!-- Mensagem normal -->
              <div v-else :class="[
                'max-w-xs lg:max-w-sm px-3 py-2 rounded-2xl text-sm shadow-sm',
                isMyMsg(msg)
                  ? 'bg-purple-600 text-white rounded-br-sm'
                  : 'bg-gray-100 dark:bg-gray-700 text-gray-900 dark:text-white rounded-bl-sm',
              ]">
                <p class="break-words leading-relaxed whitespace-pre-wrap">{{ msg.content }}</p>
                <p :class="['text-xs mt-1 text-right', isMyMsg(msg) ? 'text-purple-200' : 'text-gray-400']">
                  {{ formatTime(msg.createdAt) }}
                </p>
              </div>
            </div>
          </div>

          <!-- Input -->
          <div v-if="conversation.status !== 'finished' && isMember" class="flex-shrink-0 p-3 border-t border-gray-200 dark:border-gray-700 flex gap-2">
            <input
              v-model="messageInput"
              @keyup.enter="sendMessage"
              type="text"
              placeholder="Digite uma mensagem... (Enter para enviar)"
              class="flex-1 px-3 py-2 text-sm bg-gray-100 dark:bg-gray-700 rounded-xl border-0 outline-none focus:ring-2 focus:ring-purple-500 text-gray-900 dark:text-white placeholder-gray-400"
            />
            <button
              @click="sendMessage"
              :disabled="sending || !messageInput.trim()"
              class="px-4 py-2 bg-purple-600 hover:bg-purple-700 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-xl transition"
            >
              <i :class="sending ? 'fas fa-spinner fa-spin' : 'fas fa-paper-plane'"></i>
            </button>
          </div>
          <div v-else-if="conversation.status !== 'finished' && !isMember" class="flex-shrink-0 p-3 border-t border-gray-200 dark:border-gray-700 text-center text-xs text-gray-400 dark:text-gray-500">
            <i class="fas fa-hand-pointer mr-1"></i> Puxe a conversa para poder interagir
          </div>
          <div v-else class="flex-shrink-0 p-3 border-t border-gray-200 dark:border-gray-700 text-center text-xs text-gray-400 dark:text-gray-500">
            <i class="fas fa-lock mr-1"></i> Esta conversa foi encerrada
          </div>

        </template>
      </div>
    </div>
  </div>

  <!-- Modal: Chamar atendente -->
  <Teleport to="body">
    <div
      v-if="showInviteModal"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm"
      @click.self="showInviteModal = false"
    >
      <div class="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl w-full max-w-sm mx-4 overflow-hidden">
        <!-- Header modal -->
        <div class="flex items-center justify-between px-5 py-4 border-b border-gray-200 dark:border-gray-700">
          <h3 class="text-base font-semibold text-gray-900 dark:text-white">
            <i class="fas fa-user-plus mr-2 text-indigo-500"></i>Chamar atendente
          </h3>
          <button @click="showInviteModal = false" class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <!-- Atendentes já na conversa -->
        <div v-if="currentAttendants.length > 0" class="px-5 pt-4">
          <p class="text-xs text-gray-500 dark:text-gray-400 uppercase font-semibold mb-2">Na conversa</p>
          <div class="flex flex-wrap gap-2 mb-3">
            <span
              v-for="att in currentAttendants"
              :key="att.userId"
              class="flex items-center gap-1.5 text-xs bg-indigo-100 dark:bg-indigo-900/30 text-indigo-700 dark:text-indigo-300 px-2.5 py-1 rounded-full"
            >
              <i class="fas fa-check-circle"></i>{{ att.name }}
            </span>
          </div>
        </div>

        <!-- Lista de usuários para convidar -->
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