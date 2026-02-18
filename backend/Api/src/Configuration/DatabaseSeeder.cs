using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Api.Configuration;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Garante que as migrations foram aplicadas
        await context.Database.MigrateAsync();

        // Seed de permissões
        if (!await context.Permissions.AnyAsync())
        {
            context.Permissions.AddRange(
                new Permission { Name = "admin" },
                new Permission { Name = "operator" }
            );
            await context.SaveChangesAsync();
        }

        // Seed de usuário admin
        if (!await context.Users.AnyAsync(u => u.Username == "admin.master"))
        {
            var adminPermission = await context.Permissions.FirstAsync(p => p.Name == "admin");
            
            context.Users.Add(new User
            {
                Name = "Administrador Master",
                Username = "admin.master",
                Email = "admin@roboteasy.com",
                PasswordHash = passwordHasher.Hash("MyAdm2026TestCode"),
                PermissionId = adminPermission.Id
            });
            await context.SaveChangesAsync();
        }

        // Seed de usuário operador
        if (!await context.Users.AnyAsync(u => u.Username == "francisco.luiz"))
        {
            var operatorPermission = await context.Permissions.FirstAsync(p => p.Name == "operator");
            
            context.Users.Add(new User
            {
                Name = "Francisco Luiz",
                Username = "francisco.luiz",
                Email = "francisco.luiz.jr@outlook.com",
                PasswordHash = passwordHasher.Hash("CodandoEmC#"),
                PermissionId = operatorPermission.Id
            });
            await context.SaveChangesAsync();
        }
    }
}
