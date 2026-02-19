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
        if (!await context.Users.IgnoreQueryFilters().AnyAsync(u => u.Username == "admin.master"))
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
        if (!await context.Users.IgnoreQueryFilters().AnyAsync(u => u.Username == "francisco.luiz"))
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

        // Seed de Views
        if (!await context.Views.AnyAsync())
        {
            context.Views.AddRange(
                new View { Name = "home", Route = "/session/home", Icon = "fa-house" },
                new View { Name = "customer_service", Route = "/session/customer-service", Icon = "fa-headset" },
                new View { Name = "clients", Route = "/session/clients", Icon = "fa-user" },
                new View { Name = "history", Route = "/session/history", Icon = "fa-clock-rotate-left" },
                new View { Name = "users", Route = "/session/users", Icon = "fa-users" },
                new View { Name = "monitoring", Route = "/session/monitoring", Icon = "fa-chart-line" }
            );
            await context.SaveChangesAsync();
        }

        // Seed de PermissionView (relações entre permissões e views)
        if (!await context.PermissionViews.AnyAsync())
        {
            var adminPermission = await context.Permissions.FirstAsync(p => p.Name == "admin");
            var operatorPermission = await context.Permissions.FirstAsync(p => p.Name == "operator");

            var homeView = await context.Views.FirstAsync(v => v.Name == "home");
            var customerServiceView = await context.Views.FirstAsync(v => v.Name == "customer_service");
            var clientsView = await context.Views.FirstAsync(v => v.Name == "clients");
            var historyView = await context.Views.FirstAsync(v => v.Name == "history");
            var usersView = await context.Views.FirstAsync(v => v.Name == "users");
            var monitoringView = await context.Views.FirstAsync(v => v.Name == "monitoring");

            context.PermissionViews.AddRange(
                // Views acessíveis por todas as permissões (admin e operator)
                new PermissionView { PermissionId = adminPermission.Id, ViewId = homeView.Id },
                new PermissionView { PermissionId = operatorPermission.Id, ViewId = homeView.Id },
                new PermissionView { PermissionId = adminPermission.Id, ViewId = customerServiceView.Id },
                new PermissionView { PermissionId = operatorPermission.Id, ViewId = customerServiceView.Id },

                // Views acessíveis apenas por admin
                new PermissionView { PermissionId = adminPermission.Id, ViewId = clientsView.Id },
                new PermissionView { PermissionId = adminPermission.Id, ViewId = historyView.Id },
                new PermissionView { PermissionId = adminPermission.Id, ViewId = usersView.Id },
                new PermissionView { PermissionId = adminPermission.Id, ViewId = monitoringView.Id }
            );
            await context.SaveChangesAsync();
        }
    }
}
