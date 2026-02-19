<script setup lang="ts">
import { ref } from 'vue'

// Dados simulados de atendimento
const currentChats = ref([
  {
    id: 1,
    client: 'João Silva',
    status: 'active',
    lastMessage: 'Preciso de ajuda com meu projeto...',
    time: '14:23',
    avatar: null
  },
  {
    id: 2,
    client: 'Maria Santos',
    status: 'waiting',
    lastMessage: 'Olá, gostaria de agendar uma reunião',
    time: '14:18',
    avatar: null
  }
])

const pendingMeetings = ref([
  {
    id: 1,
    client: 'Carlos Oliveira',
    title: 'Consultoria sobre automação',
    time: '15:00',
    duration: '1h',
    status: 'scheduled'
  },
  {
    id: 2,
    client: 'Ana Costa',
    title: 'Demonstração da plataforma',
    time: '16:30',
    duration: '30min',
    status: 'confirmed'
  }
])

const activeTab = ref('chats')

function joinChat(chatId: number) {
  console.log('Joining chat:', chatId)
}

function joinMeeting(meetingId: number) {
  console.log('Joining meeting:', meetingId)
}

function getStatusColor(status: string) {
  const colors = {
    'active': 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400',
    'waiting': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400',
    'scheduled': 'bg-blue-100 text-blue-800 dark:bg-blue-900/20 dark:text-blue-400',
    'confirmed': 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400'
  }
  return colors[status as keyof typeof colors] || 'bg-gray-100 text-gray-800'
}
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div>
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
        Atendimento ao Cliente
      </h1>
      <p class="text-sm text-gray-600 dark:text-gray-400">
        Gerencie chats e reuniões em tempo real
      </p>
    </div>

    <!-- Stats Row -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-green-100 dark:bg-green-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-comments text-green-600 dark:text-green-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Chats Ativos</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ currentChats.filter(c => c.status === 'active').length }}</p>
          </div>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-yellow-100 dark:bg-yellow-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-clock text-yellow-600 dark:text-yellow-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Na Fila</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ currentChats.filter(c => c.status === 'waiting').length }}</p>
          </div>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-video text-blue-600 dark:text-blue-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Reuniões Hoje</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ pendingMeetings.length }}</p>
          </div>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-purple-100 dark:bg-purple-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-headset text-purple-600 dark:text-purple-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Tempo Resposta</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">2m 30s</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Tabs -->
    <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
      <div class="border-b border-gray-200 dark:border-gray-700">
        <nav class="flex space-x-8 px-6">
          <button
            @click="activeTab = 'chats'"
            :class="[
              'py-4 text-sm font-medium border-b-2 transition',
              activeTab === 'chats'
                ? 'border-purple-500 text-purple-600 dark:text-purple-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
            ]"
          >
            <i class="fas fa-comments mr-2"></i>
            Chats ({{ currentChats.length }})
          </button>
          <button
            @click="activeTab = 'meetings'"
            :class="[
              'py-4 text-sm font-medium border-b-2 transition',
              activeTab === 'meetings'
                ? 'border-purple-500 text-purple-600 dark:text-purple-400'
                : 'border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300'
            ]"
          >
            <i class="fas fa-video mr-2"></i>
            Reuniões ({{ pendingMeetings.length }})
          </button>
        </nav>
      </div>

      <!-- Chat Tab -->
      <div v-if="activeTab === 'chats'" class="p-6">
        <div class="space-y-4">
          <div
            v-for="chat in currentChats"
            :key="chat.id"
            class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700/50 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition"
          >
            <div class="flex items-center gap-4">
              <div class="w-10 h-10 bg-gray-300 dark:bg-gray-600 rounded-full flex items-center justify-center">
                <i class="fas fa-user text-gray-600 dark:text-gray-400"></i>
              </div>
              <div>
                <h3 class="font-medium text-gray-900 dark:text-white">{{ chat.client }}</h3>
                <p class="text-sm text-gray-600 dark:text-gray-400 truncate max-w-xs">{{ chat.lastMessage }}</p>
              </div>
            </div>
            <div class="flex items-center gap-3">
              <span :class="['px-2 py-1 text-xs font-medium rounded-full', getStatusColor(chat.status)]">
                {{ chat.status === 'active' ? 'Ativo' : 'Aguardando' }}
              </span>
              <span class="text-sm text-gray-500 dark:text-gray-400">{{ chat.time }}</span>
              <button
                @click="joinChat(chat.id)"
                class="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white text-sm rounded-lg transition"
              >
                <i class="fas fa-reply mr-2"></i>
                Responder
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Meetings Tab -->
      <div v-if="activeTab === 'meetings'" class="p-6">
        <div class="space-y-4">
          <div
            v-for="meeting in pendingMeetings"
            :key="meeting.id"
            class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700/50 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition"
          >
            <div class="flex items-center gap-4">
              <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center">
                <i class="fas fa-video text-blue-600 dark:text-blue-400"></i>
              </div>
              <div>
                <h3 class="font-medium text-gray-900 dark:text-white">{{ meeting.title }}</h3>
                <p class="text-sm text-gray-600 dark:text-gray-400">{{ meeting.client }} • {{ meeting.duration }}</p>
              </div>
            </div>
            <div class="flex items-center gap-3">
              <span :class="['px-2 py-1 text-xs font-medium rounded-full', getStatusColor(meeting.status)]">
                {{ meeting.status === 'scheduled' ? 'Agendada' : 'Confirmada' }}
              </span>
              <span class="text-sm text-gray-500 dark:text-gray-400">{{ meeting.time }}</span>
              <button
                @click="joinMeeting(meeting.id)"
                class="px-4 py-2 bg-green-600 hover:bg-green-700 text-white text-sm rounded-lg transition"
              >
                <i class="fas fa-play mr-2"></i>
                Iniciar
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>