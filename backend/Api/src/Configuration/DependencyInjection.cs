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

        // Settings - validação obrigatória
        var appSettings = configuration.GetSection("AppSettings");
        var jwtSecret = appSettings["JwtSecret"] ?? throw new InvalidOperationException("AppSettings:JwtSecret não configurado");
        if (jwtSecret.Length < 32)
            throw new InvalidOperationException("AppSettings:JwtSecret deve ter no mínimo 32 caracteres");
        
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        return services;
    }
}
