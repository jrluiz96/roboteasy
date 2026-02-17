using Api.Contracts.Requests;
using Api.Contracts.Responses;
using Api.Services;

namespace Api.Routes;

/// <summary>
/// Alternativa ao Controller - Minimal APIs (estilo Gin)
/// Descomente no Program.cs se preferir este estilo
/// </summary>
public static class UserRoutes
{
    public static void MapUserRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", async (IUserService service) =>
            Results.Ok(await service.GetAllAsync()));

        group.MapGet("/{id}", async (int id, IUserService service) =>
            await service.GetByIdAsync(id) is { } user
                ? Results.Ok(user)
                : Results.NotFound());

        group.MapPost("/", async (CreateUserRequest request, IUserService service) =>
        {
            var user = await service.CreateAsync(request);
            return Results.Created($"/api/users/{user.Id}", user);
        });

        group.MapPut("/{id}", async (int id, UpdateUserRequest request, IUserService service) =>
            await service.UpdateAsync(id, request) is { } user
                ? Results.Ok(user)
                : Results.NotFound());

        group.MapDelete("/{id}", async (int id, IUserService service) =>
            await service.DeleteAsync(id)
                ? Results.NoContent()
                : Results.NotFound());
    }
}
