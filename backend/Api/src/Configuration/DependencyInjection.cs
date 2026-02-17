using Microsoft.EntityFrameworkCore;
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

        // Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();

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

        return services;
    }
}
