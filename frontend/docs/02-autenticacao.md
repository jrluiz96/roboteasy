# Autenticação

## Visão Geral

A autenticação utiliza **JWT Bearer** com validação de sessão no servidor. Não há OAuth ou login social — apenas username/password.

## Fluxo de Login

```
1. Usuário preenche LoginForm (username + password)
2. LoginPage chama authStore.login()
3. authStore chama authApi.login() → POST /api/v1/open/login
4. Backend valida credenciais → retorna JWT + dados do usuário
5. Token é salvo no localStorage via api.setToken()
6. authStore armazena user + token no state
7. Router redireciona para /session/home
```

## Estrutura do JWT

| Claim | Descrição |
|---|---|
| `sub` | ID do usuário |
| `username` | Username |
| `session_token` | Token de sessão único |
| `iss` | `roboteasy` |
| `aud` | `roboteasy` |
| `exp` | Expiração (padrão: 8 horas) |

## Validação de Sessão

O backend implementa **validação de sessão ativa** via `SessionValidationMiddleware`:

1. Extrai `sub` (userId) e `session_token` do JWT
2. Busca o usuário no banco
3. Verifica: usuário existe, não está soft-deleted, e `user.Token == session_token`
4. Se inválido → 401. Se válido → armazena em `HttpContext.Items["CurrentUser"]`

Isso permite **invalidação server-side**: ao fazer logout, o `Token` do usuário é setado para `null`, invalidando qualquer JWT existente.

## Frontend — authStore

```typescript
// Estado
user: User | null
token: string | null    // inicializado de localStorage
loading: boolean
error: string | null

// Getters
isAuthenticated: boolean  // !!token
currentUser: User | null

// Actions
login(credentials): Promise<boolean>
logout(): void
checkAuth(): Promise<{ valid: boolean; expired?: boolean }>
```

### `login(credentials)`
1. Chama `authApi.login({ username, password })`
2. Salva token no localStorage
3. Armazena user e token no state
4. Retorna `true` em caso de sucesso

### `logout()`
1. Chama `authApi.logout()` (limpa token do localStorage)
2. Limpa user e token do state
3. Exibe toast de sucesso

### `checkAuth()`
1. Chama `authApi.me()` → `GET /api/v1/session`
2. Se sucesso → atualiza user no state, retorna `{ valid: true }`
3. Se 401 → limpa sessão, retorna `{ valid: false, expired: true }`

## Frontend — authApi

```typescript
// features/auth/services/authApi.ts
authApi.login(credentials: LoginCredentials): Promise<AuthResponse>
authApi.me(): Promise<MeResponse>
authApi.logout(): void
```

## Frontend — auth.service.ts

```typescript
// services/auth.service.ts (serviço global auxiliar)
authService.login(credentials: LoginRequest): Promise<LoginResponse>
authService.logout(): void
authService.isAuthenticated(): boolean
```

## Guard de Rotas

Implementado no `router/index.ts` via `beforeEach`:

1. **Rota requer auth + sem token** → redireciona para `/login`
2. **Token existe** → chama `authStore.checkAuth()` para validar sessão
   - Se expirado/inválido → redireciona para `/login` com toast
3. **Verificação de permissão** → compara views do usuário com a rota
   - Se não autorizado → redireciona para `/session/home` com toast de erro
4. **Já logado + visitando `/login`** → redireciona para `/session/home`

## Token no SignalR

Para conexões WebSocket, o token JWT é passado via query string:

```typescript
// Usuário (atendente)
chatService.connectAsUser(accessToken)
// URL: /hubs/chat?access_token=<JWT>

// Cliente
chatService.connectAsClient(clientToken)
// URL: /hubs/chat?client_token=<token>
```

O backend extrai o token do query string no `OnMessageReceived` event do JWT Bearer.

## Token de Cliente

Clientes do chat usam um token separado (não JWT de usuário):

1. Cliente envia dados no `ChatWidget` (nome, email, telefone, cpf)
2. `POST /api/v1/open/chat/start` → retorna `clientToken`
3. Token JWT com audience `roboteasy-client` e expiração de 24h
4. Usado para conectar ao SignalR e identificar o cliente
5. Persistido no `localStorage` para reconexão ao recarregar página
