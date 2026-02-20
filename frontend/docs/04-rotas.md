# Rotas

## Visão Geral

O roteamento é gerenciado pelo Vue Router 4 com guards de autenticação e verificação de permissões.

## Tabela de Rotas

| Caminho | Nome | Componente | Auth | Descrição |
|---|---|---|---|---|
| `/` | `home` | `site/views/HomePage.vue` | Não | Landing page pública |
| `/login` | `login` | `auth/views/LoginPage.vue` | Não | Página de login |
| `/session` | — | `session/layouts/SessionLayout.vue` | Sim | Layout da área autenticada |
| `/session/home` | `home-session` | `session/views/HomePage.vue` | Sim | Dashboard |
| `/session/users` | `users` | `session/views/UsersPage.vue` | Sim | Gerenciamento de usuários |
| `/session/clients` | `clients` | `session/views/ClientsPage.vue` | Sim | Gerenciamento de clientes |
| `/session/history` | `history` | `session/views/HistoryPage.vue` | Sim | Histórico de conversas |
| `/session/monitoring` | `monitoring` | `session/views/MonitoringPage.vue` | Sim | Monitoramento do sistema |
| `/session/customer-service` | `customer-service` | `session/views/CustomerServicePage.vue` | Sim | Chat de atendimento |
| `/:pathMatch(.*)*` | `not-found` | `site/views/NotFoundPage.vue` | Não | Página 404 |

## Estrutura de Rotas

```typescript
const routes = [
  // Públicas
  { path: '/', name: 'home', component: HomePage, meta: { requiresAuth: false } },
  { path: '/login', name: 'login', component: LoginPage, meta: { requiresAuth: false } },

  // Área autenticada (layout pai)
  {
    path: '/session',
    component: SessionLayout,
    meta: { requiresAuth: true },
    children: [
      { path: 'home', name: 'home-session', component: HomePage },
      { path: 'users', name: 'users', component: UsersPage },
      { path: 'clients', name: 'clients', component: ClientsPage },
      { path: 'history', name: 'history', component: HistoryPage },
      { path: 'monitoring', name: 'monitoring', component: MonitoringPage },
      { path: 'customer-service', name: 'customer-service', component: CustomerServicePage },
    ]
  },

  // Catch-all
  { path: '/:pathMatch(.*)*', name: 'not-found', component: NotFoundPage }
]
```

## Guards de Navegação

O `beforeEach` guard implementa:

### 1. Verificação de Autenticação

```
Se rota requer auth E não tem token → redireciona para /login
```

### 2. Validação de Sessão

```
Se tem token → chama authStore.checkAuth()
  Se sessão inválida/expirada → limpa sessão + redireciona para /login com toast
```

### 3. Verificação de Permissão

```
Se usuário logado → verifica se a rota está nas views do usuário
  Se não autorizado → redireciona para /session/home com toast de erro
```

### 4. Redirect de Logado

```
Se já logado E visitando /login → redireciona para /session/home
```

## Controle de Acesso por Permissão

As rotas da área autenticada são controladas pelas **Views** associadas à permissão do usuário (definidas no backend):

| Permissão | Views Disponíveis |
|---|---|
| **admin** | home, customer_service, clients, history, users, monitoring |
| **operator** | home, customer_service |

O guard compara `user.views[].route` com o path da rota para permitir ou negar acesso.

## SessionLayout

O `SessionLayout.vue` renderiza:

1. **Sidebar**: Menu lateral colapsável com links dinâmicos baseados nas views do usuário
2. **Header**: Barra superior com informações do usuário
3. **RouterView**: Área de conteúdo para as views filhas

O menu da sidebar é gerado dinamicamente a partir de `user.views`, garantindo que cada usuário só veja os itens que tem permissão.
