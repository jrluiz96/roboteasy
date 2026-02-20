# Serviços e API

## Cliente HTTP — `api.ts`

Singleton `ApiClient` que encapsula todas as chamadas HTTP:

```typescript
const api = new ApiClient()

api.setToken(token: string | null)  // salva/remove do localStorage
api.getToken(): string | null

api.get<T>(endpoint): Promise<ApiResponse<T>>
api.post<T>(endpoint, body?): Promise<ApiResponse<T>>
api.put<T>(endpoint, body?): Promise<ApiResponse<T>>
api.patch<T>(endpoint, body?): Promise<ApiResponse<T>>
api.delete<T>(endpoint): Promise<ApiResponse<T>>
```

**Base URL**: `import.meta.env.VITE_API_URL || 'http://localhost:8080'`

**Headers automáticos**: `Content-Type: application/json` + `Authorization: Bearer <token>` (quando autenticado).

### Interface ApiResponse

```typescript
interface ApiResponse<T> {
  code: number
  message: string
  data: T
}
```

## Serviço de Autenticação — `auth.service.ts`

```typescript
authService.login(credentials: LoginRequest): Promise<LoginResponse>
authService.logout(): void
authService.isAuthenticated(): boolean
```

| Método | Endpoint | Descrição |
|---|---|---|
| `login` | `POST /api/v1/open/login` | Faz login e salva token |
| `logout` | — | Remove token do localStorage |
| `isAuthenticated` | — | Verifica se existe token |

## Serviço de Sessão — `session.service.ts`

```typescript
sessionService.get(): Promise<Session>
```

| Método | Endpoint | Descrição |
|---|---|---|
| `get` | `GET /api/v1/session` | Retorna dados do usuário logado |

### Interface Session

```typescript
interface Session {
  id: number
  name: string
  username: string
  email: string | null
  avatarUrl: string | null
  permissionId: number
  sessionAt: string | null
  views: View[]
}

interface View {
  id: number
  name: string
  route: string
  icon: string
}
```

## Serviço de Usuários — `users.service.ts`

```typescript
usersService.getAll(): Promise<User[]>
usersService.create(data: CreateUserRequest): Promise<User>
usersService.update(id, data: UpdateUserRequest): Promise<User>
usersService.delete(id): Promise<void>
usersService.restore(id): Promise<void>
usersService.getPermissions(): Promise<Permission[]>
```

| Método | Endpoint | Descrição |
|---|---|---|
| `getAll` | `GET /api/v1/users` | Lista todos os usuários |
| `create` | `POST /api/v1/users` | Cria novo usuário |
| `update` | `PUT /api/v1/users/{id}` | Atualiza usuário |
| `delete` | `DELETE /api/v1/users/{id}` | Desativa (soft delete) |
| `restore` | `PATCH /api/v1/users/{id}/restore` | Reativa usuário |
| `getPermissions` | `GET /api/v1/users/options` | Lista permissões |

### Interfaces

```typescript
interface User {
  id: number
  name: string
  username: string
  email: string
  avatarUrl: string | null
  permissionId: number
  permissionName: string | null
  createdAt: string
  sessionAt: string | null
  deletedAt: string | null
}

interface Permission {
  id: number
  name: string
}
```

## Serviço de Conversas — `conversation.service.ts`

```typescript
conversationService.getActive(): Promise<ConversationListItem[]>
conversationService.getHistory(): Promise<ConversationListItem[]>
conversationService.getById(id): Promise<ConversationDetail | null>
conversationService.finish(id): Promise<void>
conversationService.join(id): Promise<void>
conversationService.leave(id): Promise<void>
conversationService.invite(id, attendantId): Promise<void>
```

| Método | Endpoint | Descrição |
|---|---|---|
| `getActive` | `GET /api/v1/conversations` | Lista conversas ativas |
| `getHistory` | `GET /api/v1/conversations/history` | Lista conversas finalizadas |
| `getById` | `GET /api/v1/conversations/{id}` | Detalhes de uma conversa |
| `finish` | `POST /api/v1/conversations/{id}/finish` | Finaliza conversa |
| `join` | `POST /api/v1/conversations/{id}/join` | Entra na conversa |
| `leave` | `POST /api/v1/conversations/{id}/leave` | Sai da conversa |
| `invite` | `POST /api/v1/conversations/{id}/invite/{attendantId}` | Convida atendente |

### Interfaces

```typescript
interface ConversationListItem {
  id: number
  clientId: number
  clientName: string
  clientEmail: string | null
  lastMessage: string | null
  lastMessageAt: string | null
  messageCount: number
  createdAt: string
  finishedAt: string | null
  status: string
}

interface ConversationDetail {
  id: number
  clientId: number
  clientName: string
  clientEmail: string | null
  clientPhone: string | null
  createdAt: string
  finishedAt: string | null
  attendanceTime: number | null
  status: string
  messages: ConversationMessage[]
  attendants: ConversationAttendant[]
}

interface ConversationAttendant {
  userId: number
  name: string
  avatarUrl: string | null
}
```

## Serviço de Chat — `chat.service.ts`

Singleton `ChatService` que gerencia a conexão SignalR:

### REST

```typescript
chatService.start(data: ChatStartRequest): Promise<ChatStartResponse>
// POST /api/v1/open/chat/start
```

### Conexão WebSocket

```typescript
chatService.connectAsUser(accessToken: string): Promise<void>
chatService.connectAsClient(clientToken: string): Promise<void>
chatService.disconnect(): Promise<void>
chatService.isConnected: boolean  // getter
```

**URL**: `VITE_API_URL/hubs/chat` com parâmetros `?access_token=` ou `?client_token=`

### Métodos de Envio

```typescript
chatService.joinConversation(conversationId: number): Promise<void>
chatService.leaveConversation(conversationId: number): Promise<void>
chatService.sendMessage(conversationId: number, content: string): Promise<void>
chatService.typing(conversationId: number): Promise<void>
chatService.stopTyping(conversationId: number): Promise<void>
chatService.markAsRead(conversationId: number, lastMessageId: number): Promise<void>
```

### Event Listeners

```typescript
chatService.onMessage(handler: (msg: ChatMessage) => void)
chatService.onTyping(handler: (payload: TypingPayload) => void)
chatService.onStopTyping(handler: (payload: TypingPayload) => void)
chatService.onMessageRead(handler: (payload: MessageReadPayload) => void)
chatService.onConversationFinished(handler: (data) => void)
chatService.onConversationInvited(handler: (data) => void)
chatService.onConversationCreated(handler: (data) => void)
chatService.onAttendantLeft(handler: (data) => void)
chatService.onUserOnline(handler: (payload: UserStatusPayload) => void)
chatService.onUserOffline(handler: (payload: UserStatusPayload) => void)
chatService.off(event: string)
chatService.onReconnected(callback)
chatService.onReconnecting(callback)
chatService.onClose(callback)
```

### Interfaces do Chat

```typescript
interface ChatMessage {
  id: number
  conversationId: number
  userId: number | null
  clientId: number | null
  type: number          // MessageType enum
  content: string
  fileUrl: string | null
  createdAt: string
}

interface ChatStartRequest {
  name: string
  email?: string
  phone?: string
  cpf?: string
}

interface ChatStartResponse {
  clientId: number
  clientToken: string
  conversationId: number
  isNewConversation: boolean
  messages: ChatMessage[]
}
```
