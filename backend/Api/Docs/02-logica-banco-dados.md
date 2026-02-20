# Lógica do Banco de Dados

## Visão Geral

O banco de dados é **PostgreSQL 16** gerenciado pelo **Entity Framework Core 10**. Todas as operações de acesso a dados passam exclusivamente pelos Repositories.

## Diagrama de Relacionamentos

```
Permission ──1:N──► User
Permission ──1:N──► PermissionView ◄──N:1── View

User ──1:N──► UserConversation ◄──N:1── Conversation
User ──1:N──► Message

Client ──1:N──► Conversation
Client ──1:N──► Message

Conversation ──1:N──► Message
Conversation ──1:N──► UserConversation
```

## Modelos

### User (Usuário/Atendente)

| Campo | Tipo | Descrição |
|---|---|---|
| Id | `int` | PK, auto-increment |
| Name | `string` (100) | Nome completo |
| Username | `string` (100) | Login único (unique index) |
| Email | `string?` (255) | E-mail (index) |
| PasswordHash | `string` | Hash Argon2id da senha |
| Token | `string?` | Token de sessão ativo (null = sem sessão) |
| WsConn | `string?` | Connection ID do SignalR |
| AvatarUrl | `string?` | URL do avatar |
| PermissionId | `int` | FK → Permission (Restrict) |
| CreatedAt | `DateTime` | Data de criação |
| UpdatedAt | `DateTime?` | Última atualização |
| DeletedAt | `DateTime?` | Soft delete (null = ativo) |
| SessionAt | `DateTime?` | Último login |

**Soft delete**: Query filter global `DeletedAt == null`. O `UserRepository.GetAllAsync()` usa `IgnoreQueryFilters()` para listar todos (incluindo inativos) na tela de administração.

### Client (Cliente do Chat)

| Campo | Tipo | Descrição |
|---|---|---|
| Id | `long` | PK, auto-increment |
| Name | `string` (150) | Nome do cliente |
| WsConn | `string?` | Connection ID do SignalR |
| Cpf | `string?` (14) | CPF (index) |
| Phone | `string?` (20) | Telefone (index) |
| Email | `string?` (255) | E-mail (index) |
| CreatedAt | `DateTime` | Data de criação (index) |
| UpdatedAt | `DateTime?` | Última atualização |

### Permission

| Campo | Tipo | Descrição |
|---|---|---|
| Id | `int` | PK, auto-increment |
| Name | `string` (100) | Nome (admin, operator) |
| CreatedAt | `DateTime` | Data de criação |
| UpdatedAt | `DateTime?` | Última atualização |
| DeletedAt | `DateTime?` | Soft delete |

### View (Tela do Sistema)

| Campo | Tipo | Descrição |
|---|---|---|
| Id | `int` | PK, auto-increment |
| Name | `string` (100) | Identificador da tela |
| Route | `string` (255) | Rota no frontend |
| Icon | `string` (100) | Classe do ícone FontAwesome |
| CreatedAt | `DateTime` | Data de criação |
| UpdatedAt | `DateTime?` | Última atualização |
| DeletedAt | `DateTime?` | Soft delete |

### PermissionView (Join: Permission ↔ View)

| Campo | Tipo | Descrição |
|---|---|---|
| Id | `int` | PK, auto-increment |
| PermissionId | `int` | FK → Permission (Cascade) |
| ViewId | `int` | FK → View (Cascade) |

### Conversation

| Campo | Tipo | Descrição |
|---|---|---|
| Id | `long` | PK, auto-increment |
| ClientId | `long` | FK → Client (Restrict) |
| CreatedAt | `DateTime` | Data de criação (index) |
| FinishedAt | `DateTime?` | Data de finalização (index) |
| AttendanceTime | `int?` | Tempo de atendimento em segundos |

**Índices**: `ClientId`, `CreatedAt`, `FinishedAt`, composto `{ClientId, FinishedAt}`.

### Message

| Campo | Tipo | Descrição |
|---|---|---|
| Id | `long` | PK, auto-increment |
| ConversationId | `long` | FK → Conversation (Cascade) |
| ClientId | `long?` | FK → Client (SetNull) — null = enviada por atendente |
| UserId | `int?` | FK → User (SetNull) — null = enviada por cliente |
| Type | `MessageType` | Tipo da mensagem (enum) |
| Content | `string` | Conteúdo (required) |
| FileUrl | `string?` (500) | URL do arquivo |
| FileName | `string?` (255) | Nome do arquivo |
| FileSize | `long?` | Tamanho do arquivo em bytes |
| CreatedAt | `DateTime` | Data de criação |

**Índices**: `ConversationId`, `CreatedAt`, composto `{ConversationId, CreatedAt}`.

**MessageType (enum):**

| Valor | Nome |
|---|---|
| 1 | Text |
| 2 | Image |
| 3 | File |
| 4 | Audio |
| 5 | Video |
| 6 | System |

### UserConversation (Join: User ↔ Conversation)

| Campo | Tipo | Descrição |
|---|---|---|
| Id | `long` | PK, auto-increment |
| UserId | `int` | FK → User (Restrict) |
| ConversationId | `long` | FK → Conversation (Cascade) |
| StartedAt | `DateTime` | Quando o atendente entrou |
| FinishedAt | `DateTime?` | Quando o atendente saiu |
| Events | `string?` | Eventos em formato jsonb |

**Índices**: `UserId`, `ConversationId`, composto `{UserId, FinishedAt}`.

## Hash de Senhas — Argon2id

As senhas são hasheadas usando **Argon2id** (não BCrypt). Configuração via variáveis de ambiente:

| Parâmetro | Padrão | Descrição |
|---|---|---|
| MemorySize | 65536 | Memória em KB (64MB) |
| Iterations | 4 | Número de iterações |
| Parallelism | 2 | Threads paralelas |
| HashLength | 32 | Tamanho do hash em bytes |
| SaltLength | 16 | Tamanho do salt em bytes |

## Seed Inicial (DatabaseSeeder)

Executado na inicialização após `Database.MigrateAsync()`:

### 1. Permissions

| Nome |
|---|
| admin |
| operator |

### 2. Users

| Username | Nome | Email | Senha | Permissão |
|---|---|---|---|---|
| admin.master | Administrador Master | admin@roboteasy.com | MyAdm2026TestCode | admin |
| francisco.luiz | Francisco Luiz | francisco.luiz.jr@outlook.com | CodandoEmC# | operator |

### 3. Views

| Nome | Rota | Ícone |
|---|---|---|
| home | /session/home | fa-house |
| customer_service | /session/customer-service | fa-headset |
| clients | /session/clients | fa-user |
| history | /session/history | fa-clock-rotate-left |
| users | /session/users | fa-users |
| monitoring | /session/monitoring | fa-chart-line |

### 4. PermissionViews

| Permissão | Views |
|---|---|
| admin | Todas as 6 views |
| operator | home, customer_service |

## Migrações

As migrações ficam em `Migrations/` e são aplicadas automaticamente via `DatabaseSeeder.SeedAsync()` no startup.

Para criar uma nova migração:

```bash
cd backend/Api
dotnet ef migrations add NomeDaMigracao
```
