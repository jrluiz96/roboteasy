# Estrutura do Frontend

## Visão Geral

Frontend SPA desenvolvido com **Vue 3** (Composition API), **TypeScript**, **Pinia** para gerenciamento de estado e **Tailwind CSS 4** para estilização.

## Tecnologias Principais

| Tecnologia | Versão | Descrição |
|------------|--------|-----------|
| Vue.js | 3.5 | Framework reativo |
| TypeScript | 5.9 | Tipagem estática |
| Pinia | 3.0 | Gerenciamento de estado |
| Vue Router | 4.6 | Roteamento SPA |
| Tailwind CSS | 4.1 | Framework CSS utilitário |
| Vite | 7.3 | Build tool e dev server |

## Estrutura de Diretórios

```
frontend/
├── src/
│   ├── App.vue                 # Componente raiz
│   ├── main.ts                 # Entry point
│   ├── style.css               # Estilos globais (Tailwind)
│   │
│   ├── components/             # Componentes globais reutilizáveis
│   │   └── ToastContainer.vue  # Sistema de notificações
│   │
│   ├── features/               # Módulos por funcionalidade
│   │   ├── auth/               # Autenticação
│   │   ├── session/            # Área logada
│   │   └── site/               # Páginas públicas
│   │
│   ├── router/                 # Configuração de rotas
│   │   └── index.ts
│   │
│   ├── services/               # Serviços de API
│   │   ├── api.ts              # Cliente HTTP base
│   │   ├── auth.service.ts     # Endpoints de autenticação
│   │   ├── session.service.ts  # Endpoints de sessão
│   │   └── users.service.ts    # Endpoints de usuários
│   │
│   └── stores/                 # Stores globais Pinia
│       ├── index.ts            # Configuração Pinia
│       └── toastStore.ts       # Store de notificações
│
├── public/                     # Assets estáticos
├── index.html                  # HTML template
├── package.json
├── vite.config.ts
└── tsconfig.json
```

## Arquitetura Feature-Based

O frontend segue arquitetura modular por features:

### Feature `auth/`
```
auth/
├── index.ts           # Exports públicos
├── components/        # Componentes específicos
├── services/          # authApi
├── stores/            # authStore
├── types/             # Interfaces TypeScript
└── views/
    └── LoginPage.vue
```

### Feature `session/`
```
session/
├── index.ts
├── layouts/
│   └── SessionLayout.vue   # Layout com sidebar
└── views/
    ├── DashboardPage.vue
    └── PlaceholderPage.vue
```

### Feature `site/`
```
site/
├── index.ts
└── views/
    ├── HomePage.vue
    └── NotFoundPage.vue
```

## Fluxo de Dados

```
┌─────────────────────────────────────────────────────────┐
│                      Componente Vue                      │
│                                                          │
│   ┌─────────┐        ┌─────────┐        ┌─────────────┐ │
│   │ Template │◄──────│ Script  │───────►│ Pinia Store │ │
│   └─────────┘        └────┬────┘        └──────┬──────┘ │
│                           │                     │        │
└───────────────────────────│─────────────────────│────────┘
                            │                     │
                            ▼                     ▼
                    ┌──────────────┐      ┌─────────────┐
                    │   Services   │◄─────│   authApi   │
                    └──────┬───────┘      └─────────────┘
                           │
                           ▼
                    ┌──────────────┐
                    │  API Client  │
                    │   (api.ts)   │
                    └──────┬───────┘
                           │
                           ▼
                    ┌──────────────┐
                    │  Backend API │
                    │  :8080       │
                    └──────────────┘
```
