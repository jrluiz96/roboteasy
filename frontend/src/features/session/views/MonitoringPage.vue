<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'

// Dados simulados de monitoramento
const systemStats = ref({
  uptime: '99.8%',
  responseTime: '245ms',
  activeUsers: 12,
  activeChats: 3,
  activeMeetings: 2,
  serverLoad: 45,
  memoryUsage: 67,
  diskUsage: 34
})

const realtimeData = ref([
  { time: '14:55', users: 12, chats: 3, meetings: 2, load: 45 },
  { time: '14:50', users: 10, chats: 2, meetings: 1, load: 42 },
  { time: '14:45', users: 8, chats: 1, meetings: 2, load: 38 },
  { time: '14:40', users: 11, chats: 4, meetings: 1, load: 48 },
  { time: '14:35', users: 9, chats: 2, meetings: 3, load: 44 }
])

const alerts = ref([
  {
    id: 1,
    type: 'warning',
    title: 'Uso de memória elevado',
    description: 'Uso de memória atingiu 67% da capacidade',
    time: '14:52',
    severity: 'medium'
  },
  {
    id: 2,
    type: 'info',
    title: 'Backup automático concluído',
    description: 'Backup diário executado com sucesso',
    time: '14:30',
    severity: 'low'
  },
  {
    id: 3,
    type: 'success',
    title: 'Sistema funcionando normalmente',
    description: 'Todos os serviços operando dentro dos parâmetros normais',
    time: '14:25',
    severity: 'low'
  }
])

const services = ref([
  {
    name: 'API Gateway',
    status: 'running',
    uptime: '99.9%',
    cpu: 23,
    memory: 45,
    requests: '1.2K/min'
  },
  {
    name: 'Chat Service',
    status: 'running',
    uptime: '99.7%',
    cpu: 18,
    memory: 32,
    requests: '450/min'
  },
  {
    name: 'Video Service',
    status: 'running',
    uptime: '99.8%',
    cpu: 35,
    memory: 58,
    requests: '125/min'
  },
  {
    name: 'Database',
    status: 'running',
    uptime: '100%',
    cpu: 12,
    memory: 28,
    requests: '2.8K/min'
  },
  {
    name: 'File Storage',
    status: 'warning',
    uptime: '98.5%',
    cpu: 8,
    memory: 15,
    requests: '89/min'
  }
])

const selectedTimeRange = ref('1h')

const chartData = computed(() => {
  return realtimeData.value.reverse().map(item => ({
    time: item.time,
    users: item.users,
    load: item.load
  }))
})

function getStatusColor(status: string): string {
  const colors = {
    'running': 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400',
    'warning': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400',
    'error': 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400',
    'stopped': 'bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400'
  }\n  return colors[status as keyof typeof colors] || 'bg-gray-100 text-gray-800'\n}\n\nfunction getAlertColor(type: string): string {\n  const colors = {\n    'success': 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400',\n    'warning': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-400',\n    'error': 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400',\n    'info': 'bg-blue-100 text-blue-800 dark:bg-blue-900/20 dark:text-blue-400'\n  }\n  return colors[type as keyof typeof colors] || 'bg-gray-100 text-gray-800'\n}\n\nfunction getAlertIcon(type: string): string {\n  const icons = {\n    'success': 'fa-check-circle',\n    'warning': 'fa-exclamation-triangle',\n    'error': 'fa-times-circle',\n    'info': 'fa-info-circle'\n  }\n  return icons[type as keyof typeof icons] || 'fa-circle'\n}\n\nfunction restartService(serviceName: string) {\n  console.log('Restarting service:', serviceName)\n  // Simular reinicialização\n  const service = services.value.find(s => s.name === serviceName)\n  if (service) {\n    service.status = 'running'\n  }\n}\n\nfunction viewServiceLogs(serviceName: string) {\n  console.log('Viewing logs for:', serviceName)\n}\n\nfunction dismissAlert(alertId: number) {\n  alerts.value = alerts.value.filter(a => a.id !== alertId)\n}\n\nfunction refreshData() {\n  console.log('Refreshing monitoring data...')\n  // Simular atualização dos dados\n}\n\n// Simular atualização em tempo real\nonMounted(() => {\n  setInterval(() => {\n    // Atualizar dados em tempo real\n    systemStats.value.activeUsers = Math.floor(Math.random() * 20) + 5\n    systemStats.value.activeChats = Math.floor(Math.random() * 8) + 1\n    systemStats.value.activeMeetings = Math.floor(Math.random() * 5) + 1\n    systemStats.value.serverLoad = Math.floor(Math.random() * 30) + 30\n  }, 30000) // Atualizar a cada 30 segundos\n})\n</script>\n\n<template>\n  <div class=\"space-y-6\">\n    <!-- Header -->\n    <div class=\"flex justify-between items-center\">\n      <div>\n        <h1 class=\"text-2xl font-bold text-gray-900 dark:text-white\">\n          Monitoramento do Sistema\n        </h1>\n        <p class=\"text-sm text-gray-600 dark:text-gray-400\">\n          Acompanhe a saúde e performance do sistema em tempo real\n        </p>\n      </div>\n      <div class=\"flex gap-2\">\n        <button\n          @click=\"refreshData\"\n          class=\"px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition\"\n        >\n          <i class=\"fas fa-sync-alt mr-2\"></i>\n          Atualizar\n        </button>\n        <select\n          v-model=\"selectedTimeRange\"\n          class=\"px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white\"\n        >\n          <option value=\"1h\">Última hora</option>\n          <option value=\"6h\">Últimas 6 horas</option>\n          <option value=\"24h\">Último dia</option>\n        </select>\n      </div>\n    </div>\n\n    <!-- System Overview -->\n    <div class=\"grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4\">\n      <div class=\"bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700\">\n        <div class=\"flex items-center justify-between\">\n          <div>\n            <p class=\"text-sm text-gray-600 dark:text-gray-400\">Uptime do Sistema</p>\n            <p class=\"text-2xl font-bold text-green-600 dark:text-green-400\">{{ systemStats.uptime }}</p>\n          </div>\n          <div class=\"w-10 h-10 bg-green-100 dark:bg-green-900/20 rounded-lg flex items-center justify-center\">\n            <i class=\"fas fa-server text-green-600 dark:text-green-400\"></i>\n          </div>\n        </div>\n      </div>\n      \n      <div class=\"bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700\">\n        <div class=\"flex items-center justify-between\">\n          <div>\n            <p class=\"text-sm text-gray-600 dark:text-gray-400\">Tempo de Resposta</p>\n            <p class=\"text-2xl font-bold text-blue-600 dark:text-blue-400\">{{ systemStats.responseTime }}</p>\n          </div>\n          <div class=\"w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center\">\n            <i class=\"fas fa-clock text-blue-600 dark:text-blue-400\"></i>\n          </div>\n        </div>\n      </div>\n      \n      <div class=\"bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700\">\n        <div class=\"flex items-center justify-between\">\n          <div>\n            <p class=\"text-sm text-gray-600 dark:text-gray-400\">Usuários Online</p>\n            <p class=\"text-2xl font-bold text-purple-600 dark:text-purple-400\">{{ systemStats.activeUsers }}</p>\n          </div>\n          <div class=\"w-10 h-10 bg-purple-100 dark:bg-purple-900/20 rounded-lg flex items-center justify-center\">\n            <i class=\"fas fa-users text-purple-600 dark:text-purple-400\"></i>\n          </div>\n        </div>\n      </div>\n      \n      <div class=\"bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700\">\n        <div class=\"flex items-center justify-between\">\n          <div>\n            <p class=\"text-sm text-gray-600 dark:text-gray-400\">Carga do Servidor</p>\n            <p class=\"text-2xl font-bold text-yellow-600 dark:text-yellow-400\">{{ systemStats.serverLoad }}%</p>\n          </div>\n          <div class=\"w-10 h-10 bg-yellow-100 dark:bg-yellow-900/20 rounded-lg flex items-center justify-center\">\n            <i class=\"fas fa-microchip text-yellow-600 dark:text-yellow-400\"></i>\n          </div>\n        </div>\n      </div>\n    </div>\n\n    <!-- Resource Usage -->\n    <div class=\"grid grid-cols-1 lg:grid-cols-3 gap-6\">\n      <div class=\"bg-white dark:bg-gray-800 p-6 rounded-lg border border-gray-200 dark:border-gray-700\">\n        <h3 class=\"text-lg font-semibold text-gray-900 dark:text-white mb-4\">Uso de CPU</h3>\n        <div class=\"space-y-2\">\n          <div class=\"flex justify-between text-sm\">\n            <span class=\"text-gray-600 dark:text-gray-400\">Carga atual</span>\n            <span class=\"font-medium text-gray-900 dark:text-white\">{{ systemStats.serverLoad }}%</span>\n          </div>\n          <div class=\"w-full bg-gray-200 dark:bg-gray-700 rounded-full h-3\">\n            <div \n              class=\"bg-gradient-to-r from-blue-400 to-blue-600 h-3 rounded-full transition-all\"\n              :style=\"{ width: `${systemStats.serverLoad}%` }\"\n            ></div>\n          </div>\n        </div>\n      </div>\n      \n      <div class=\"bg-white dark:bg-gray-800 p-6 rounded-lg border border-gray-200 dark:border-gray-700\">\n        <h3 class=\"text-lg font-semibold text-gray-900 dark:text-white mb-4\">Uso de Memória</h3>\n        <div class=\"space-y-2\">\n          <div class=\"flex justify-between text-sm\">\n            <span class=\"text-gray-600 dark:text-gray-400\">Memória utilizada</span>\n            <span class=\"font-medium text-gray-900 dark:text-white\">{{ systemStats.memoryUsage }}%</span>\n          </div>\n          <div class=\"w-full bg-gray-200 dark:bg-gray-700 rounded-full h-3\">\n            <div \n              class=\"bg-gradient-to-r from-green-400 to-green-600 h-3 rounded-full transition-all\"\n              :style=\"{ width: `${systemStats.memoryUsage}%` }\"\n            ></div>\n          </div>\n        </div>\n      </div>\n      \n      <div class=\"bg-white dark:bg-gray-800 p-6 rounded-lg border border-gray-200 dark:border-gray-700\">\n        <h3 class=\"text-lg font-semibold text-gray-900 dark:text-white mb-4\">Uso de Disco</h3>\n        <div class=\"space-y-2\">\n          <div class=\"flex justify-between text-sm\">\n            <span class=\"text-gray-600 dark:text-gray-400\">Espaço utilizado</span>\n            <span class=\"font-medium text-gray-900 dark:text-white\">{{ systemStats.diskUsage }}%</span>\n          </div>\n          <div class=\"w-full bg-gray-200 dark:bg-gray-700 rounded-full h-3\">\n            <div \n              class=\"bg-gradient-to-r from-purple-400 to-purple-600 h-3 rounded-full transition-all\"\n              :style=\"{ width: `${systemStats.diskUsage}%` }\"\n            ></div>\n          </div>\n        </div>\n      </div>\n    </div>\n\n    <!-- Services Status -->\n    <div class=\"bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700\">\n      <div class=\"px-6 py-4 border-b border-gray-200 dark:border-gray-700\">\n        <h3 class=\"text-lg font-semibold text-gray-900 dark:text-white\">Status dos Serviços</h3>\n      </div>\n      <div class=\"p-6\">\n        <div class=\"space-y-4\">\n          <div \n            v-for=\"service in services\" \n            :key=\"service.name\"\n            class=\"flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700/50 rounded-lg\"\n          >\n            <div class=\"flex items-center gap-4\">\n              <div class=\"flex items-center gap-2\">\n                <span :class=\"['px-2 py-1 text-xs font-medium rounded-full', getStatusColor(service.status)]\">\n                  {{ service.status === 'running' ? 'Rodando' : service.status === 'warning' ? 'Aviso' : 'Parado' }}\n                </span>\n                <h4 class=\"font-medium text-gray-900 dark:text-white\">{{ service.name }}</h4>\n              </div>\n              <div class=\"flex items-center gap-6 text-sm text-gray-600 dark:text-gray-400\">\n                <span>Uptime: {{ service.uptime }}</span>\n                <span>CPU: {{ service.cpu }}%</span>\n                <span>Mem: {{ service.memory }}%</span>\n                <span>{{ service.requests }}</span>\n              </div>\n            </div>\n            <div class=\"flex gap-2\">\n              <button\n                @click=\"viewServiceLogs(service.name)\"\n                class=\"p-2 text-gray-600 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 transition\"\n                title=\"Ver logs\"\n              >\n                <i class=\"fas fa-file-alt\"></i>\n              </button>\n              <button\n                @click=\"restartService(service.name)\"\n                class=\"p-2 text-gray-600 dark:text-gray-400 hover:text-green-600 dark:hover:text-green-400 transition\"\n                title=\"Reiniciar serviço\"\n              >\n                <i class=\"fas fa-redo\"></i>\n              </button>\n            </div>\n          </div>\n        </div>\n      </div>\n    </div>\n\n    <!-- Alerts -->\n    <div class=\"bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700\">\n      <div class=\"px-6 py-4 border-b border-gray-200 dark:border-gray-700\">\n        <h3 class=\"text-lg font-semibold text-gray-900 dark:text-white\">Alertas e Notificações</h3>\n      </div>\n      <div class=\"p-6\">\n        <div class=\"space-y-3\">\n          <div \n            v-for=\"alert in alerts\" \n            :key=\"alert.id\"\n            class=\"flex items-start justify-between p-4 bg-gray-50 dark:bg-gray-700/50 rounded-lg\"\n          >\n            <div class=\"flex items-start gap-3\">\n              <div :class=\"['w-8 h-8 rounded-lg flex items-center justify-center', getAlertColor(alert.type)]\">\n                <i :class=\"['fas text-sm', getAlertIcon(alert.type)]\"></i>\n              </div>\n              <div>\n                <h4 class=\"font-medium text-gray-900 dark:text-white\">{{ alert.title }}</h4>\n                <p class=\"text-sm text-gray-600 dark:text-gray-400\">{{ alert.description }}</p>\n                <span class=\"text-xs text-gray-500 dark:text-gray-400\">{{ alert.time }}</span>\n              </div>\n            </div>\n            <button\n              @click=\"dismissAlert(alert.id)\"\n              class=\"p-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition\"\n              title=\"Dispensar alerta\"\n            >\n              <i class=\"fas fa-times\"></i>\n            </button>\n          </div>\n        </div>\n      </div>\n    </div>\n  </div>\n</template>