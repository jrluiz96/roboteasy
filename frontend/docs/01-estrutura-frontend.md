# Estrutura do Frontend

## Visão Geral

SPA construída com Vue 3, TypeScript, Tailwind CSS 4 e Pinia. Comunicação em tempo real via SignalR.

## Tecnologias

| Tecnologia | Versão | Uso |
|---|---|---|
| Vue | 3.5 | Framework reativo |
| TypeScript | ~5.9 | Tipagem estática |
| Tailwind CSS | 4.1 | Estilização utility-first |
| Pinia | 3.0 | State management |
| Vue Router | 4.6 | Roteamento SPA |
| @microsoft/signalr | 10.0 | WebSocket (chat em tempo real) |
| FontAwesome Free | 7.2 | Ícones |
| Vite | 7.3 | Build tool + dev server |

## Árvore de Diretórios

```
frontend/src/
├── App.vue                              # Componente raiz (RouterView + ToastContainer)
├── main.ts                              # Entry point (Pinia + Router + CSS)
├── style.css                            # Estilos globais + Tailwind import
├── vite-env.d.ts                        # Tipos Vite
├── assets/
│   └── vue.svg
├── components/
│   └── ToastContainer.vue               # Notificações toast (teleport, animações)
├── features/
│   ├── auth/                            # Feature: Autenticação
│   │   ├── index.ts                     # Exports públicos
│   │   ├── components/
│   │   │   └── LoginForm.vue            # Formulário de login (username/password)
│   │   ├── services/
│   │   │   └── authApi.ts               # API de auth (login, me, logout)
│   │   ├── stores/
│   │   │   └── authStore.ts             # Store Pinia de autenticação
│   │   ├── types/
│   │   │   └── index.ts                 # Tipos: User, LoginCredentials, AuthResponse
│   │   └── views/
│   │       └── LoginPage.vue            # Página de login
│   ├── session/                         # Feature: Área autenticada
│   │   ├── index.ts                     # Exports públicos
│   │   ├── layouts/
│   │   │   └── SessionLayout.vue        # Layout com sidebar + header + RouterView
│   │   └── views/
│   │       ├── HomePage.vue             # Dashboard (stats, atividades recentes)
│   │       ├── UsersPage.vue            # CRUD de usuários (~710 linhas)
│   │       ├── CustomerServicePage.vue  # Chat em tempo real (~706 linhas)
│   │       ├── ClientsPage.vue          # Gerenciamento de clientes (dados mockados)
│   │       ├── HistoryPage.vue          # Histórico de conversas finalizadas
│   │       ├── MonitoringPage.vue       # Monitoramento do sistema (dados simulados)
│   │       └── PlaceholderPage.vue      # Página "em construção" reutilizável
│   └── site/                            # Feature: Páginas públicas
│       ├── index.ts                     # Exports públicos
│       ├── components/
│       │   └── ChatWidget.vue           # Widget de chat flutuante (canto inferior direito)
│       └── views/
│           ├── HomePage.vue             # Landing page (hero, features, pricing)
│           └── NotFoundPage.vue         # Página 404
├── router/
│   └── index.ts                         # Definição de rotas + guards de autenticação
├── services/
│   ├── api.ts                           # ApiClient (GET, POST, PUT, PATCH, DELETE + token)
│   ├── auth.service.ts                  # Login, logout, isAuthenticated
│   ├── chat.service.ts                  # ChatService SignalR (connect, send, events)
│   ├── conversation.service.ts          # CRUD de conversas (active, history, join, leave...)
│   ├── session.service.ts               # Dados da sessão do usuário
│   ├── users.service.ts                 # CRUD de usuários + permissões
│   └── index.ts                         # Re-exports
└── stores/
    ├── index.ts                         # Instância Pinia
    └── toastStore.ts                    # Store de notificações toast
```

## Organização por Feature

O frontend segue a estrutura **feature-based**:

```
features/
├── auth/          # Tudo relacionado a autenticação
├── session/       # Área logada (layout, views)
└── site/          # Páginas públicas
```

Cada feature pode conter:
- `views/` — Páginas (componentes de rota)
- `components/` — Componentes reutilizáveis da feature
- `stores/` — Stores Pinia da feature
- `services/` — APIs da feature
- `types/` — Interfaces/tipos da feature
- `index.ts` — Exports públicos

## Serviços Compartilhados (`services/`)

Os serviços em `src/services/` são globais e usados por qualquer feature:

| Serviço | Descrição |
|---|---|
| `api.ts` | Cliente HTTP base (ApiClient singleton) |
| `auth.service.ts` | Login/logout/verificação de autenticação |
| `session.service.ts` | Dados da sessão do usuário logado |
| `users.service.ts` | CRUD de usuários e permissões |
| `conversation.service.ts` | Operações de conversas (listar, histórico, join, leave, finish, invite) |
| `chat.service.ts` | Conexão SignalR, envio/recebimento de mensagens em tempo real |

## Componentes Globais (`components/`)

| Componente | Descrição |
|---|---|
| `ToastContainer.vue` | Container fixo para notificações toast (success, error, warning, info) |

## Configuração Vite

```typescript
// vite.config.ts
plugins: [vue(), tailwindcss()]
resolve.alias: { '@': './src' }
server: {
  port: 3000,
  strictPort: true,
  proxy: {
    '/api': { target: 'http://localhost:8080', changeOrigin: true },
    '/hubs': { target: 'http://localhost:8080', changeOrigin: true, ws: true }
  }
}
```

O proxy encaminha chamadas `/api/*` e `/hubs/*` para o backend automaticamente em desenvolvimento.
