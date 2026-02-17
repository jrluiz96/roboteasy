# Estrutura do Backend - API RobotEasy

## Visão Geral

Este backend foi construído em **ASP.NET Core 10** com **Minimal APIs** e arquitetura em camadas.

---

## Estrutura de Pastas

```
backend/Api/
├── Api.csproj              # Definição do projeto e dependências
├── Program.cs              # Entry point da aplicação
├── Dockerfile              # Build da imagem Docker
├── appsettings.json        # Configurações da aplicação
├── Docs/                   # Documentação do sistema
├── Migrations/             # Migrations do Entity Framework Core
│
└── src/                    # Código fonte da aplicação
    │
    ├── Configuration/      # Configurações e injeção de dependências
    │   ├── AppSettings.cs          # Classes de configuração (JWT, etc)
    │   └── DependencyInjection.cs  # Registro de serviços no container DI
    │
    ├── Contracts/          # Objetos de transferência de dados (Request/Response)
    │   ├── Requests/       # Dados que a API recebe
    │   │   ├── CreateUserRequest.cs
    │   │   └── UpdateUserRequest.cs
    │   └── Responses/      # Dados que a API retorna
    │       └── UserResponse.cs
    │
    ├── Controllers/        # Controllers REST (estilo MVC)
    │   └── UserController.cs
    │
    ├── Data/               # Contexto do banco de dados
    │   └── AppDbContext.cs     # DbContext do Entity Framework Core
    │
    ├── Middleware/         # Middlewares HTTP (erro, auth, logging)
    │   └── ErrorHandlingMiddleware.cs
    │
    ├── Models/             # Entidades do banco de dados
    │   ├── User.cs
    │   ├── Permission.cs
    │   ├── View.cs
    │   ├── PermissionView.cs
    │   ├── Client.cs
    │   ├── Conversation.cs
    │   ├── UserConversation.cs
    │   └── Message.cs
    │
    ├── Repositories/       # Acesso a dados (Data Access Layer)
    │   ├── IUserRepository.cs      # Interface (contrato)
    │   └── UserRepository.cs       # Implementação
    │
    ├── Routes/             # Minimal APIs (alternativa ao Controller)
    │   └── UserRoutes.cs
    │
    └── Services/           # Lógica de negócio (Business Logic Layer)
        ├── IUserService.cs
        └── UserService.cs
```

---

## Fluxo de uma Requisição

```
HTTP Request
     │
     ▼
┌─────────────────┐
│   Middleware    │  ← ErrorHandlingMiddleware (captura exceções)
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Controller    │  ← Recebe request, valida, chama Service
│   ou Routes     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    Service      │  ← Lógica de negócio, regras, transformações
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Repository    │  ← Acesso ao banco via Entity Framework
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  AppDbContext   │  ← Conexão com PostgreSQL
└─────────────────┘
```

---

## Injeção de Dependências

O C# usa DI nativo. As dependências são registradas em `DependencyInjection.cs`:

```csharp
// Registra como Scoped (1 instância por request)
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IUserService, UserService>();
```

E injetadas automaticamente no construtor:

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    
    public UserService(IUserRepository repository) // DI injeta automaticamente
    {
        _repository = repository;
    }
}
```

---

## Controllers vs Minimal APIs (Routes)

### Controller (Estilo MVC tradicional)
```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() { ... }
}
```

### Minimal APIs (mais simples)
```csharp
app.MapGet("/api/users", async (IUserService service) =>
    Results.Ok(await service.GetAllAsync()));
```

Ambos funcionam, escolha o estilo que preferir.

---

## Executando o Projeto

### Desenvolvimento Local
```bash
cd backend/Api
dotnet run
```

### Com Docker
```bash
docker compose up --build -d
```

### Criando Migrations
```bash
dotnet ef migrations add NomeDaMigration
```

---

## Tecnologias Utilizadas

- **ASP.NET Core 10** - Framework web
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **BCrypt** - Hash de senhas
- **Swagger/OpenAPI** - Documentação da API
- **Docker** - Containerização
