# Variáveis de Ambiente

Todas as configurações sensíveis são fornecidas via variáveis de ambiente (definidas no `docker-compose.yml`). Os arquivos `appsettings.json` contêm apenas configurações de logging.

## Database

| Variável | Obrigatória | Exemplo | Descrição |
|---|---|---|---|
| `Database__Host` | Sim | `db` | Host do PostgreSQL |
| `Database__Port` | Sim | `5432` | Porta do PostgreSQL |
| `Database__Name` | Sim | `roboteasy` | Nome do banco |
| `Database__User` | Sim | `roboteasy` | Usuário do banco |
| `Database__Password` | Sim | `roboteasy123` | Senha do banco |

## AppSettings

| Variável | Obrigatória | Exemplo | Descrição |
|---|---|---|---|
| `AppSettings__JwtSecret` | Sim | `eBs3Cr78JSHE8X...` | Chave secreta JWT (mín. 32 caracteres) |
| `AppSettings__JwtExpirationHours` | Sim | `8` | Tempo de expiração do token JWT em horas |
| `AppSettings__CorsOrigins` | Não | `*` | Origens permitidas CORS (padrão: `*`) |

## Argon2 (Hash de Senhas)

| Variável | Obrigatória | Padrão | Descrição |
|---|---|---|---|
| `Argon2__MemorySize` | Sim | `65536` | Memória em KB (64MB) |
| `Argon2__Iterations` | Sim | `4` | Número de iterações |
| `Argon2__Parallelism` | Sim | `2` | Threads paralelas |
| `Argon2__HashLength` | Não | `32` | Tamanho do hash em bytes |
| `Argon2__SaltLength` | Não | `16` | Tamanho do salt em bytes |

## ASP.NET Core

| Variável | Exemplo | Descrição |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Ambiente da aplicação |
| `ASPNETCORE_URLS` | `http://+:8080` | URL de binding (definida no Dockerfile) |

## PostgreSQL (Container)

| Variável | Exemplo | Descrição |
|---|---|---|
| `POSTGRES_USER` | `roboteasy` | Usuário do PostgreSQL |
| `POSTGRES_PASSWORD` | `roboteasy123` | Senha do PostgreSQL |
| `POSTGRES_DB` | `roboteasy` | Nome do banco |

## Exemplo Completo (docker-compose.yml)

```yaml
services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Database__Host=db
      - Database__Port=5432
      - Database__Name=roboteasy
      - Database__User=roboteasy
      - Database__Password=roboteasy123
      - AppSettings__JwtSecret=eBs3Cr78JSHE8XhbjeyMytn2zODSsd9k
      - AppSettings__JwtExpirationHours=8
      - AppSettings__CorsOrigins=*
      - Argon2__MemorySize=65536
      - Argon2__Iterations=4
      - Argon2__Parallelism=2
      - Argon2__HashLength=32
      - Argon2__SaltLength=16

  db:
    environment:
      - POSTGRES_USER=roboteasy
      - POSTGRES_PASSWORD=roboteasy123
      - POSTGRES_DB=roboteasy
```

> **Nota:** O separador `__` (double underscore) é o padrão do .NET para variáveis de ambiente hierárquicas (equivale a `:` nos arquivos JSON).
