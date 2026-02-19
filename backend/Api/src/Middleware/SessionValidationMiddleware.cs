using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Middleware;

/// <summary>
/// Middleware que valida se o session_token do JWT ainda é válido no banco de dados.
/// Isso permite invalidar tokens via logout (setando Token = null no usuário).
/// </summary>
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        // Se o usuário está autenticado com JWT, valida o session_token
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(JwtRegisteredClaimNames.Sub) 
                ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
            var sessionTokenClaim = context.User.FindFirst("session_token");

            if (userIdClaim != null && sessionTokenClaim != null)
            {
                if (int.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await dbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    // Verifica se o usuário existe, não foi deletado e se o session_token bate
                    if (user == null || user.DeletedAt != null || user.Token != sessionTokenClaim.Value)
                    {
                        await WriteUnauthorizedResponse(context, "Session invalid or expired");
                        return;
                    }

                    // Adiciona o usuário ao HttpContext para uso nos controllers
                    context.Items["CurrentUser"] = user;
                }
            }
        }

        await _next(context);
    }

    private static async Task WriteUnauthorizedResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        var response = new
        {
            code = 401,
            status = "error",
            message = message,
            data = (object?)null
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        
        await context.Response.WriteAsync(json);
    }
}

public static class SessionValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SessionValidationMiddleware>();
    }
}
