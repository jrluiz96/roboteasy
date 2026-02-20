# Padrão de Construção de Telas

> **Referência canônica:** `src/features/session/views/UsersPage.vue`
>
> Ao construir uma nova tela, use este documento como guia e diga:
> _"Seguindo o padrão do doc `07-padrao-telas.md`, construa a tela XYZ com as entidades A, B, C e as ações X, Y, Z."_

---

## 1. Visão geral da estrutura

Toda tela de gerenciamento segue o mesmo esqueleto:

```
<script setup>
  1. Imports
  2. Estado principal (lista + loading)
  3. Filtros e busca
  4. Estado dos modais (add / edit / confirm / ações específicas)
  5. Computed properties (filteredItems, contadores)
  6. Funções de carregamento (load*)
  7. Funções de utilidade (formatDate, getBadgeColor...)
  8. Funções de CRUD (add / edit / update / delete / restore)
  9. Funções de ações especiais (ex: resetPassword)
  10. onMounted → carrega tudo em paralelo
</script>

<template>
  1. Header (título + botão de ação principal)
  2. Stats cards
  3. Filtros (busca texto + selects)
  4. Tabela de dados (loading / empty / rows)
  5. Modal de criação
  6. Modal de edição
  7. Modais de ações específicas
  8. Modal de confirmação genérico
</template>
```

---

## 2. Arquivo de serviço (`src/services/*.service.ts`)

Cada entidade tem seu próprio arquivo de serviço. Padrão completo:

```ts
import { api, type ApiResponse } from './api'

// --- Interfaces ---
export interface MinhaEntidade {
  id: number
  // ... campos
  createdAt: string
  deletedAt: string | null   // null = ativo, preenchido = inativo
}

export interface CreateMinhaEntidadeRequest { /* ... */ }

export interface UpdateMinhaEntidadeRequest {
  campo1?: string            // tudo opcional — backend aceita atualização parcial
  campo2?: number
}

// --- Serviço ---
export const minhaEntidadeService = {
  async getAll(): Promise<MinhaEntidade[]> {
    const response = await api.get<MinhaEntidade[]>('/api/v1/rota')
    if (response.code === 200 && response.data) return response.data
    throw new Error(response.message)
  },

  async create(data: CreateMinhaEntidadeRequest): Promise<MinhaEntidade> {
    const response = await api.post<MinhaEntidade>('/api/v1/rota', data)
    if (response.code === 201 && response.data) return response.data
    throw new Error(response.message)
  },

  async update(id: number, data: UpdateMinhaEntidadeRequest): Promise<MinhaEntidade> {
    const response = await api.put<MinhaEntidade>(`/api/v1/rota/${id}`, data)
    if (response.code === 200 && response.data) return response.data
    throw new Error(response.message)
  },

  async delete(id: number): Promise<void> {
    const response = await api.delete(`/api/v1/rota/${id}`)
    if (response.code !== 200) throw new Error(response.message)
  },

  async restore(id: number): Promise<void> {
    const response = await api.patch(`/api/v1/rota/${id}/restore`)
    if (response.code !== 200) throw new Error(response.message)
  },
}
```

---

## 3. Script setup — passo a passo

### 3.1 Imports

```ts
import { ref, computed, onMounted } from 'vue'
import { minhaEntidadeService, type MinhaEntidade, type CreateMinhaEntidadeRequest } from '@/services/minhaEntidade.service'
import { useToastStore } from '@/stores/toastStore'
```

### 3.2 Estado principal

```ts
const toast = useToastStore()

const items = ref<MinhaEntidade[]>([])
const loading = ref(true)
```

### 3.3 Filtros e busca

```ts
const searchQuery = ref('')
const statusFilter = ref<'all' | 'active' | 'inactive'>('active')
```

### 3.4 Estado dos modais

```ts
// Modal de criação
const showAddModal = ref(false)
const addForm = ref<CreateMinhaEntidadeRequest>({ campo1: '', campo2: 0 })
const addLoading = ref(false)

// Modal de edição
const showEditModal = ref(false)
const editForm = ref<UpdateMinhaEntidadeRequest>({})
const editingItem = ref<MinhaEntidade | null>(null)
const editLoading = ref(false)

// Modal de confirmação genérico
const showConfirmModal = ref(false)
const confirmAction = ref<(() => Promise<void>) | null>(null)
const confirmTitle = ref('')
const confirmMessage = ref('')
const confirmLoading = ref(false)
```

### 3.5 Computed properties

```ts
const filteredItems = computed(() => {
  return items.value.filter(item => {
    // Filtro de status
    if (statusFilter.value === 'active' && item.deletedAt) return false
    if (statusFilter.value === 'inactive' && !item.deletedAt) return false

    // Busca textual
    if (searchQuery.value) {
      const query = searchQuery.value.toLowerCase()
      return item.campo1.toLowerCase().includes(query) ||
             item.campo2.toString().includes(query)
    }
    return true
  })
})

const stats = computed(() => ({
  total: items.value.length,
  active: items.value.filter(i => !i.deletedAt).length,
  inactive: items.value.filter(i => i.deletedAt).length,
}))
```

### 3.6 Funções de carregamento

```ts
async function loadItems() {
  loading.value = true
  try {
    items.value = await minhaEntidadeService.getAll()
  } catch (error: any) {
    toast.error(error.message || 'Erro ao carregar dados')
  } finally {
    loading.value = false
  }
}
```

### 3.7 Funções de utilidade

```ts
function formatDate(date: string) {
  return new Date(date).toLocaleDateString('pt-BR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}

function getBadgeColor(status: string): string {
  const colors: Record<string, string> = {
    active: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
    inactive: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300',
  }
  return colors[status] || 'bg-gray-100 text-gray-800'
}
```

### 3.8 Funções de CRUD

```ts
async function handleAdd() {
  addLoading.value = true
  try {
    await minhaEntidadeService.create(addForm.value)
    toast.success('Item criado com sucesso')
    showAddModal.value = false
    resetAddForm()
    await loadItems()
  } catch (error: any) {
    toast.error(error.message || 'Erro ao criar item')
  } finally {
    addLoading.value = false
  }
}

function openEdit(item: MinhaEntidade) {
  editingItem.value = item
  editForm.value = { campo1: item.campo1, campo2: item.campo2 }
  showEditModal.value = true
}

async function handleEdit() {
  if (!editingItem.value) return
  editLoading.value = true
  try {
    await minhaEntidadeService.update(editingItem.value.id, editForm.value)
    toast.success('Item atualizado com sucesso')
    showEditModal.value = false
    await loadItems()
  } catch (error: any) {
    toast.error(error.message || 'Erro ao atualizar')
  } finally {
    editLoading.value = false
  }
}

function confirmDelete(item: MinhaEntidade) {
  confirmTitle.value = 'Desativar Item'
  confirmMessage.value = `Deseja desativar "${item.campo1}"?`
  confirmAction.value = async () => {
    await minhaEntidadeService.delete(item.id)
    toast.success('Item desativado')
    await loadItems()
  }
  showConfirmModal.value = true
}

function confirmRestore(item: MinhaEntidade) {
  confirmTitle.value = 'Reativar Item'
  confirmMessage.value = `Deseja reativar "${item.campo1}"?`
  confirmAction.value = async () => {
    await minhaEntidadeService.restore(item.id)
    toast.success('Item reativado')
    await loadItems()
  }
  showConfirmModal.value = true
}

async function handleConfirm() {
  if (!confirmAction.value) return
  confirmLoading.value = true
  try {
    await confirmAction.value()
    showConfirmModal.value = false
  } catch (error: any) {
    toast.error(error.message || 'Erro ao executar ação')
  } finally {
    confirmLoading.value = false
  }
}
```

### 3.9 Funções de reset

```ts
function resetAddForm() {
  addForm.value = { campo1: '', campo2: 0 }
}
```

### 3.10 onMounted

```ts
onMounted(async () => {
  await Promise.all([
    loadItems(),
    // loadOutraCoisa(), // se necessário
  ])
})
```

---

## 4. Template — passo a passo

### 4.1 Header

```html
<div class="flex items-center justify-between mb-6">
  <div>
    <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Minha Entidade</h1>
    <p class="text-gray-600 dark:text-gray-400 mt-1">Gerencie as entidades do sistema</p>
  </div>
  <button
    @click="showAddModal = true"
    class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg flex items-center gap-2 transition"
  >
    <i class="fas fa-plus"></i>
    Nova Entidade
  </button>
</div>
```

### 4.2 Stats cards

```html
<div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
  <div class="bg-white dark:bg-gray-800 rounded-lg p-4 shadow-sm border border-gray-200 dark:border-gray-700">
    <div class="flex items-center gap-3">
      <div class="p-2 bg-blue-100 dark:bg-blue-900 rounded-lg">
        <i class="fas fa-list text-blue-600 dark:text-blue-400"></i>
      </div>
      <div>
        <p class="text-sm text-gray-600 dark:text-gray-400">Total</p>
        <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ stats.total }}</p>
      </div>
    </div>
  </div>
  <!-- ... cards de active / inactive -->
</div>
```

### 4.3 Filtros

```html
<div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 mb-6">
  <div class="flex flex-col md:flex-row gap-4">
    <div class="flex-1">
      <div class="relative">
        <i class="fas fa-search absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"></i>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Buscar por nome..."
          class="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>
    </div>
    <select
      v-model="statusFilter"
      class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
    >
      <option value="all">Todos</option>
      <option value="active">Ativos</option>
      <option value="inactive">Inativos</option>
    </select>
  </div>
</div>
```

### 4.4 Tabela de dados

```html
<div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
  <!-- Loading -->
  <div v-if="loading" class="p-8 text-center">
    <i class="fas fa-spinner fa-spin text-2xl text-gray-400"></i>
    <p class="mt-2 text-gray-500">Carregando...</p>
  </div>

  <!-- Empty state -->
  <div v-else-if="filteredItems.length === 0" class="p-8 text-center">
    <i class="fas fa-inbox text-4xl text-gray-300 dark:text-gray-600"></i>
    <p class="mt-2 text-gray-500 dark:text-gray-400">Nenhum item encontrado</p>
  </div>

  <!-- Table -->
  <table v-else class="w-full">
    <thead>
      <tr class="bg-gray-50 dark:bg-gray-700/50">
        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
          Nome
        </th>
        <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
          Status
        </th>
        <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
          Ações
        </th>
      </tr>
    </thead>
    <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
      <tr v-for="item in filteredItems" :key="item.id" class="hover:bg-gray-50 dark:hover:bg-gray-700/30">
        <td class="px-6 py-4 whitespace-nowrap">
          <span class="text-gray-900 dark:text-white font-medium">{{ item.campo1 }}</span>
        </td>
        <td class="px-6 py-4 whitespace-nowrap">
          <span :class="getBadgeColor(item.deletedAt ? 'inactive' : 'active')"
            class="px-2 py-1 text-xs font-medium rounded-full">
            {{ item.deletedAt ? 'Inativo' : 'Ativo' }}
          </span>
        </td>
        <td class="px-6 py-4 whitespace-nowrap text-right">
          <button @click="openEdit(item)" class="text-blue-600 hover:text-blue-800 mr-3">
            <i class="fas fa-edit"></i>
          </button>
          <button v-if="!item.deletedAt" @click="confirmDelete(item)" class="text-red-600 hover:text-red-800">
            <i class="fas fa-trash"></i>
          </button>
          <button v-else @click="confirmRestore(item)" class="text-green-600 hover:text-green-800">
            <i class="fas fa-undo"></i>
          </button>
        </td>
      </tr>
    </tbody>
  </table>
</div>
```

### 4.5 Modal genérico

```html
<!-- Modal de criação / edição -->
<div v-if="showAddModal" class="fixed inset-0 z-50 flex items-center justify-center">
  <div class="absolute inset-0 bg-black/50" @click="showAddModal = false"></div>
  <div class="relative bg-white dark:bg-gray-800 rounded-xl shadow-2xl w-full max-w-md mx-4 p-6">
    <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Nova Entidade</h3>

    <form @submit.prevent="handleAdd" class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Campo 1</label>
        <input
          v-model="addForm.campo1"
          type="text"
          required
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div class="flex justify-end gap-3 pt-4">
        <button type="button" @click="showAddModal = false"
          class="px-4 py-2 text-gray-700 dark:text-gray-300 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700">
          Cancelar
        </button>
        <button type="submit" :disabled="addLoading"
          class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50">
          {{ addLoading ? 'Salvando...' : 'Salvar' }}
        </button>
      </div>
    </form>
  </div>
</div>
```

### 4.6 Modal de confirmação

```html
<div v-if="showConfirmModal" class="fixed inset-0 z-50 flex items-center justify-center">
  <div class="absolute inset-0 bg-black/50" @click="showConfirmModal = false"></div>
  <div class="relative bg-white dark:bg-gray-800 rounded-xl shadow-2xl w-full max-w-sm mx-4 p-6 text-center">
    <i class="fas fa-exclamation-triangle text-4xl text-yellow-500 mb-4"></i>
    <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">{{ confirmTitle }}</h3>
    <p class="text-gray-600 dark:text-gray-400 mb-6">{{ confirmMessage }}</p>

    <div class="flex justify-center gap-3">
      <button @click="showConfirmModal = false"
        class="px-4 py-2 text-gray-700 dark:text-gray-300 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700">
        Cancelar
      </button>
      <button @click="handleConfirm" :disabled="confirmLoading"
        class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 disabled:opacity-50">
        {{ confirmLoading ? 'Aguarde...' : 'Confirmar' }}
      </button>
    </div>
  </div>
</div>
```

---

## 5. Convenções de estilo

| Elemento | Classes Tailwind |
|---|---|
| Container de página | `<div class="p-6">` (padding padrão do layout) |
| Card / seção | `bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700` |
| Botão primário | `bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg transition` |
| Botão secundário | `border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700` |
| Botão danger | `bg-red-600 hover:bg-red-700 text-white` |
| Botão success | `bg-green-600 hover:bg-green-700 text-white` |
| Input | `w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500` |
| Label | `block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1` |
| Badge ativo | `bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300 px-2 py-1 text-xs font-medium rounded-full` |
| Badge inativo | `bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300 px-2 py-1 text-xs font-medium rounded-full` |
| Título da página | `text-2xl font-bold text-gray-900 dark:text-white` |
| Subtítulo | `text-gray-600 dark:text-gray-400` |
| Table header | `bg-gray-50 dark:bg-gray-700/50 text-xs font-medium text-gray-500 uppercase` |
| Table row hover | `hover:bg-gray-50 dark:hover:bg-gray-700/30` |
| Modal overlay | `fixed inset-0 z-50 bg-black/50` |
| Modal box | `bg-white dark:bg-gray-800 rounded-xl shadow-2xl w-full max-w-md mx-4 p-6` |

---

## 6. Checklist para nova tela

- [ ] Criar arquivo de serviço em `src/services/*.service.ts`
- [ ] Criar view em `src/features/session/views/MinhaPage.vue`
- [ ] Seguir o esqueleto do script setup (seções 1-10)
- [ ] Seguir o esqueleto do template (seções 1-8)
- [ ] Usar classes Tailwind da tabela de convenções
- [ ] Adicionar rota em `router/index.ts`
- [ ] Adicionar view no `DatabaseSeeder.cs` (backend) se for tela com permissão
- [ ] Testar: loading, empty state, criação, edição, exclusão, restauração
- [ ] Testar: dark mode, responsividade, toast notifications
