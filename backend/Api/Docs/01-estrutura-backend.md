# Estrutura do Backend

## Visão Geral

API REST construída com .NET 10, Entity Framework Core e PostgreSQL 16. Inclui WebSocket via SignalR para chat em tempo real.

## Tecnologias

| Tecnologia | Versão | Uso |
|---|---|---|
| .NET | 10.0 | Framework |
| Entity Framework Core | 10.0 | ORM |
| PostgreSQL | 16 | Banco de dados |
| SignalR | — | WebSocket (chat em tempo real) |
| Argon2id | — | Hash de senhas |
| JWT Bearer | — | Autenticação |
| Swagger | — | Documentação de API |
| Docker | — | Containerização |

## Árvore de Diretórios

```
backend/Api/
├── Program.cs                         # Entry point + pipeline
├── Api.csproj                         # Projeto .NET
├── Dockerfile                         # Build multi-stage
├── appsettings.json                   # Config base
├── appsettings.Development.json       # Config dev
├── Docs/                              # Documentação
│   ├── 01-estrutura-backend.md
│   ├── 02-logica-banco-dados.md
│   └── 03-variaveis-ambiente.md
├── Migrations/                        # Migrações EF Core
├── Properties/
│   └── launchSettings.json
└── src/
    ├── Configuration/
    │   ├── AppSettings.cs             # Records de configuração (AppSettings, Argon2Settings)
    │   ├── DatabaseSeeder.cs          # Seed inicial (permissions, users, views)
    │   └── DependencyInjection.cs     # Registro de todos os serviços
    ├── Contracts/
    │   ├── Requests/
    │   │   ├── ChatStartRequest.cs    # { Name, Email?, Phone?, Cpf? }
    │   │   ├── CreateUserRequest.cs   # { Name, Username, Password, PermissionId }
    │   │   ├── LoginRequest.cs        # { Username, Password }
    │   │   └── UpdateUserRequest.cs   # { Name?, Username?, Password?, PermissionId?, Email? }
    │   └── Responses/
    │       ├── ApiResponse.cs         # Resposta padrão { Code, Status, Message, Data }
    │       ├── ChatStartResponse.cs
    │       ├── ConversationAttendantResponse.cs
    │       ├── ConversationDetailResponse.cs
    │       ├── ConversationListItemResponse.cs
    │       ├── LoginResponse.cs
    │       ├── MessageResponse.cs
    │       ├── SessionResponse.cs
    │       ├── UserOptionsResponse.cs
    │       └── UserResponse.cs
    ├── Controllers/
    │   ├── OpenController.cs          # Rotas públicas (login, swagger, chat/start)
    │   ├── SessionController.cs       # Dados da sessão do usuário
    │   ├── UsersController.cs         # CRUD de usuários
    │   └── ConversationsController.cs # Conversas (listar, histórico, join, invite, leave, finish)
    ├── Data/
    │   ├── AppDbContext.cs            # DbContext com 8 DbSets
    │   └── DesignTimeDbContextFactory.cs
    ├── Hubs/
    │   ├── ChatEvents.cs              # Constantes de eventos SignalR
    │   └── ChatHub.cs                 # Hub de chat em tempo real
    ├── Middleware/
    │   ├── CorsMiddleware.cs          # CORS customizado
    │   ├── ErrorHandlingMiddleware.cs # Tratamento global de erros (retorna ApiResponse)
    │   └── SessionValidationMiddleware.cs # Validação de sessão via token
    ├── Models/
    │   ├── Client.cs                  # Cliente do chat
    │   ├── Conversation.cs            # Conversa
    │   ├── Message.cs                 # Mensagem (+ enum MessageType)
    │   ├── Permission.cs              # Permissão (admin, operator)
    │   ├── PermissionView.cs          # Join table Permission ↔ View
    │   ├── User.cs                    # Usuário/Atendente
    │   ├── UserConversation.cs        # Join table User ↔ Conversation
    │   └── View.cs                    # Tela do sistema
    ├── Repositories/
    │   ├── IClientRepository.cs
    │   ├── ClientRepository.cs
    │   ├── IConversationRepository.cs
    │   ├── ConversationRepository.cs
    │   ├── IMessageRepository.cs
    │   ├── MessageRepository.cs
    │   ├── IPermissionRepository.cs
    │   ├── PermissionRepository.cs
    │   ├── IPermissionViewRepository.cs
    │   ├── PermissionViewRepository.cs
    │   ├── IUserRepository.cs
    │   └── UserRepository.cs
    └── Services/
        ├── IChatService.cs
        ├── ChatService.cs             # Inicia conversa de chat
        ├── IConversationService.cs
        ├── ConversationService.cs     # Lógica de conversas
        ├── ISessionService.cs
        ├── SessionService.cs          # Login, logout, sessão
        ├── IUserService.cs
        ├── UserService.cs             # CRUD de usuários
        ├── JwtService.cs              # IJwtService + JwtService (geração/validação JWT)
        └── PasswordHasher.cs          # IPasswordHasher + Argon2PasswordHasher
```

## Padrão Arquitetural

```
Controller → Service → Repository → AppDbContext
```

- **Controller**: Recebe requisição HTTP, chama Service, retorna `ApiResponse<T>`
- **Service**: Lógica de negócio, orquestra Repositories
- **Repository**: Acesso a dados via EF Core (único local que usa `AppDbContext`)
- **AppDbContext**: Apenas em `Repositories/`, `Data/`, `Configuration/DependencyInjection.cs` e `DatabaseSeeder.cs`

## Endpoints da API

### OpenController — `api/v1/open` (sem autenticação)

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/v1/open/login` | Login com username/password → retorna JWT |
| GET | `/api/v1/open/swagger` | Redireciona para Swagger UI |
| POST | `/api/v1/open/chat/start` | Inicia chat como cliente → retorna clientToken |

### SessionController — `api/v1/session` (autenticado)

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/v1/session` | Dados da sessão (user, views, permissões) |

### UsersController — `api/v1/users` (autenticado)

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/v1/users/options` | Lista permissões disponíveis |
| GET | `/api/v1/users` | Lista todos os usuários (incluindo inativos) |
| GET | `/api/v1/users/{id}` | Busca usuário por ID |
| POST | `/api/v1/users` | Cria novo usuário |
| PUT | `/api/v1/users/{id}` | Atualiza usuário |
| DELETE | `/api/v1/users/{id}` | Desativa usuário (soft delete) |
| PATCH | `/api/v1/users/{id}/restore` | Reativa usuário |

### ConversationsController — `api/v1/conversations` (autenticado)

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/v1/conversations` | Lista conversas ativas do usuário |
| GET | `/api/v1/conversations/history` | Lista conversas finalizadas |
| GET | `/api/v1/conversations/{id}` | Detalhes de uma conversa |
| POST | `/api/v1/conversations/{id}/join` | Entrar na conversa (puxar) |
| POST | `/api/v1/conversations/{id}/invite/{attendantId}` | Convidar atendente |
| POST | `/api/v1/conversations/{id}/leave` | Sair da conversa |
| POST | `/api/v1/conversations/{id}/finish` | Finalizar conversa |

## SignalR Hub — `/hubs/chat`

### Conexão

- **Usuário (atendente)**: Conecta com `?access_token=<JWT>`. Salva `WsConn`, entra no grupo `attendants`, broadcast `user:online`.
- **Cliente**: Conecta com `?client_token=<token>`. Salva `WsConn`.
- **Desconexão**: Limpa `WsConn`, broadcast `user:offline` (para usuários).

### Métodos Invocáveis

| Método | Parâmetros | Descrição |
|---|---|---|
| `JoinConversation` | `conversationId` | Entra no grupo SignalR da conversa |
| `LeaveConversation` | `conversationId` | Sai do grupo SignalR da conversa |
| `SendMessage` | `conversationId, content` | Envia mensagem (salva no DB, broadcast para o grupo) |
| `Typing` | `conversationId` | Indica que está digitando |
| `StopTyping` | `conversationId` | Para de indicar digitação |
| `MarkAsRead` | `conversationId, lastMessageId` | Marca mensagens como lidas |

### Eventos Emitidos (ChatEvents)

| Evento | Emitido quando |
|---|---|
| `user:online` | Atendente conecta |
| `user:offline` | Atendente desconecta |
| `message:receive` | Nova mensagem enviada |
| `typing:start` | Alguém está digitando |
| `typing:stop` | Parou de digitar |
| `message:read` | Mensagens foram lidas |
| `conversation:finished` | Conversa finalizada |
| `conversation:invited` | Atendente convidado |
| `conversation:created` | Nova conversa criada |
| `attendant:left` | Atendente saiu da conversa |

## Middleware — Pipeline

Ordem de execução em `Program.cs`:

1. `UseCorsMiddleware()` — CORS customizado
2. `UseCors("SignalRPolicy")` — CORS nativo para SignalR
3. `UseErrorHandling()` — Try/catch global → `ApiResponse` com status 500
4. Swagger UI (apenas em Development)
5. `UseAuthentication()` — JWT Bearer
6. `UseSessionValidation()` — Valida token de sessão no banco
7. `UseAuthorization()`
8. `MapControllers()`
9. `MapHub<ChatHub>("/hubs/chat")`
10. `DatabaseSeeder.SeedAsync()` — Migra + seed na inicialização

## Resposta Padrão — ApiResponse

Todas as respostas seguem o formato:

```json
{
  "code": 200,
  "status": "success",
  "message": "Operação realizada com sucesso",
  "data": { ... }
}
```

Factories disponíveis: `Success`, `Created`, `Error`, `NotFound`, `Unauthorized`, `BadRequest`, `Ok`, `Fail`.

## Injeção de Dependências

Registrado em `DependencyInjection.cs`:

**Scoped:**
- Repositories: `IUserRepository`, `IPermissionRepository`, `IPermissionViewRepository`, `IClientRepository`, `IConversationRepository`, `IMessageRepository`
- Services: `IUserService`, `ISessionService`, `IChatService`, `IConversationService`

**Singleton:**
- `IPasswordHasher` → `Argon2PasswordHasher`
- `IJwtService` → `JwtService`

**Autenticação JWT:**
- Issuer/Audience: `roboteasy`
- `ClockSkew = TimeSpan.Zero`
- SignalR extrai token do query string para paths `/hubs/*`
