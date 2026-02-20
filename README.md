# RobotEasy

Sistema de atendimento ao cliente com chat em tempo real via WebSocket.

## Visão Geral

- **Backend**: API REST + SignalR Hub (.NET 10, PostgreSQL 16)
- **Frontend**: SPA (Vue 3, TypeScript, Tailwind CSS 4)
- **Infra**: Docker Compose (API + PostgreSQL)

## Funcionalidades

- Autenticação JWT com validação de sessão server-side
- CRUD de usuários com soft delete e permissões (admin/operator)
- Chat em tempo real (SignalR) entre clientes e atendentes
- Widget de chat público para clientes (sem login)
- Gerenciamento de conversas: puxar, convidar, sair, finalizar
- Indicadores de digitação, leitura e status online/offline
- Histórico de conversas finalizadas
- Controle de acesso por permissão (views dinâmicas)
- Dark mode + responsivo

## Arquitetura

```
┌─────────────┐     HTTP/WS      ┌─────────────┐     SQL      ┌──────────────┐
│   Frontend   │ ◄──────────────► │   Backend   │ ◄──────────► │  PostgreSQL   │
│  Vue 3 SPA   │    :3000→:8080   │  .NET 10    │              │    16         │
│  Tailwind    │                  │  SignalR     │              │              │
└─────────────┘                  └─────────────┘              └──────────────┘
```

## Quick Start

### Com Docker (backend + banco)

```bash
docker-compose up --build -d
# API: http://localhost:8080
# Swagger: http://localhost:8080/swagger
```

### Frontend (desenvolvimento)

```bash
cd frontend
npm install
npm run dev
# App: http://localhost:3000
```

### Credenciais padrão (seed)

| Usuário | Senha | Permissão |
|---|---|---|
| `admin.master` | `MyAdm2026TestCode` | admin (todas as telas) |
| `francisco.luiz` | `CodandoEmC#` | operator (home + atendimento) |

## Tech Stack

### Backend

| Tecnologia | Versão |
|---|---|
| .NET | 10.0 |
| Entity Framework Core | 10.0 |
| PostgreSQL | 16 |
| SignalR | — |
| Argon2id | — |
| JWT Bearer | — |

### Frontend

| Tecnologia | Versão |
|---|---|
| Vue | 3.5 |
| TypeScript | ~5.9 |
| Tailwind CSS | 4.1 |
| Pinia | 3.0 |
| Vue Router | 4.6 |
| @microsoft/signalr | 10.0 |
| Vite | 7.3 |

## Estrutura do Projeto

```
roboteasy/
├── docker-compose.yml       # API + PostgreSQL
├── README.md                # Este arquivo
├── backend/Api/
│   ├── Dockerfile
│   ├── Program.cs
│   ├── Docs/                # Documentação do backend
│   │   ├── 01-estrutura-backend.md
│   │   ├── 02-logica-banco-dados.md
│   │   └── 03-variaveis-ambiente.md
│   └── src/
│       ├── Configuration/   # DI, Seeder, Settings
│       ├── Contracts/       # Requests + Responses
│       ├── Controllers/     # 4 controllers
│       ├── Data/            # AppDbContext
│       ├── Hubs/            # ChatHub + ChatEvents
│       ├── Middleware/      # CORS, Error, Session
│       ├── Models/          # 8 entidades
│       ├── Repositories/    # 6 repositories (interface + impl)
│       └── Services/        # 6 services (interface + impl)
└── frontend/
    ├── docs/                # Documentação do frontend
    │   ├── 01-estrutura-frontend.md
    │   ├── 02-autenticacao.md
    │   ├── 03-servicos-api.md
    │   ├── 04-rotas.md
    │   ├── 05-stores-pinia.md
    │   ├── 06-desenvolvimento.md
    │   └── 07-padrao-telas.md
    └── src/
        ├── components/      # Componentes globais
        ├── features/        # auth, session, site
        ├── router/          # Rotas + guards
        ├── services/        # HTTP + SignalR clients
        └── stores/          # Pinia stores
```

## Padrão Arquitetural (Backend)

```
Controller → Service → Repository → AppDbContext
```

- `AppDbContext` só é acessado pelos Repositories
- Todas as respostas seguem `ApiResponse<T>` (`{ code, status, message, data }`)
- Hash de senhas com Argon2id (não BCrypt)

## Endpoints

| Área | Rota Base | Auth |
|---|---|---|
| Login / Chat Start | `/api/v1/open` | Não |
| Sessão | `/api/v1/session` | Sim |
| Usuários | `/api/v1/users` | Sim |
| Conversas | `/api/v1/conversations` | Sim |
| Chat WebSocket | `/hubs/chat` | Sim (JWT ou client token) |

## Documentação

### Backend
- [Estrutura](backend/Api/Docs/01-estrutura-backend.md) — Árvore, endpoints, SignalR, middleware, DI
- [Banco de Dados](backend/Api/Docs/02-logica-banco-dados.md) — Modelos, relacionamentos, seed
- [Variáveis de Ambiente](backend/Api/Docs/03-variaveis-ambiente.md) — Configuração via env vars

### Frontend
- [Estrutura](frontend/docs/01-estrutura-frontend.md) — Organização de diretórios e features
- [Autenticação](frontend/docs/02-autenticacao.md) — Fluxo JWT, guards, tokens
- [Serviços/API](frontend/docs/03-servicos-api.md) — HTTP client, serviços, SignalR
- [Rotas](frontend/docs/04-rotas.md) — Definição de rotas e permissões
- [Stores Pinia](frontend/docs/05-stores-pinia.md) — Estado global (auth, toast)
- [Desenvolvimento](frontend/docs/06-desenvolvimento.md) — Setup, debug, convenções
- [Padrão de Telas](frontend/docs/07-padrao-telas.md) — Template para novas telas CRUD
