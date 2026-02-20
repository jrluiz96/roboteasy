# Stores Pinia

## Visão Geral

O estado global é gerenciado pelo Pinia 3. As stores utilizam a Composition API (`setup stores` ou `defineStore` com options).

## authStore

**Store ID**: `auth`

**Localização**: `features/auth/stores/authStore.ts`

### Estado

| Propriedade | Tipo | Descrição |
|---|---|---|
| `user` | `User \| null` | Dados do usuário logado |
| `token` | `string \| null` | JWT (inicializado do localStorage) |
| `loading` | `boolean` | Indica operação em andamento |
| `error` | `string \| null` | Mensagem de erro |

### Getters

| Getter | Retorno | Descrição |
|---|---|---|
| `isAuthenticated` | `boolean` | `!!token` |
| `currentUser` | `User \| null` | Referência ao user |

### Actions

#### `login(credentials: LoginCredentials): Promise<boolean>`

1. Chama `authApi.login({ username, password })`
2. Salva token no localStorage via `api.setToken()`
3. Armazena user e token no state
4. Exibe toast de sucesso
5. Retorna `true` em caso de sucesso, `false` em caso de erro

#### `logout(): void`

1. Chama `authApi.logout()` (limpa localStorage)
2. Limpa user e token do state
3. Exibe toast "Sessão encerrada"

#### `checkAuth(): Promise<{ valid: boolean; expired?: boolean }>`

1. Chama `authApi.me()` → `GET /api/v1/session`
2. Se sucesso → atualiza user, retorna `{ valid: true }`
3. Se erro 401 → limpa sessão, retorna `{ valid: false, expired: true }`
4. Se outro erro → retorna `{ valid: false }`

### Tipo User

```typescript
interface User {
  id: number
  username: string
  email: string | null
  name?: string
  avatarUrl?: string
  permissionName?: string
  permissionId?: number
  views?: { id: number; name: string; route: string; icon: string }[]
}
```

## toastStore

**Store ID**: `toast`

**Localização**: `stores/toastStore.ts`

### Estado

| Propriedade | Tipo | Descrição |
|---|---|---|
| `toasts` | `Toast[]` | Lista de notificações ativas |

### Actions

| Action | Parâmetros | Descrição |
|---|---|---|
| `show` | `message, type?, duration?` | Exibe toast genérico |
| `success` | `message, duration?` | Toast verde de sucesso |
| `error` | `message, duration?` | Toast vermelho de erro |
| `warning` | `message, duration?` | Toast amarelo de aviso |
| `info` | `message, duration?` | Toast azul informativo |
| `remove` | `id` | Remove toast específico |
| `clear` | — | Remove todos os toasts |

Todos os métodos de criação retornam o `id` do toast (para remoção programática).

### Tipo Toast

```typescript
type ToastType = 'success' | 'error' | 'warning' | 'info'

interface Toast {
  id: number
  type: ToastType
  message: string
  duration: number
}
```

### Uso

```typescript
import { useToastStore } from '@/stores/toastStore'

const toast = useToastStore()

toast.success('Usuário criado com sucesso')
toast.error('Erro ao carregar dados')
toast.warning('Sessão expira em 5 minutos')
toast.info('Nova conversa disponível')
```

## Criando uma Nova Store

```typescript
// features/minha-feature/stores/minhaStore.ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useMinhaStore = defineStore('minha-store', () => {
  // State
  const items = ref<Item[]>([])
  const loading = ref(false)

  // Getters
  const count = computed(() => items.value.length)

  // Actions
  async function loadItems() {
    loading.value = true
    try {
      items.value = await minhaService.getAll()
    } finally {
      loading.value = false
    }
  }

  return { items, loading, count, loadItems }
})
```
