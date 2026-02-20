<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { usersService, type User, type Permission, type CreateUserRequest, type UpdateUserRequest } from '@/services/users.service'
import { useToastStore } from '@/stores/toastStore'

const toastStore = useToastStore()

// Estado da aplicação
const users = ref<User[]>([])
const permissions = ref<Permission[]>([])
const loading = ref(false)
const loadingPermissions = ref(false)

// Filtros e busca
const searchQuery = ref('')
const selectedPermission = ref('all')
const selectedStatus = ref('all')

// Modais
const showAddUser = ref(false)
const showEditUser = ref(false)
const editingUser = ref<User | null>(null)

// Modal de reset de senha
const showResetPassword = ref(false)
const resetPasswordUser = ref<User | null>(null)
const newPassword = ref('')
const newPasswordConfirm = ref('')
const showPassword = ref(false)

// Modal de confirmação customizado
const confirmModal = ref({
  show: false,
  title: '',
  message: '',
  confirmLabel: 'Confirmar',
  confirmClass: 'bg-red-600 hover:bg-red-700',
  resolve: null as ((val: boolean) => void) | null
})

function showConfirm(title: string, message: string, confirmLabel = 'Confirmar', confirmClass = 'bg-red-600 hover:bg-red-700'): Promise<boolean> {
  return new Promise(resolve => {
    confirmModal.value = { show: true, title, message, confirmLabel, confirmClass, resolve }
  })
}

function handleConfirm(result: boolean) {
  confirmModal.value.show = false
  confirmModal.value.resolve?.(result)
}

// Form para novo usuário
const newUser = ref<CreateUserRequest>({
  name: '',
  username: '',
  email: '',
  permissionId: 1, // Será definido dinamicamente 
  password: ''
})

// Computed para permissionId padrão 
const defaultPermissionId = computed(() => {
  return permissions.value.find(p => p.name === 'operator')?.id || 1
})

// Computed properties
const filteredUsers = computed(() => {
  let filtered = users.value

  if (searchQuery.value) {
    filtered = filtered.filter(user =>
      user.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      user.username.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      user.email.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
  }

  if (selectedPermission.value !== 'all') {
    filtered = filtered.filter(user => 
      user.permissionName === selectedPermission.value
    )
  }

  if (selectedStatus.value === 'active') {
    filtered = filtered.filter(user => !user.deletedAt)
  } else if (selectedStatus.value === 'inactive') {
    filtered = filtered.filter(user => !!user.deletedAt)
  }

  return filtered
})

const activeUsersCount = computed(() => users.value.filter(u => !u.deletedAt).length)
const inactiveUsersCount = computed(() => users.value.filter(u => !!u.deletedAt).length)
const adminUsersCount = computed(() => users.value.filter(u => u.permissionName === 'admin' && !u.deletedAt).length)
const operatorUsersCount = computed(() => users.value.filter(u => u.permissionName === 'operator' && !u.deletedAt).length)

// Métodos de carregamento
async function loadUsers() {
  loading.value = true
  try {
    users.value = await usersService.getAll()
    console.log('Usuários carregados:', users.value)
  } catch (error: any) {
    toastStore.error('Erro ao carregar usuários: ' + error.message)
    console.error('Erro ao carregar usuários:', error)
  } finally {
    loading.value = false
  }
}

async function loadPermissions() {
  loadingPermissions.value = true
  try {
    permissions.value = await usersService.getPermissions()
    console.log('Permissões carregadas:', permissions.value)
  } catch (error: any) {
    toastStore.error('Erro ao carregar permissões: ' + error.message)
    console.error('Erro ao carregar permissões:', error)
  } finally {
    loadingPermissions.value = false
  }
}

// Métodos de utilidade
function getPermissionColor(permissionName: string) {
  return permissionName === 'admin'
    ? 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400'
    : 'bg-blue-100 text-blue-800 dark:bg-blue-900/20 dark:text-blue-400'
}

function getStatusColor(isActive: boolean) {
  return isActive
    ? 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400'
    : 'bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400'
}

function formatDate(date: string | null | undefined): string {
  if (!date) return 'Nunca'
  return new Date(date).toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

function isValidEmail(email: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
}

// Métodos de CRUD
async function addUser() {
  if (!newUser.value.name || !newUser.value.username || !newUser.value.email || !newUser.value.password) {
    toastStore.error('Por favor, preencha todos os campos obrigatórios')
    return
  }

  if (!isValidEmail(newUser.value.email)) {
    toastStore.error('Informe um e-mail válido')
    return
  }

  try {
    const createdUser = await usersService.create(newUser.value)
    users.value.push(createdUser)
    newUser.value = {
      name: '',
      username: '',
      email: '',
      permissionId: defaultPermissionId.value,
      password: ''
    }
    showAddUser.value = false
    toastStore.success('Usuário criado com sucesso')
  } catch (error: any) {
    toastStore.error('Erro ao criar usuário: ' + error.message)
  }
}

function editUser(user: User) {
  editingUser.value = { ...user }
  showEditUser.value = true
}

async function updateUser() {
  if (!editingUser.value) return

  if (editingUser.value.email && !isValidEmail(editingUser.value.email)) {
    toastStore.error('Informe um e-mail válido')
    return
  }

  try {
    const updateData: UpdateUserRequest = {
      name: editingUser.value.name,
      email: editingUser.value.email || '',
      permissionId: editingUser.value.permissionId
    }
    
    await usersService.update(editingUser.value.id, updateData)
    await loadUsers()
    showEditUser.value = false
    editingUser.value = null
    toastStore.success('Usuário atualizado com sucesso')
  } catch (error: any) {
    toastStore.error('Erro ao atualizar usuário: ' + error.message)
  }
}

async function deleteUser(userId: number) {
  const ok = await showConfirm('Excluir usuário', 'Tem certeza que deseja excluir este usuário? Esta ação não pode ser desfeita.', 'Excluir', 'bg-red-600 hover:bg-red-700')
  if (!ok) return

  try {
    await usersService.delete(userId)
    await loadUsers() // Recarregar a lista
    toastStore.success('Usuário excluído com sucesso')
  } catch (error: any) {
    toastStore.error('Erro ao excluir usuário: ' + error.message)
  }
}

async function restoreUser(userId: number) {
  const ok = await showConfirm('Restaurar usuário', 'Tem certeza que deseja restaurar este usuário?', 'Restaurar', 'bg-green-600 hover:bg-green-700')
  if (!ok) return

  try {
    await usersService.restore(userId)
    await loadUsers() // Recarregar a lista
    toastStore.success('Usuário restaurado com sucesso')
  } catch (error: any) {
    toastStore.error('Erro ao restaurar usuário: ' + error.message)
  }
}

async function resetPassword(user: User) {
  resetPasswordUser.value = user
  newPassword.value = ''
  newPasswordConfirm.value = ''
  showPassword.value = false
  showResetPassword.value = true
}

async function submitResetPassword() {
  if (!resetPasswordUser.value) return

  if (newPassword.value.length < 6) {
    toastStore.error('A senha deve ter no mínimo 6 caracteres')
    return
  }

  if (newPassword.value !== newPasswordConfirm.value) {
    toastStore.error('As senhas não coincidem')
    return
  }

  try {
    await usersService.update(resetPasswordUser.value.id, { password: newPassword.value })
    showResetPassword.value = false
    resetPasswordUser.value = null
    toastStore.success('Senha redefinida com sucesso')
  } catch (error: any) {
    toastStore.error('Erro ao redefinir senha: ' + error.message)
  }
}

// Lifecycle
onMounted(async () => {
  await Promise.all([
    loadUsers(),
    loadPermissions()
  ])
  
  // Definir permissionId padrão após carregar permissões
  if (permissions.value.length > 0) {
    newUser.value.permissionId = defaultPermissionId.value
    console.log('✅ PermissionId padrão definido:', newUser.value.permissionId)
  }
})
</script>

<template>
  <div class="space-y-6 w-full min-w-0 overflow-container">
    <!-- Header -->
    <div class="flex justify-between items-center w-full min-w-0">
      <div class="min-w-0 flex-1">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">
          Gestão de Usuários
        </h1>
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Gerencie usuários do sistema e suas permissões
        </p>
      </div>
      <button
        @click="showAddUser = true"
        class="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition"
      >
        <i class="fas fa-user-plus mr-2"></i>
        Novo Usuário
      </button>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4 w-full">
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700 card-container">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-users text-blue-600 dark:text-blue-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Total de Usuários</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">
              {{ loading ? '...' : users.length }}
            </p>
          </div>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700 card-container">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-green-100 dark:bg-green-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-user-check text-green-600 dark:text-green-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Ativos</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">
              {{ loading ? '...' : activeUsersCount }}
            </p>
          </div>
        </div>
      </div>
      <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700 card-container">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-red-100 dark:bg-red-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-user-shield text-red-600 dark:text-red-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Administradores</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">
              {{ loading ? '...' : adminUsersCount }}
            </p>
          </div>
        </div>
      </div>
        <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700 card-container">
        <div class="flex items-center">
          <div class="w-10 h-10 bg-gray-100 dark:bg-gray-900/20 rounded-lg flex items-center justify-center">
            <i class="fas fa-user-times text-gray-600 dark:text-gray-400"></i>
          </div>
          <div class="ml-3">
            <p class="text-sm text-gray-600 dark:text-gray-400">Inativos</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">
              {{ loading ? '...' : inactiveUsersCount }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700 w-full card-container">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4 w-full">
        <div>
          <div class="relative">
            <i class="fas fa-search absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400"></i>
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Buscar por nome, usuário ou email..."
              class="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
            />
          </div>
        </div>
        <div>
          <select
            v-model="selectedPermission"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
          >
            <option value="all">Todas as permissões</option>
            <option v-for="permission in permissions" :key="permission.id" :value="permission.name">
              {{ permission.name }}
            </option>
          </select>
        </div>
        <div>
          <select
            v-model="selectedStatus"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
          >
            <option value="all">Todos os status</option>
            <option value="active">Ativos</option>
            <option value="inactive">Inativos</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Users Table -->
    <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden w-full card-container">
      <div class="overflow-x-auto w-full overflow-container">
        <table class="w-full">
          <thead class="bg-gray-50 dark:bg-gray-700">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Usuário</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Permissão</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Status</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Último Acesso</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Criado em</th>
              <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Ações</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
            <!-- Loading state -->
            <tr v-if="loading">
              <td colspan="6" class="px-6 py-8 text-center text-gray-500 dark:text-gray-400">
                <i class="fas fa-spinner fa-spin mr-2"></i>
                Carregando usuários...
              </td>
            </tr>
            <!-- Empty state -->
            <tr v-else-if="filteredUsers.length === 0">
              <td colspan="6" class="px-6 py-8 text-center text-gray-500 dark:text-gray-400">
                Nenhum usuário encontrado
              </td>
            </tr>
            <!-- Users list -->
            <tr v-else v-for="user in filteredUsers" :key="user.id" class="hover:bg-gray-50 dark:hover:bg-gray-700/50">
              <td class="px-6 py-4">
                <div>
                  <div class="font-medium text-gray-900 dark:text-white">{{ user.name }}</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">@{{ user.username }}</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">{{ user.email }}</div>
                </div>
              </td>
              <td class="px-6 py-4">
                <span :class="['px-2 py-1 text-xs font-medium rounded-full', getPermissionColor(user.permissionName)]">
                  {{ user.permissionName }}
                </span>
              </td>
              <td class="px-6 py-4">
                <span :class="['px-2 py-1 text-xs font-medium rounded-full', getStatusColor(!user.deletedAt)]">
                  {{ !user.deletedAt ? 'Ativo' : 'Inativo' }}
                </span>
              </td>
              <td class="px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
                {{ formatDate(user.sessionAt) }}
              </td>
              <td class="px-6 py-4 text-sm text-gray-500 dark:text-gray-400">
                {{ formatDate(user.createdAt) }}
              </td>
              <td class="px-6 py-4 text-right space-x-2">
                <template v-if="user.deletedAt">
                  <!-- Usuário deletado - mostrar apenas botão de restaurar -->
                  <button
                    @click="restoreUser(user.id)"
                    class="text-green-600 dark:text-green-400 hover:text-green-900 dark:hover:text-green-300"
                    title="Restaurar usuário"
                  >
                    <i class="fas fa-undo"></i>
                  </button>
                </template>
                <template v-else>
                  <!-- Usuário ativo - mostrar ações normais -->
                  <button
                    @click="resetPassword(user)"
                    class="text-blue-600 hover:text-blue-900 dark:text-blue-400 dark:hover:text-blue-300"
                    title="Resetar senha do usuário"
                  >
                    <i class="fas fa-key"></i>
                  </button>
                  <button
                    @click="editUser(user)"
                    class="text-purple-600 dark:text-purple-400 hover:text-purple-900 dark:hover:text-purple-300"
                  >
                    <i class="fas fa-edit"></i>
                  </button>
                  <button
                    @click="deleteUser(user.id)"
                    class="text-red-600 dark:text-red-400 hover:text-red-900 dark:hover:text-red-300"
                  >
                    <i class="fas fa-trash"></i>
                  </button>
                </template>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add User Modal -->
    <div v-if="showAddUser" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md mx-4">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Novo Usuário</h3>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Nome *</label>
            <input
              v-model="newUser.name"
              type="text"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
              required
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Usuário *</label>
            <input
              v-model="newUser.username"
              type="text"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
              required
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Email *</label>
            <input
              v-model="newUser.email"
              type="email"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
              required
            />
            <p v-if="newUser.email && !isValidEmail(newUser.email)" class="mt-1 text-xs text-red-500">E-mail inválido</p>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Senha *</label>
            <input
              v-model="newUser.password"
              type="password"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
              required
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Permissão *</label>
            <select
              v-model="newUser.permissionId"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
              :disabled="loadingPermissions"
            >
              <option v-if="loadingPermissions" disabled>Carregando permissões...</option>
              <option v-else-if="permissions.length === 0" disabled>Nenhuma permissão disponível</option>
              <option v-else v-for="permission in permissions" :key="permission.id" :value="permission.id">
                {{ permission.name }}
              </option>
            </select>
          </div>
        </div>
        <div class="flex gap-3 mt-6">
          <button
            @click="addUser"
            :disabled="loadingPermissions || !newUser.name || !newUser.username || !newUser.email || !newUser.password"
            class="flex-1 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Adicionar
          </button>
          <button
            @click="showAddUser = false"
            class="flex-1 px-4 py-2 bg-gray-300 hover:bg-gray-400 text-gray-700 rounded-lg transition"
          >
            Cancelar
          </button>
        </div>
      </div>
    </div>

    <!-- Edit User Modal -->
    <div v-if="showEditUser && editingUser" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md mx-4">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Editar Usuário</h3>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Nome *</label>
            <input
              v-model="editingUser.name"
              type="text"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Usuário</label>
            <input
              :value="editingUser.username"
              type="text"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-gray-50 dark:bg-gray-600 text-gray-500 dark:text-gray-400"
              disabled
              title="O nome de usuário não pode ser alterado"
            />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Email *</label>
            <input
              v-model="editingUser.email"
              type="email"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
            />
            <p v-if="editingUser.email && !isValidEmail(editingUser.email)" class="mt-1 text-xs text-red-500">E-mail inválido</p>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Permissão *</label>
            <select
              v-model="editingUser.permissionId"
              class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
              :disabled="loadingPermissions"
            >
              <option v-if="loadingPermissions" disabled>Carregando permissões...</option>
              <option v-else-if="permissions.length === 0" disabled>Nenhuma permissão disponível</option>
              <option v-else v-for="permission in permissions" :key="permission.id" :value="permission.id">
                {{ permission.name }}
              </option>
            </select>
          </div>
        </div>
        <div class="flex gap-3 mt-6">
          <button
            @click="updateUser"
            :disabled="loadingPermissions || !editingUser.name || !editingUser.email"
            class="flex-1 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Salvar
          </button>
          <button
            @click="showEditUser = false; editingUser = null"
            class="flex-1 px-4 py-2 bg-gray-300 hover:bg-gray-400 text-gray-700 rounded-lg transition"
          >
            Cancelar
          </button>
        </div>
      </div>
    </div>

    <!-- Reset Password Modal -->
    <div v-if="showResetPassword && resetPasswordUser" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md mx-4 shadow-xl">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-1">Redefinir senha</h3>
        <p class="text-sm text-gray-500 dark:text-gray-400 mb-5">Defina uma nova senha para <span class="font-medium text-gray-700 dark:text-gray-200">{{ resetPasswordUser.name }}</span></p>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Nova senha *</label>
            <div class="relative">
              <input
                v-model="newPassword"
                :type="showPassword ? 'text' : 'password'"
                class="w-full px-3 py-2 pr-10 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
                placeholder="Mínimo 6 caracteres"
              />
              <button type="button" @click="showPassword = !showPassword" class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200">
                <i :class="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
              </button>
            </div>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Confirmar nova senha *</label>
            <div class="relative">
              <input
                v-model="newPasswordConfirm"
                :type="showPassword ? 'text' : 'password'"
                class="w-full px-3 py-2 pr-10 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white"
                placeholder="Repita a nova senha"
              />
            </div>
            <p v-if="newPasswordConfirm && newPassword !== newPasswordConfirm" class="mt-1 text-xs text-red-500">As senhas não coincidem</p>
          </div>
        </div>
        <div class="flex gap-3 mt-6">
          <button
            @click="submitResetPassword"
            :disabled="!newPassword || !newPasswordConfirm || newPassword !== newPasswordConfirm"
            class="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Redefinir senha
          </button>
          <button
            @click="showResetPassword = false; resetPasswordUser = null"
            class="flex-1 px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-lg transition"
          >
            Cancelar
          </button>
        </div>
      </div>
    </div>

    <!-- Confirm Modal -->
    <div v-if="confirmModal.show" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-sm mx-4 shadow-xl">
        <div class="flex items-center gap-3 mb-3">
          <div class="w-10 h-10 rounded-full bg-red-100 dark:bg-red-900/30 flex items-center justify-center flex-shrink-0">
            <i class="fas fa-exclamation-triangle text-red-600 dark:text-red-400"></i>
          </div>
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white">{{ confirmModal.title }}</h3>
        </div>
        <p class="text-sm text-gray-600 dark:text-gray-400 mb-6 ml-13">{{ confirmModal.message }}</p>
        <div class="flex gap-3 justify-end">
          <button
            @click="handleConfirm(false)"
            class="px-4 py-2 text-sm bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-lg transition"
          >
            Cancelar
          </button>
          <button
            @click="handleConfirm(true)"
            :class="['px-4 py-2 text-sm text-white rounded-lg transition', confirmModal.confirmClass]"
          >
            {{ confirmModal.confirmLabel }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>