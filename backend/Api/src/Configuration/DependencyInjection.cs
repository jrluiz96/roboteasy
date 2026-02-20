using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Api.Data;
using Api.Repositories;
using Api.Services;

namespace Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database - todas as configurações são obrigatórias
        var dbSection = configuration.GetSection("Database");
        var host = dbSection["Host"] ?? throw new InvalidOperationException("Database:Host não configurado");
        var port = dbSection["Port"] ?? throw new InvalidOperationException("Database:Port não configurado");
        var name = dbSection["Name"] ?? throw new InvalidOperationException("Database:Name não configurado");
        var user = dbSection["User"] ?? throw new InvalidOperationException("Database:User não configurado");
        var password = dbSection["Password"] ?? throw new InvalidOperationException("Database:Password não configurado");
        
        var connectionString = $"Host={host};Port={port};Database={name};Username={user};Password={password}";
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionViewRepository, PermissionViewRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddHttpClient<IGitHubService, GitHubService>();

        // Argon2 Settings - validação obrigatória
        var argon2Section = configuration.GetSection("Argon2");
        _ = argon2Section["MemorySize"] ?? throw new InvalidOperationException("Argon2:MemorySize não configurado");
        _ = argon2Section["Iterations"] ?? throw new InvalidOperationException("Argon2:Iterations não configurado");
        _ = argon2Section["Parallelism"] ?? throw new InvalidOperationException("Argon2:Parallelism não configurado");
        
        services.Configure<Argon2Settings>(argon2Section);

        // Settings - validação obrigatória
        var appSettings = configuration.GetSection("AppSettings");
        var jwtSecret = appSettings["JwtSecret"] ?? throw new InvalidOperationException("AppSettings:JwtSecret não configurado");
        if (jwtSecret.Length < 32)
            throw new InvalidOperationException("AppSettings:JwtSecret deve ter no mínimo 32 caracteres");
        _ = appSettings["JwtExpirationHours"] ?? throw new InvalidOperationException("AppSettings:JwtExpirationHours não configurado");
        
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        // JWT Authentication
        var key = Encoding.UTF8.GetBytes(jwtSecret);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "roboteasy",
                ValidAudience = "roboteasy",
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            // SignalR: WebSocket não suporta headers, token vem via query string
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
        
        services.AddAuthorization();

        // GitHub OAuth Settings - validação obrigatória
        var githubSection = configuration.GetSection("GitHub");
        _ = githubSection["ClientId"] ?? throw new InvalidOperationException("GitHub:ClientId não configurado");
        _ = githubSection["ClientSecret"] ?? throw new InvalidOperationException("GitHub:ClientSecret não configurado");
        _ = githubSection["CallbackUrl"] ?? throw new InvalidOperationException("GitHub:CallbackUrl não configurado");
        
        services.Configure<GitHubSettings>(githubSection);

        return services;
    }
}
