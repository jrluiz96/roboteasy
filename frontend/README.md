# Frontend — RobotEasy

SPA de atendimento ao cliente com chat em tempo real, construída com Vue 3 + TypeScript + Tailwind CSS 4.

## Quick Start

```bash
npm install
npm run dev
# → http://localhost:3000
```

## Scripts

| Comando | Descrição |
|---|---|
| `npm run dev` | Dev server com hot reload (porta 3000) |
| `npm run build` | Build de produção (`dist/`) |
| `npm run preview` | Preview do build |

## Documentação

| Doc | Descrição |
|---|---|
| [01-estrutura-frontend.md](docs/01-estrutura-frontend.md) | Árvore de diretórios e organização |
| [02-autenticacao.md](docs/02-autenticacao.md) | Fluxo JWT, guards, tokens |
| [03-servicos-api.md](docs/03-servicos-api.md) | Serviços HTTP e SignalR |
| [04-rotas.md](docs/04-rotas.md) | Rotas e permissões |
| [05-stores-pinia.md](docs/05-stores-pinia.md) | Stores Pinia (auth, toast) |
| [06-desenvolvimento.md](docs/06-desenvolvimento.md) | Setup, debug, convenções |
| [07-padrao-telas.md](docs/07-padrao-telas.md) | Padrão para construir novas telas |

## Tech Stack

- **Vue 3.5** + Composition API + `<script setup>`
- **TypeScript ~5.9**
- **Tailwind CSS 4.1** (via `@tailwindcss/vite`)
- **Pinia 3.0** (state management)
- **Vue Router 4.6** (SPA routing)
- **@microsoft/signalr 10.0** (WebSocket)
- **FontAwesome Free 7.2** (ícones)
- **Vite 7.3** (build + dev)
