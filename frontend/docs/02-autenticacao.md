# Sistema de Autenticação

## Fluxo de Login

### Login com Usuário/Senha

```
┌─────────┐    ┌───────────┐    ┌─────────────┐    ┌─────────┐
│ LoginPage│───►│ authStore │───►│  authApi    │───►│ Backend │
└─────────┘    └───────────┘    └─────────────┘    └─────────┘
     │              │                  │                 │
     │  1. submit   │                  │                 │
     │─────────────►│  2. login()      │                 │
     │              │─────────────────►│  3. POST       │
     │              │                  │  /login        │
     │              │                  │────────────────►│
     │              │                  │                 │
     │              │                  │◄────────────────│
     │              │                  │  { token, user }│
     │              │◄─────────────────│                 │
     │              │  4. setToken()   │                 │
     │              │  5. update state │                 │
     │◄─────────────│                  │                 │
     │  6. redirect │                  │                 │
     │  /session    │                  │                 │
```

### Login com GitHub OAuth

```
1. Usuário clica "Login com GitHub"
2. Redireciona para: /api/v1/open/github/login
3. Backend redireciona para GitHub OAuth
4. GitHub autentica e retorna para /api/v1/open/github/callback
5. Backend processa, cria/atualiza usuário
6. Backend redireciona para frontend com token na URL
7. Frontend salva token e redireciona para /session
```

## authStore

```typescript
// Estado
user: User | null           // Usuário logado
token: string | null        // JWT Token
loading: boolean            // Estado de carregamento
error: string | null        // Mensagem de erro

// Getters
isAuthenticated: boolean    // Se há token válido
currentUser: User | null    // Usuário atual

// Actions
login(credentials)          // Login com usuário/senha
loginWithGitHub()           // Redireciona para OAuth
logout()                    // Encerra sessão
checkAuth()                 // Valida sessão no backend
```

## Proteção de Rotas

O router implementa navigation guard para proteger rotas:

```typescript
// router/index.ts
router.beforeEach(async (to, _from, next) => {
  const authStore = useAuthStore(pinia)
  const hasToken = !!localStorage.getItem('token')
  
  // Rota requer autenticação
  if (to.meta.requiresAuth) {
    if (!hasToken) {
      next({ name: 'login' })    // Sem token → login
      return
    }
    
    // Valida sessão no backend
    const result = await authStore.checkAuth()
    
    if (!result.valid) {
      if (result.expired) {
        toastStore.warning('Sessão expirada. Faça login novamente.')
      }
      next({ name: 'login' })
      return
    }
  }
  
  // Usuário logado tentando acessar login → dashboard
  if (to.name === 'login' && hasToken) {
    next({ name: 'dashboard' })
    return
  }
  
  next()
})
```

## Armazenamento do Token

O token JWT é armazenado em `localStorage`:

```typescript
// services/api.ts
setToken(token: string | null): void {
  this.token = token
  if (token) {
    localStorage.setItem('token', token)
  } else {
    localStorage.removeItem('token')
  }
}
```

Todas as requisições autenticadas incluem o header:
```
Authorization: Bearer {token}
```

## Interceptação de Erros

Quando o backend retorna `401 Unauthorized`:
1. `authStore.checkAuth()` detecta o erro
2. Limpa a sessão local
3. Redireciona para `/login`
4. Exibe toast "Sessão expirada"
