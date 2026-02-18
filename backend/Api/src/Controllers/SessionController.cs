using Api.Contracts.Responses;
using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/session")]
[Tags("Session")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly AppDbContext _context;

    public SessionController(AppDbContext context)
    {
        _context = context;
    }
    /// <summary>
    /// Retorna os dados da sessão atual do usuário autenticado
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSession()
    {
        // O usuário foi validado e adicionado ao HttpContext pelo SessionValidationMiddleware
        var user = HttpContext.Items["CurrentUser"] as User;
        
        if (user == null)
        {
            var error = ApiResponse<object>.Unauthorized("Sessão não encontrada");
            return StatusCode(error.Code, error);
        }

        // Busca as views que o usuário tem permissão para acessar
        var userViews = await _context.PermissionViews
            .Include(pv => pv.View)
            .Where(pv => pv.PermissionId == user.PermissionId)
            .Select(pv => new ViewResponse
            {
                Id = pv.View.Id,
                Name = pv.View.Name,
                Route = pv.View.Route,
                Icon = pv.View.Icon
            })
            .ToListAsync();

        var sessionData = new SessionResponse
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            GitHubLogin = user.GitHubLogin,
            PermissionId = user.PermissionId,
            SessionAt = user.SessionAt,
            Views = userViews
        };

        var response = ApiResponse<SessionResponse>.Success(sessionData, "Sessão válida");
        return StatusCode(response.Code, response);
    }
}

/// <summary>
/// Resposta com dados da sessão do usuário
/// </summary>
public class SessionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string? GitHubLogin { get; set; }
    public int PermissionId { get; set; }
    public DateTime? SessionAt { get; set; }
    public List<ViewResponse> Views { get; set; } = new List<ViewResponse>();
}

/// <summary>
/// Resposta com dados de uma view
/// </summary>
public class ViewResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}
