# Lógica do Banco de Dados - RobotEasy

## Visão Geral

O banco de dados é **PostgreSQL** gerenciado pelo **Entity Framework Core**.  
Utilizamos **Code-First**: as tabelas são geradas a partir das classes C# (Models).

---

## Diagrama do Banco

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              SISTEMA DE USUÁRIOS                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────┐       ┌─────────────────┐       ┌──────────────┐           │
│  │ permissions │◄──────│ permission_views│──────►│    views     │           │
│  ├─────────────┤  1:N  ├─────────────────┤  N:1  ├──────────────┤           │
│  │ id (PK)     │       │ id (PK)         │       │ id (PK)      │           │
│  │ name        │       │ permission_id   │       │ name         │           │
│  │ created_at  │       │ view_id         │       │ route        │           │
│  │ updated_at  │       └─────────────────┘       │ created_at   │           │
│  │ deleted_at  │                                 │ updated_at   │           │
│  └──────┬──────┘                                 │ deleted_at   │           │
│         │                                        └──────────────┘           │
│         │ 1:N                                                               │
│         │                                                                   │
│  ┌──────▼──────┐                                                            │
│  │   users     │                                                            │
│  ├─────────────┤                                                            │
│  │ id (PK)     │                                                            │
│  │ name        │                                                            │
│  │ username    │  ← UNIQUE                                                  │
│  │ email       │                                                            │
│  │ password    │                                                            │
│  │ token       │                                                            │
│  │ ws_conn     │  ← WebSocket connection ID                                 │
│  │ permission_id (FK)                                                       │
│  │ created_at  │                                                            │
│  │ updated_at  │                                                            │
│  │ deleted_at  │  ← Soft delete                                             │
│  │ session_at  │  ← Último login                                            │
│  └─────────────┘                                                            │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                              SISTEMA DE CHAT                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────┐         ┌─────────────────────┐         ┌──────────────┐  │
│  │   clients    │◄────────│   conversations     │────────►│   messages   │  │
│  ├──────────────┤    1:N  ├─────────────────────┤    1:N  ├──────────────┤  │
│  │ id (PK) BI   │         │ id (PK) BIGINT      │         │ id (PK) BI   │  │
│  │ name         │         │ client_id (FK)      │         │ conv_id (FK) │  │
│  │ ws_conn      │         │ created_at          │         │ client_id    │  │
│  │ cpf          │         │ finished_at         │         │ user_id      │  │
│  │ phone        │         │ attendance_time     │         │ type (enum)  │  │
│  │ email        │         └──────────┬──────────┘         │ content      │  │
│  │ created_at   │                    │                    │ file_url     │  │
│  │ updated_at   │                    │ 1:N                │ file_name    │  │
│  └──────────────┘                    │                    │ file_size    │  │
│                      ┌───────────────▼───────────┐        │ created_at   │  │
│                      │   user_conversations      │        └──────────────┘  │
│                      ├───────────────────────────┤                          │
│                      │ id (PK) BIGINT            │                          │
│                      │ user_id (FK)              │◄─────── users            │
│                      │ conversation_id (FK)      │                          │
│                      │ started_at                │                          │
│                      │ finished_at               │                          │
│                      │ events (JSONB)            │  ← Logs de eventos       │
│                      └───────────────────────────┘                          │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Tabelas e Campos

### 1. users (Atendentes/Administradores)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | int PK | Identificador único |
| name | varchar(100) | Nome completo |
| username | varchar(100) UNIQUE | Login do usuário |
| email | varchar(255) | Email opcional |
| password_hash | text | Senha hashada com BCrypt |
| token | varchar(500) | JWT token atual |
| ws_conn | varchar(255) | ID da conexão WebSocket ativa |
| permission_id | int FK | Referência à permissão |
| created_at | timestamp | Data de criação |
| updated_at | timestamp | Última atualização |
| deleted_at | timestamp | **Soft delete** (null = ativo) |
| session_at | timestamp | Data do último login |

### 2. permissions (Perfis de Acesso)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | int PK | Identificador único |
| name | varchar(100) | Nome do perfil (Admin, Atendente, etc) |
| created_at | timestamp | Data de criação |
| updated_at | timestamp | Última atualização |
| deleted_at | timestamp | Soft delete |

### 3. views (Telas/Rotas do Sistema)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | int PK | Identificador único |
| name | varchar(100) | Nome da tela |
| route | varchar(255) | Rota da aplicação (/dashboard, /users, etc) |
| created_at | timestamp | Data de criação |
| updated_at | timestamp | Última atualização |
| deleted_at | timestamp | Soft delete |

### 4. permission_views (Relação N:N)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | int PK | Identificador único |
| permission_id | int FK | Qual permissão |
| view_id | int FK | Qual tela pode acessar |

### 5. clients (Clientes do Chat)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | bigint PK | ID alto volume |
| name | varchar(150) | Nome do cliente |
| ws_conn | varchar(255) | Conexão WebSocket atual |
| cpf | varchar(14) | CPF (indexado para busca) |
| phone | varchar(20) | Telefone (indexado) |
| email | varchar(255) | Email (indexado) |
| created_at | timestamp | Primeira interação |
| updated_at | timestamp | Última atualização |

### 6. conversations (Sessões de Chat)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | bigint PK | ID alto volume |
| client_id | bigint FK | Cliente da conversa |
| created_at | timestamp | Início da conversa |
| finished_at | timestamp | Fim (null = ativa) |
| attendance_time | int | Tempo total em segundos |

### 7. user_conversations (Atendentes por Conversa)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | bigint PK | ID alto volume |
| user_id | int FK | Atendente |
| conversation_id | bigint FK | Conversa |
| started_at | timestamp | Início do atendimento |
| finished_at | timestamp | Fim (transferência/encerramento) |
| events | jsonb | Logs: transferências, pausas, etc |

**Por que essa tabela existe?**  
Uma conversa pode ter **múltiplos atendentes** (transferência).  
Isso permite rastrear quem atendeu, quando, e eventos ocorridos.

### 8. messages (Mensagens)

| Campo | Tipo | Descrição |
|-------|------|-----------|
| id | bigint PK | ID alto volume |
| conversation_id | bigint FK | Qual conversa |
| client_id | bigint FK nullable | Se enviada pelo cliente |
| user_id | int FK nullable | Se enviada pelo atendente |
| type | enum | Text, Image, File, Audio, Video, System |
| content | text | Conteúdo da mensagem |
| file_url | varchar(500) | URL do arquivo (se houver) |
| file_name | varchar(255) | Nome original do arquivo |
| file_size | bigint | Tamanho em bytes |
| created_at | timestamp | Data/hora do envio |

---

## Índices para Performance

### Tabela clients
```sql
CREATE INDEX ix_clients_cpf ON clients(cpf);
CREATE INDEX ix_clients_phone ON clients(phone);
CREATE INDEX ix_clients_email ON clients(email);
CREATE INDEX ix_clients_created_at ON clients(created_at);
```

### Tabela conversations
```sql
CREATE INDEX ix_conversations_client_id ON conversations(client_id);
CREATE INDEX ix_conversations_created_at ON conversations(created_at);
CREATE INDEX ix_conversations_finished_at ON conversations(finished_at);
-- Índice composto: buscar conversa ativa de um cliente
CREATE INDEX ix_conversations_client_finished ON conversations(client_id, finished_at);
```

### Tabela user_conversations
```sql
CREATE INDEX ix_user_conversations_user_id ON user_conversations(user_id);
CREATE INDEX ix_user_conversations_conversation_id ON user_conversations(conversation_id);
-- Índice composto: atendimentos ativos de um usuário
CREATE INDEX ix_user_conversations_user_finished ON user_conversations(user_id, finished_at);
```

### Tabela messages (CRÍTICO)
```sql
CREATE INDEX ix_messages_conversation_id ON messages(conversation_id);
CREATE INDEX ix_messages_created_at ON messages(created_at);
-- Índice composto: carregar mensagens de uma conversa ordenadas (mais usado)
CREATE INDEX ix_messages_conv_created ON messages(conversation_id, created_at);
```

---

## Soft Delete

As tabelas `users`, `permissions` e `views` usam **soft delete**:
- Registros não são removidos, apenas marcam `deleted_at`
- Consultas filtram automaticamente registros deletados
- Permite recuperação e auditoria

```csharp
// Configurado no EF Core:
entity.HasQueryFilter(u => u.DeletedAt == null);
```

---

## Por que BIGINT no Chat?

Tabelas de chat (`clients`, `conversations`, `messages`) usam `bigint` porque:
- Um sistema de chat pode ter **milhões de mensagens**
- `int` suporta até ~2 bilhões
- `bigint` suporta até ~9 quintilhões
- Melhor prevenir do que migrar depois

---

## Por que NÃO usar Base64 para arquivos?

Decisão: Guardar `file_url` ao invés de `file_content base64`:

| Aspecto | Base64 no DB | URL externa |
|---------|--------------|-------------|
| Tamanho | +33% (codificação) | Original |
| Performance | Lento (carregar sempre) | Sob demanda |
| Backup | DB gigante | DB leve |
| CDN | Impossível | Possível |

**Solução**: Arquivos vão para S3/MinIO/disco, DB guarda apenas a URL.

---

## Convenções de Nomenclatura

| Tipo | Convenção | Exemplo |
|------|-----------|---------|
| Tabelas | snake_case plural | `user_conversations` |
| Colunas | snake_case | `permission_id` |
| Foreign Keys | `{entidade}_id` | `client_id`, `user_id` |
| Índices | `ix_{tabela}_{colunas}` | `ix_messages_conv_created` |
| Classes C# | PascalCase | `UserConversation` |

---

## Migrations

O Entity Framework Core gera migrations automaticamente:

```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration

# Aplicar no banco
dotnet ef database update

# Reverter última migration
dotnet ef migrations remove
```

No `Program.cs`, as migrations são aplicadas automaticamente no startup:
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```
