# Gerenciamento de Estado (Pinia)

## Visão Geral

O estado global é gerenciado pelo **Pinia 3**, usando a **Composition API** syntax (setup stores).

## Configuração

```typescript
// stores/index.ts
import { createPinia } from 'pinia'

const pinia = createPinia()

export default pinia
export { useToastStore } from './toastStore'
```

```typescript
// main.ts
import pinia from '@/stores'

app.use(pinia)
```

## Stores Disponíveis

### toastStore (global)

Sistema de notificações toast.

```typescript
// Uso
const toastStore = useToastStore()

toastStore.success('Operação realizada!')
toastStore.error('Algo deu errado')
toastStore.warning('Atenção!')
toastStore.info('Informação')

// Com duração customizada (ms)
toastStore.success('Mensagem', 6000)
```

**Estado:**
```typescript
toasts: Toast[]  // Lista de toasts ativos

interface Toast {
  id: number
  type: 'success' | 'error' | 'warning' | 'info'
  message: string
  duration: number
}
```

**Actions:**
```typescript
show(message, type, duration)  // Exibe toast genérico
success(message, duration?)    // Toast de sucesso
error(message, duration?)      // Toast de erro
warning(message, duration?)    // Toast de aviso
info(message, duration?)       // Toast informativo
remove(id)                     // Remove toast específico
```

### authStore (feature auth)

Gerencia autenticação do usuário.

```typescript
const authStore = useAuthStore()

// Login
await authStore.login({ username: 'user', password: 'pass' })

// Verificar auth
const result = await authStore.checkAuth()
if (!result.valid) {
  // Sessão inválida
}

// Logout
authStore.logout()
```

**Estado:**
```typescript
user: User | null        // Usuário logado
token: string | null     // JWT Token
loading: boolean         // Estado de loading
error: string | null     // Mensagem de erro
```

**Getters:**
```typescript
isAuthenticated: boolean    // Se há sessão ativa
currentUser: User | null    // Dados do usuário
```

**Actions:**
```typescript
login(credentials)          // Login com usuário/senha
loginWithGitHub()           // Redireciona para OAuth
logout()                    // Encerra sessão
checkAuth()                 // Valida sessão no backend
clearSession()              // Limpa dados locais
```

## Padrão de Setup Store

```typescript
export const useMyStore = defineStore('myStore', () => {
  // State (ref)
  const count = ref(0)
  
  // Getters (computed)
  const double = computed(() => count.value * 2)
  
  // Actions (functions)
  function increment() {
    count.value++
  }
  
  // Expor publicamente
  return {
    count,
    double,
    increment
  }
})
```

## Acessando Stores Fora de Componentes

Para usar stores em arquivos que não são componentes (router, serviços):

```typescript
import pinia from '@/stores'
import { useAuthStore } from '@/features/auth'

// Passa o pinia explicitamente
const authStore = useAuthStore(pinia)
```
