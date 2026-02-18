# Variáveis de Ambiente

Todas as configurações da API são obrigatórias e devem ser definidas via Docker. A aplicação não aceita valores padrão e irá falhar se qualquer configuração estiver ausente.

## Database

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `Database__Host` | Host do PostgreSQL | `db` |
| `Database__Port` | Porta do PostgreSQL | `5432` |
| `Database__Name` | Nome do banco de dados | `roboteasy` |
| `Database__User` | Usuário do banco | `roboteasy` |
| `Database__Password` | Senha do banco | `roboteasy123` |

## AppSettings

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `AppSettings__JwtSecret` | Chave secreta para JWT (mínimo 32 caracteres) | `eBs3Cr78JSHE8XhbjeyMytn2zODSsd9k` |
| `AppSettings__JwtExpirationHours` | Tempo de expiração do token em horas | `8` |

## GitHub OAuth

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `GitHub__ClientId` | Client ID da OAuth App no GitHub | `Iv1.abc123...` |
| `GitHub__ClientSecret` | Client Secret da OAuth App no GitHub | `abc123secret...` |
| `GitHub__CallbackUrl` | URL de callback após autenticação | `http://localhost:8080/api/v1/open/github/callback` |

## Argon2 (Hash de Senha)

Configurações para o algoritmo Argon2id usado no hash de senhas.

| Variável | Descrição | Valor Recomendado |
|----------|-----------|-------------------|
| `Argon2__MemorySize` | Memória em KB | `65536` (64 MB) |
| `Argon2__Iterations` | Número de iterações | `4` |
| `Argon2__Parallelism` | Grau de paralelismo | `2` |
| `Argon2__HashLength` | Tamanho do hash em bytes | `32` |
| `Argon2__SaltLength` | Tamanho do salt em bytes | `16` |

### Ajustando Segurança vs Performance

- **Mais seguro**: Aumente `MemorySize` e `Iterations`
- **Mais rápido**: Diminua os valores (não recomendado em produção)
- **Multi-core**: Ajuste `Parallelism` conforme núcleos disponíveis

### Valores OWASP Recomendados

Para produção, considere:
```
Argon2__MemorySize=65536   # 64 MB
Argon2__Iterations=4
Argon2__Parallelism=2
```
