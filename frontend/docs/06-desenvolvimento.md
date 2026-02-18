# Desenvolvimento

## Requisitos

- Node.js 20+
- npm ou pnpm

## Comandos

```bash
# Instalar dependências
npm install

# Servidor de desenvolvimento (hot reload)
npm run dev
# Acesse: http://localhost:5173

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
|------|--------|---------|
| Componentes | PascalCase | `LoginPage.vue` |
| Stores | camelCase + use | `useAuthStore` |
| Services | camelCase | `authService` |
| Types/Interfaces | PascalCase | `User`, `LoginCredentials` |

## Tailwind CSS 4

O projeto usa Tailwind 4 com integração Vite nativa:

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
