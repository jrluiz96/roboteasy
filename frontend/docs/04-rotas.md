# Sistema de Rotas

## Visão Geral

O roteamento usa **Vue Router 4** com history mode (URLs limpas sem hash).

## Mapa de Rotas

```
/                   → HomePage (pública)
/login              → LoginPage (pública)
/session            → SessionLayout (protegida)
  /session          → DashboardPage
  /session/robots   → PlaceholderPage
  /session/clients  → PlaceholderPage
  /session/reports  → PlaceholderPage
  /session/settings → PlaceholderPage
/*                  → NotFoundPage (404)
```

## Estrutura das Rotas

### Rotas Públicas

```typescript
{
  path: '/',
  name: 'home',
  component: () => import('@/features/site/views/HomePage.vue'),
  meta: { requiresAuth: false }
},
{
  path: '/login',
  name: 'login',
  component: () => import('@/features/auth/views/LoginPage.vue'),
  meta: { requiresAuth: false }
}
```

### Rotas Protegidas

```typescript
{
  path: '/session',
  component: () => import('@/features/session/layouts/SessionLayout.vue'),
  meta: { requiresAuth: true },
  children: [
    {
      path: '',
      name: 'dashboard',
      component: () => import('@/features/session/views/DashboardPage.vue')
    },
    {
      path: 'robots',
      name: 'robots',
      component: () => import('@/features/session/views/PlaceholderPage.vue'),
      props: { title: 'Robôs', description: 'Gerencie seus robôs de automação.' }
    },
    // ...
  ]
}
```

## Meta Fields

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `requiresAuth` | boolean | Se a rota requer autenticação |

## Lazy Loading

Todas as rotas usam **dynamic imports** para code splitting:

```typescript
component: () => import('@/features/session/views/DashboardPage.vue')
```

Isso divide o bundle em chunks menores, carregando cada página sob demanda.

## Navigation Guards

### Global Before Guard

```typescript
router.beforeEach(async (to, _from, next) => {
  // 1. Verifica se rota requer auth
  if (to.meta.requiresAuth) {
    // 2. Sem token? → login
    if (!hasToken) {
      next({ name: 'login' })
      return
    }
    
    // 3. Valida sessão no backend
    const result = await authStore.checkAuth()
    
    if (!result.valid) {
      // 4. Sessão inválida/expirada → login com toast
      next({ name: 'login' })
      return
    }
  }
  
  // 5. Usuário logado em /login → dashboard
  if (to.name === 'login' && hasToken) {
    next({ name: 'dashboard' })
    return
  }
  
  next()
})
```

## Navegação Programática

```typescript
import { useRouter } from 'vue-router'

const router = useRouter()

// Por nome
router.push({ name: 'dashboard' })

// Por path
router.push('/session/robots')

// Com parâmetros
router.push({ name: 'user', params: { id: 1 } })

// Substituir (sem histórico)
router.replace({ name: 'login' })
```

## Layouts

O sistema usa **nested routes** para layouts:

```
┌────────────────────────────────────────────┐
│                SessionLayout               │
│ ┌──────────┐ ┌──────────────────────────┐  │
│ │          │ │                          │  │
│ │ Sidebar  │ │      <RouterView>        │  │
│ │          │ │       (DashboardPage)    │  │
│ │          │ │                          │  │
│ └──────────┘ └──────────────────────────┘  │
└────────────────────────────────────────────┘
```

O `SessionLayout.vue` contém a sidebar e renderiza as páginas filhas via `<RouterView>`.
