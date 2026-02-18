# Serviços de API

## Cliente HTTP (api.ts)

Classe `ApiClient` centraliza todas as requisições HTTP:

```typescript
const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8080'

class ApiClient {
  private baseUrl: string
  private token: string | null

  // Métodos HTTP
  get<T>(endpoint: string): Promise<ApiResponse<T>>
  post<T>(endpoint: string, body?): Promise<ApiResponse<T>>
  put<T>(endpoint: string, body?): Promise<ApiResponse<T>>
  patch<T>(endpoint: string, body?): Promise<ApiResponse<T>>
  delete<T>(endpoint: string): Promise<ApiResponse<T>>

  // Gerenciamento de token
  setToken(token: string | null): void
  getToken(): string | null
}
```

### Configuração Base URL

Definida via variável de ambiente:

```bash
# .env.local (desenvolvimento)
VITE_API_URL=http://localhost:8080

# .env.production
VITE_API_URL=https://api.roboteasy.com
```

### Formato de Resposta

Todas as respostas seguem o padrão:

```typescript
interface ApiResponse<T> {
  code: number      // Código HTTP
  message: string   // Mensagem descritiva
  data: T           // Dados retornados
}

interface ApiError {
  code: number
  message: string
  data: null
}
```

## Serviços Disponíveis

### authService (auth.service.ts)

```typescript
// Login com credenciais
authService.login({ username, password })
// Retorna: { token, user }

// URL do OAuth GitHub
authService.getGitHubLoginUrl()
// Retorna: "http://localhost:8080/api/v1/open/github/login"

// Logout local
authService.logout()

// Verifica se há token
authService.isAuthenticated()
```

### sessionService (session.service.ts)

```typescript
// Obtém dados da sessão atual
sessionService.get()
// Retorna: Session { id, name, username, email, avatarUrl, permissionId, ... }
```

### authApi (features/auth/services/authApi.ts)

API específica da feature auth:

```typescript
// Login
authApi.login({ username, password })

// Dados do usuário logado
authApi.me()

// Logout (limpa token)
authApi.logout()
```

## Endpoints Consumidos

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/v1/open/login` | Login com usuário/senha |
| GET | `/api/v1/open/github/login` | Inicia OAuth GitHub |
| GET | `/api/v1/session` | Dados da sessão atual |
| GET | `/api/v1/session/me` | Dados do usuário logado |
| POST | `/api/v1/session/logout` | Logout (invalida sessão) |

## Tratamento de Erros

```typescript
try {
  const response = await api.post('/endpoint', data)
  // Sucesso
} catch (e: unknown) {
  const error = e as ApiError
  console.error(error.message)  // "Credenciais inválidas"
  console.error(error.code)     // 401
}
```
