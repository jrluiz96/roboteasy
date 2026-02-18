using Api.Contracts.Responses;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/session")]
[Tags("Session")]
[Authorize]
public class SessionController : ControllerBase
{
    /// <summary>
    /// Retorna os dados da sessão atual do usuário autenticado
    /// </summary>
    [HttpGet]
    public IActionResult GetSession()
    {
        // O usuário foi validado e adicionado ao HttpContext pelo SessionValidationMiddleware
        var user = HttpContext.Items["CurrentUser"] as User;
        
        if (user == null)
        {
            var error = ApiResponse<object>.Unauthorized("Sessão não encontrada");
            return StatusCode(error.Code, error);
        }

        var sessionData = new SessionResponse
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            GitHubLogin = user.GitHubLogin,
            PermissionId = user.PermissionId,
            SessionAt = user.SessionAt
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
}
