# Desenvolvimento

## Requisitos

- Node.js 20+
- npm

## Comandos

```bash
# Instalar dependências
npm install

# Servidor de desenvolvimento (hot reload)
npm run dev
# Acesse: http://localhost:3000

# Build para produção
npm run build
# Output: dist/

# Preview do build de produção
npm run preview
```

## Variáveis de Ambiente

Crie arquivos `.env` na raiz do frontend:

```bash
# .env.local (desenvolvimento)
VITE_API_URL=http://localhost:8080

# .env.production (produção)
VITE_API_URL=https://api.roboteasy.com
```

> Prefixo `VITE_` é obrigatório para variáveis expostas ao frontend.

> Em desenvolvimento, o Vite proxy encaminha `/api/*` e `/hubs/*` para `http://localhost:8080` automaticamente (configurado em `vite.config.ts`), então `VITE_API_URL` pode ser omitido.

## Estrutura de Componentes

### Single File Component (SFC)

```vue
<script setup lang="ts">
// Composition API + TypeScript
import { ref, computed } from 'vue'

const count = ref(0)
const double = computed(() => count.value * 2)

function increment() {
  count.value++
}
</script>

<template>
  <div class="p-4">
    <p>Count: {{ count }}</p>
    <p>Double: {{ double }}</p>
    <button @click="increment">+1</button>
  </div>
</template>
```

### Convenções de Nomenclatura

| Tipo | Padrão | Exemplo |
|---|---|---|
| Componentes | PascalCase | `LoginPage.vue` |
| Stores | camelCase + use | `useAuthStore` |
| Services | camelCase | `authService` |
| Types/Interfaces | PascalCase | `User`, `LoginCredentials` |

## Tailwind CSS 4

O projeto usa Tailwind CSS 4 com integração Vite nativa:

```typescript
// vite.config.ts
import tailwindcss from '@tailwindcss/vite'

export default {
  plugins: [tailwindcss()]
}
```

```css
/* style.css */
@import "tailwindcss";
```

## Debug

### Vue DevTools

Instale a extensão [Vue DevTools](https://devtools.vuejs.org/) no navegador para:
- Inspecionar componentes
- Visualizar estado Pinia
- Timeline de eventos

### Logs de Rede

No DevTools do navegador (F12):
1. Aba **Network**
2. Filtrar por **Fetch/XHR**
3. Visualizar requests para o backend

## Estrutura de Imports

Aliases configurados no `vite.config.ts`:

```typescript
// Ao invés de:
import { api } from '../../../services/api'

// Use:
import { api } from '@/services/api'
```

`@` = `src/`

## Adicionando Nova Feature

1. Criar pasta em `src/features/minha-feature/`
2. Estrutura mínima:
   ```
   minha-feature/
   ├── index.ts          # Exports públicos
   ├── views/
   │   └── MinhaPage.vue
   ├── stores/           # (opcional)
   ├── services/         # (opcional)
   └── types/            # (opcional)
   ```
3. Adicionar rota em `router/index.ts`
4. Exportar em `index.ts`:
   ```typescript
   export { useMinhaStore } from './stores/minhaStore'
   export type * from './types'
   ```

## Dependências

### Produção

| Pacote | Versão | Uso |
|---|---|---|
| `vue` | ^3.5 | Framework reativo |
| `vue-router` | ^4.6 | Roteamento SPA |
| `pinia` | ^3.0 | State management |
| `@microsoft/signalr` | ^10.0 | WebSocket (chat real-time) |
| `@fortawesome/fontawesome-free` | ^7.2 | Ícones |

### Desenvolvimento

| Pacote | Versão | Uso |
|---|---|---|
| `vite` | ^7.3 | Build tool + dev server |
| `typescript` | ~5.9 | Tipagem estática |
| `tailwindcss` | ^4.1 | Estilização |
| `@tailwindcss/vite` | ^4.1 | Plugin Vite para Tailwind |
| `@vitejs/plugin-vue` | ^6.0 | Plugin Vite para Vue SFC |
| `vue-tsc` | ^3.1 | Type checking de .vue |
