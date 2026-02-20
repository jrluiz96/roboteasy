<script setup lang="ts">
import { ref, nextTick, onMounted, onUnmounted } from 'vue'
import { chatService, type ChatMessage } from '@/services/chat.service'

// --- Tipos ---
interface DisplayMessage {
  from: 'client' | 'agent'
  text: string
  time: string
}

interface ChatSession {
  clientToken: string
  conversationId: number
  clientName: string
  clientEmail: string
}

const STORAGE_KEY = 'chat_session'

// --- State ---
const chatOpen = ref(false)
const chatStep = ref<'form' | 'chat'>('form')
const submitting = ref(false)
const errorMsg = ref('')

const clientForm = ref({ name: '', email: '', phone: '', cpf: '' })
const conversationId = ref<number | null>(null)
const messages = ref<DisplayMessage[]>([])
const messageInput = ref('')
const messagesEnd = ref<HTMLElement | null>(null)
const conversationClosed = ref(false)

// --- Helpers ---
function now() {
  return new Date().toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
}

function isValidEmail(email: string) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
}

function scrollToBottom() {
  nextTick(() => messagesEnd.value?.scrollIntoView({ behavior: 'smooth' }))
}

function toDisplayMsg(m: ChatMessage): DisplayMessage {
  return {
    from: m.clientId != null ? 'client' : 'agent',
    text: m.content,
    time: new Date(m.createdAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })
  }
}

function saveSession(session: ChatSession) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(session))
}

function loadSession(): ChatSession | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? (JSON.parse(raw) as ChatSession) : null
  } catch {
    return null
  }
}

function clearSession() {
  localStorage.removeItem(STORAGE_KEY)
}

// Registra o listener de mensagens recebidas (atendente → cliente)
function registerMessageListener() {
  chatService.onMessage((msg: ChatMessage) => {
    if (msg.conversationId !== conversationId.value) return
    if (msg.clientId != null) return // eco da própria mensagem do cliente
    messages.value.push({ from: 'agent', text: msg.content, time: now() })
    scrollToBottom()
  })

  chatService.onConversationFinished(({ conversationId: closedId }) => {
    if (closedId !== conversationId.value) return
    conversationClosed.value = true
    messages.value.push({
      from: 'agent',
      text: 'O atendimento foi encerrado pelo operador. Obrigado pelo contato! 🙏',
      time: now()
    })
    scrollToBottom()
    clearSession()
  })
}

// Conecta o WebSocket e entra na sala — re-registra ao reconectar
async function connectToConversation(clientToken: string, convId: number) {
  await chatService.connectAsClient(clientToken)
  await chatService.joinConversation(convId)
  // Re-entra no grupo sempre que o SignalR reconectar automaticamente
  chatService.onReconnected(async () => {
    await chatService.joinConversation(convId)
  })
  registerMessageListener()
}

// --- Mount: reconecta se houver sessão salva ---
onMounted(async () => {
  const session = loadSession()
  if (!session) return // sem sessão → fica no formulário, nada a fazer

  try {
    clientForm.value.name  = session.clientName
    clientForm.value.email = session.clientEmail

    // Re-chama start() para obter histórico real + novo token + reconectar WS
    const result = await chatService.start({
      name:  session.clientName,
      email: session.clientEmail,
    })

    conversationId.value = result.conversationId

    if (result.messages.length > 0) {
      messages.value = result.messages.map(toDisplayMsg)
    } else {
      messages.value = [{
        from: 'agent',
        text: `Bem-vindo de volta, ${session.clientName}! 👋`,
        time: now()
      }]
    }

    // Persiste sessão atualizada (novo token)
    saveSession({
      clientToken: result.clientToken,
      conversationId: result.conversationId,
      clientName: session.clientName,
      clientEmail: session.clientEmail,
    })

    await connectToConversation(result.clientToken, result.conversationId)
    chatStep.value = 'chat'
    scrollToBottom()
  } catch {
    // token expirado ou erro → limpa sessão e volta ao formulário
    clearSession()
  }
})

// --- Actions ---
function toggleChat() {
  chatOpen.value = !chatOpen.value
}

async function submitForm() {
  if (!clientForm.value.name) return
  errorMsg.value = ''
  submitting.value = true

  try {
    const result = await chatService.start({
      name: clientForm.value.name,
      email: clientForm.value.email || undefined,
      phone: clientForm.value.phone || undefined,
      cpf: clientForm.value.cpf || undefined
    })

    conversationId.value = result.conversationId

    // Carrega histórico ou boas-vindas
    if (!result.isNewConversation && result.messages.length > 0) {
      messages.value = result.messages.map(toDisplayMsg)
    } else {
      messages.value = [{
        from: 'agent',
        text: `Olá, ${clientForm.value.name}! 👋 Um de nossos atendentes irá responder em breve!`,
        time: now()
      }]
    }

    // Persiste sessão para sobreviver ao F5
    saveSession({
      clientToken: result.clientToken,
      conversationId: result.conversationId,
      clientName: clientForm.value.name,
      clientEmail: clientForm.value.email,
    })

    await connectToConversation(result.clientToken, result.conversationId)

    chatStep.value = 'chat'
    scrollToBottom()
  } catch (e: any) {
    errorMsg.value = e.message || 'Erro ao conectar. Tente novamente.'
  } finally {
    submitting.value = false
  }
}

function endChat() {
  clearSession()
  chatService.disconnect()
  conversationId.value = null
  messages.value = []
  conversationClosed.value = false
  clientForm.value = { name: '', email: '', phone: '', cpf: '' }
  chatStep.value = 'form'
}

async function sendMessage() {
  const text = messageInput.value.trim()
  if (!text || conversationId.value === null) return

  messages.value.push({ from: 'client', text, time: now() })
  messageInput.value = ''
  scrollToBottom()

  try {
    await chatService.sendMessage(conversationId.value, text)
  } catch {
    // mensagem já aparece localmente
  }
}

function handleKey(e: KeyboardEvent) {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault()
    sendMessage()
  }
}

onUnmounted(() => {
  // desconecta o WS mas mantém o localStorage para o F5
  chatService.disconnect()
})
</script>

<template>
  <div class="fixed bottom-6 right-6 z-50 flex flex-col items-end gap-3">

    <!-- Chat Widget Panel -->
    <Transition name="chat-pop">
      <div
        v-if="chatOpen"
        class="w-80 sm:w-96 bg-white dark:bg-gray-900 rounded-2xl shadow-2xl overflow-hidden flex flex-col border border-gray-200 dark:border-gray-700"
        style="max-height: 520px"
      >
        <!-- Header -->
        <div class="bg-gradient-to-r from-purple-600 to-pink-600 px-4 py-3 flex items-center justify-between flex-shrink-0">
          <div class="flex items-center gap-2">
            <div class="w-8 h-8 bg-white/20 rounded-full flex items-center justify-center">
              <i class="fas fa-headset text-white text-sm"></i>
            </div>
            <div>
              <p class="text-white font-semibold text-sm leading-none">Suporte MeetConnect</p>
              <p class="text-white/70 text-xs mt-0.5">
                {{ chatStep === 'chat' ? 'Aguardando atendente...' : 'Preencha para iniciar' }}
              </p>
            </div>
          </div>
          <div class="flex items-center gap-2">
            <button
              v-if="chatStep === 'chat'"
              @click="endChat"
              title="Encerrar conversa"
              class="text-white/60 hover:text-red-300 transition text-xs"
            >
              <i class="fas fa-sign-out-alt"></i>
            </button>
            <button @click="toggleChat" class="text-white/70 hover:text-white transition">
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>

        <!-- STEP 1: Formulário de identificação -->
        <div v-if="chatStep === 'form'" class="p-4 flex-1 overflow-y-auto space-y-3">
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Conte-nos um pouco sobre você para iniciar o atendimento.
          </p>
          <div>
            <label class="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">Nome *</label>
            <input
              v-model="clientForm.name"
              type="text"
              placeholder="Seu nome completo"
              class="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-800 dark:text-white"
            />
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">E-mail *</label>
            <input
              v-model="clientForm.email"
              type="email"
              placeholder="seu@email.com"
              class="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-800 dark:text-white"
            />
            <p v-if="clientForm.email && !isValidEmail(clientForm.email)" class="mt-1 text-xs text-red-500">
              E-mail inválido
            </p>
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">Telefone</label>
            <input
              v-model="clientForm.phone"
              type="tel"
              placeholder="(00) 00000-0000"
              class="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-800 dark:text-white"
            />
          </div>
          <div>
            <label class="block text-xs font-medium text-gray-600 dark:text-gray-300 mb-1">CPF</label>
            <input
              v-model="clientForm.cpf"
              type="text"
              placeholder="000.000.000-00"
              class="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-800 dark:text-white"
            />
          </div>

          <!-- Erro API -->
          <p v-if="errorMsg" class="text-xs text-red-500 text-center">{{ errorMsg }}</p>

          <button
            @click="submitForm"
            :disabled="!clientForm.name || (!!clientForm.email && !isValidEmail(clientForm.email)) || submitting"
            class="w-full py-2 text-sm font-semibold text-white bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 rounded-lg transition disabled:opacity-40 disabled:cursor-not-allowed flex items-center justify-center gap-2"
          >
            <i v-if="submitting" class="fas fa-circle-notch fa-spin"></i>
            <span>{{ submitting ? 'Conectando...' : 'Iniciar atendimento' }}</span>
          </button>
        </div>

        <!-- STEP 2: Chat -->
        <div v-else class="flex flex-col flex-1 min-h-0">
          <!-- Messages -->
          <div class="flex-1 overflow-y-auto p-4 space-y-3" style="max-height: 340px">
            <div
              v-for="(msg, i) in messages"
              :key="i"
              :class="['flex', msg.from === 'client' ? 'justify-end' : 'justify-start']"
            >
              <div :class="[
                'max-w-[75%] px-3 py-2 rounded-2xl text-sm',
                msg.from === 'client'
                  ? 'bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-br-sm'
                  : 'bg-gray-100 dark:bg-gray-800 text-gray-800 dark:text-gray-200 rounded-bl-sm'
              ]">
                <p>{{ msg.text }}</p>
                <p :class="['text-xs mt-1', msg.from === 'client' ? 'text-white/60 text-right' : 'text-gray-400']">
                  {{ msg.time }}
                </p>
              </div>
            </div>
            <div ref="messagesEnd"></div>
          </div>

          <!-- Input ou Banner de encerrado -->
          <div class="border-t border-gray-200 dark:border-gray-700 flex-shrink-0">
            <!-- Conversa encerrada -->
            <div v-if="conversationClosed" class="px-4 py-3 flex flex-col items-center gap-2 text-center bg-gray-50 dark:bg-gray-800/60">
              <p class="text-xs text-gray-500 dark:text-gray-400">
                <i class="fas fa-lock mr-1 text-gray-400"></i>Atendimento encerrado
              </p>
              <button
                @click="endChat"
                class="text-xs px-3 py-1.5 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition"
              >
                Iniciar novo atendimento
              </button>
            </div>
            <!-- Input normal -->
            <div v-else class="px-3 py-2 flex items-end gap-2">
              <textarea
                v-model="messageInput"
                @keydown="handleKey"
                rows="1"
                placeholder="Digite sua mensagem..."
                class="flex-1 px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-800 dark:text-white resize-none"
              />
              <button
                @click="sendMessage"
                :disabled="!messageInput.trim()"
                class="w-9 h-9 bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 text-white rounded-xl flex items-center justify-center transition disabled:opacity-40 flex-shrink-0"
              >
                <i class="fas fa-paper-plane text-xs"></i>
              </button>
            </div>
          </div>
        </div>

      </div>
    </Transition>

    <!-- Floating Button -->
    <button
      @click="toggleChat"
      class="w-14 h-14 bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 text-white rounded-full shadow-lg shadow-purple-500/40 flex items-center justify-center transition-all duration-200 hover:scale-110"
    >
      <i :class="chatOpen ? 'fas fa-times text-xl' : 'fas fa-comments text-xl'"></i>
    </button>

  </div>
</template>

<style scoped>
.chat-pop-enter-active,
.chat-pop-leave-active {
  transition: all 0.25s ease;
}
.chat-pop-enter-from,
.chat-pop-leave-to {
  opacity: 0;
  transform: translateY(16px) scale(0.95);
}
</style>
