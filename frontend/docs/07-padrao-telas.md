# 07 - Padrão de Construção de Telas

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
  updatedAt: string
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
    const response = await api.delete<void>(`/api/v1/rota/${id}`)
    if (response.code !== 200) throw new Error(response.message)
  },

  async restore(id: number): Promise<void> {
    // Backend retorna apenas { code, message } sem data — não checar .data
    const response = await api.patch<void>(`/api/v1/rota/${id}/restore`)
    if (response.code !== 200) throw new Error(response.message)
  }
}
```

> **Regra importante:** quando o backend retorna só `message` sem `data` (ex: delete, restore), verificar apenas `response.code`, nunca `response.data` — caso contrário a mensagem de sucesso vira erro no catch.

---

## 3. Estado reativo padrão

```ts
// Lista principal + loading
const items = ref<MinhaEntidade[]>([])
const loading = ref(false)

// Filtros
const searchQuery = ref('')
const selectedStatus = ref('all')    // 'all' | 'active' | 'inactive'

// Modais
const showAdd = ref(false)
const showEdit = ref(false)
const editingItem = ref<MinhaEntidade | null>(null)
```

---

## 4. Regra de ativo/inativo

```ts
// Ativo   = deletedAt === null
// Inativo = deletedAt !== null  (soft delete — são a mesma coisa)

const activeCount   = computed(() => items.value.filter(i => !i.deletedAt).length)
const inactiveCount = computed(() => items.value.filter(i => !!i.deletedAt).length)
```

Nunca usar campo `status` separado. A única fonte de verdade é `deletedAt`.

---

## 5. Filtro computado padrão

```ts
const filteredItems = computed(() => {
  let filtered = items.value

  // Busca textual
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    filtered = filtered.filter(i =>
      i.nome.toLowerCase().includes(q) ||
      i.outroCampo?.toLowerCase().includes(q)
    )
  }

  // Filtro de status
  if (selectedStatus.value === 'active') {
    filtered = filtered.filter(i => !i.deletedAt)
  } else if (selectedStatus.value === 'inactive') {
    filtered = filtered.filter(i => !!i.deletedAt)
  }
  // 'all' → não filtra

  return filtered
})
```

---

## 6. Modal de confirmação genérico (sem `confirm()` nativo)

```ts
const confirmModal = ref({
  show: false,
  title: '',
  message: '',
  confirmLabel: 'Confirmar',
  confirmClass: 'bg-red-600 hover:bg-red-700',
  resolve: null as ((val: boolean) => void) | null
})

function showConfirm(
  title: string,
  message: string,
  confirmLabel = 'Confirmar',
  confirmClass = 'bg-red-600 hover:bg-red-700'
): Promise<boolean> {
  return new Promise(resolve => {
    confirmModal.value = { show: true, title, message, confirmLabel, confirmClass, resolve }
  })
}

function handleConfirm(result: boolean) {
  confirmModal.value.show = false
  confirmModal.value.resolve?.(result)
}
```

**Uso:**
```ts
async function deleteItem(id: number) {
  const ok = await showConfirm(
    'Excluir item',
    'Esta ação não pode ser desfeita.',
    'Excluir',
    'bg-red-600 hover:bg-red-700'
  )
  if (!ok) return
  // ... chama serviço
}
```

**Cores de confirmação por tipo de ação:**
| Ação     | `confirmClass`                        |
|----------|---------------------------------------|
| Excluir  | `bg-red-600 hover:bg-red-700`         |
| Restaurar| `bg-green-600 hover:bg-green-700`     |
| Resetar  | `bg-blue-600 hover:bg-blue-700`       |
| Aviso    | `bg-yellow-600 hover:bg-yellow-700`   |

**Template do modal de confirmação** (colar antes do `</div>` que fecha o template):

```html
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
      <button @click="handleConfirm(false)"
        class="px-4 py-2 text-sm bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-lg transition">
        Cancelar
      </button>
      <button @click="handleConfirm(true)"
        :class="['px-4 py-2 text-sm text-white rounded-lg transition', confirmModal.confirmClass]">
        {{ confirmModal.confirmLabel }}
      </button>
    </div>
  </div>
</div>
```

---

## 7. Funções CRUD padrão

```ts
async function loadItems() {
  loading.value = true
  try {
    items.value = await minhaEntidadeService.getAll()
  } catch (error: any) {
    toastStore.error('Erro ao carregar: ' + error.message)
  } finally {
    loading.value = false
  }
}

async function addItem() {
  try {
    const created = await minhaEntidadeService.create(newItem.value)
    items.value.push(created)
    showAdd.value = false
    toastStore.success('Criado com sucesso')
  } catch (error: any) {
    toastStore.error('Erro ao criar: ' + error.message)
  }
}

async function updateItem() {
  if (!editingItem.value) return
  try {
    await minhaEntidadeService.update(editingItem.value.id, { /* campos */ })
    await loadItems()
    showEdit.value = false
    editingItem.value = null
    toastStore.success('Atualizado com sucesso')
  } catch (error: any) {
    toastStore.error('Erro ao atualizar: ' + error.message)
  }
}

async function deleteItem(id: number) {
  const ok = await showConfirm('Excluir', 'Tem certeza?', 'Excluir', 'bg-red-600 hover:bg-red-700')
  if (!ok) return
  try {
    await minhaEntidadeService.delete(id)
    await loadItems()
    toastStore.success('Excluído com sucesso')
  } catch (error: any) {
    toastStore.error('Erro ao excluir: ' + error.message)
  }
}

async function restoreItem(id: number) {
  const ok = await showConfirm('Restaurar', 'Tem certeza?', 'Restaurar', 'bg-green-600 hover:bg-green-700')
  if (!ok) return
  try {
    await minhaEntidadeService.restore(id)
    await loadItems()
    toastStore.success('Restaurado com sucesso')
  } catch (error: any) {
    toastStore.error('Erro ao restaurar: ' + error.message)
  }
}
```

---

## 8. onMounted

```ts
onMounted(async () => {
  await Promise.all([
    loadItems(),
    loadOutraCoisaSeNecessario()
  ])
})
```

Sempre usar `Promise.all` para carregar dados em paralelo.

---

## 9. Utilidades de template

### Badge de status
```ts
function getStatusColor(isActive: boolean) {
  return isActive
    ? 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400'
    : 'bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400'
}
```
```html
<span :class="['px-2 py-1 text-xs font-medium rounded-full', getStatusColor(!item.deletedAt)]">
  {{ !item.deletedAt ? 'Ativo' : 'Inativo' }}
</span>
```

### Formatação de data
```ts
function formatDate(date: string | null | undefined): string {
  if (!date) return 'Nunca'
  return new Date(date).toLocaleDateString('pt-BR', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}
```

---

## 10. Estrutura do template

### Header
```html
<div class="flex justify-between items-center w-full min-w-0">
  <div class="min-w-0 flex-1">
    <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Título da Tela</h1>
    <p class="text-sm text-gray-600 dark:text-gray-400">Subtítulo descritivo</p>
  </div>
  <button @click="showAdd = true"
    class="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition">
    <i class="fas fa-plus mr-2"></i> Novo Item
  </button>
</div>
```

### Stats cards
```html
<div class="grid grid-cols-1 md:grid-cols-4 gap-4 w-full">
  <!-- Repetir para cada stat -->
  <div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700 card-container">
    <div class="flex items-center">
      <div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/20 rounded-lg flex items-center justify-center">
        <i class="fas fa-icon text-blue-600 dark:text-blue-400"></i>
      </div>
      <div class="ml-3">
        <p class="text-sm text-gray-600 dark:text-gray-400">Label</p>
        <p class="text-lg font-semibold text-gray-900 dark:text-white">
          {{ loading ? '...' : count }}
        </p>
      </div>
    </div>
  </div>
</div>
```

### Filtros
```html
<div class="bg-white dark:bg-gray-800 p-4 rounded-lg border border-gray-200 dark:border-gray-700 w-full card-container">
  <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
    <!-- Busca -->
    <div class="relative">
      <i class="fas fa-search absolute left-3 top-1/2 -translate-y-1/2 text-gray-400"></i>
      <input v-model="searchQuery" type="text" placeholder="Buscar..."
        class="w-full pl-10 pr-4 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white" />
    </div>
    <!-- Select de status -->
    <select v-model="selectedStatus"
      class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white">
      <option value="all">Todos os status</option>
      <option value="active">Ativos</option>
      <option value="inactive">Inativos</option>
    </select>
  </div>
</div>
```

### Tabela
```html
<div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden w-full card-container">
  <div class="overflow-x-auto w-full overflow-container">
    <table class="w-full">
      <thead class="bg-gray-50 dark:bg-gray-700">
        <tr>
          <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Coluna</th>
          <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">Ações</th>
        </tr>
      </thead>
      <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
        <tr v-if="loading">
          <td colspan="X" class="px-6 py-8 text-center text-gray-500 dark:text-gray-400">
            <i class="fas fa-spinner fa-spin mr-2"></i> Carregando...
          </td>
        </tr>
        <tr v-else-if="filteredItems.length === 0">
          <td colspan="X" class="px-6 py-8 text-center text-gray-500 dark:text-gray-400">
            Nenhum item encontrado
          </td>
        </tr>
        <tr v-else v-for="item in filteredItems" :key="item.id"
          class="hover:bg-gray-50 dark:hover:bg-gray-700/50">
          <!-- colunas -->
          <td class="px-6 py-4 text-right space-x-2">
            <template v-if="item.deletedAt">
              <button @click="restoreItem(item.id)"
                class="text-green-600 dark:text-green-400 hover:text-green-900" title="Restaurar">
                <i class="fas fa-undo"></i>
              </button>
            </template>
            <template v-else>
              <button @click="editItem(item)"
                class="text-purple-600 dark:text-purple-400 hover:text-purple-900">
                <i class="fas fa-edit"></i>
              </button>
              <button @click="deleteItem(item.id)"
                class="text-red-600 dark:text-red-400 hover:text-red-900">
                <i class="fas fa-trash"></i>
              </button>
            </template>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</div>
```

### Modal genérico (add/edit)
```html
<div v-if="showAdd" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
  <div class="bg-white dark:bg-gray-800 rounded-lg p-6 w-full max-w-md mx-4">
    <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Novo Item</h3>
    <div class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Campo *</label>
        <input v-model="newItem.campo" type="text"
          class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white" />
      </div>
    </div>
    <div class="flex gap-3 mt-6">
      <button @click="addItem" :disabled="!newItem.campo"
        class="flex-1 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed">
        Adicionar
      </button>
      <button @click="showAdd = false"
        class="flex-1 px-4 py-2 bg-gray-300 hover:bg-gray-400 text-gray-700 rounded-lg transition">
        Cancelar
      </button>
    </div>
  </div>
</div>
```

---

## 11. Classes CSS reutilizáveis

| Uso                         | Classes                                                                                   |
|-----------------------------|-------------------------------------------------------------------------------------------|
| Card container              | `bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 card-container` |
| Input padrão                | `w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent dark:bg-gray-700 dark:text-white` |
| Botão primário              | `px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition`           |
| Botão cancelar              | `px-4 py-2 bg-gray-300 hover:bg-gray-400 text-gray-700 rounded-lg transition`            |
| Botão cancelar (dark aware) | `px-4 py-2 bg-gray-200 hover:bg-gray-300 dark:bg-gray-700 dark:hover:bg-gray-600 text-gray-700 dark:text-gray-300 rounded-lg transition` |
| Botão desabilitado          | adicionar `disabled:opacity-50 disabled:cursor-not-allowed`                               |
| Badge verde (ativo)         | `px-2 py-1 text-xs font-medium rounded-full bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400` |
| Badge cinza (inativo)       | `px-2 py-1 text-xs font-medium rounded-full bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400` |
| Ação editar                 | `text-purple-600 dark:text-purple-400 hover:text-purple-900 dark:hover:text-purple-300`  |
| Ação excluir                | `text-red-600 dark:text-red-400 hover:text-red-900 dark:hover:text-red-300`              |
| Ação restaurar              | `text-green-600 dark:text-green-400 hover:text-green-900 dark:hover:text-green-300`      |

---

## 12. Checklist ao criar uma nova tela

- [ ] Criar `src/services/novaEntidade.service.ts` com interfaces e métodos CRUD
- [ ] Registrar a rota em `src/router/index.ts` (dentro de `/session`)
- [ ] Garantir que a view criada existe em `src/features/session/views/`
- [ ] Incluir o modal de confirmação genérico no template
- [ ] Nunca usar `confirm()` nativo do browser
- [ ] Nunca usar campo `status` — usar apenas `deletedAt`
- [ ] Seguir ordem dos blocos no `<script setup>` descrita na seção 1
- [ ] `onMounted` com `Promise.all` para cargas em paralelo
- [ ] Todos os erros capturados com `toastStore.error()`
