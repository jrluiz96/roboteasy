using Api.Contracts.Responses;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/session")]
[Tags("Session")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionController(ISessionService sessionService)
    {
        _sessionService = sessionService;
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
            var error = ApiResponse<object>.Unauthorized("Session not found");
            return StatusCode(error.Code, error);
        }

        var sessionData = await _sessionService.GetSessionAsync(user.Id);
        if (sessionData == null)
        {
            var error = ApiResponse<object>.NotFound("Session data not found");
            return StatusCode(error.Code, error);
        }

        var response = ApiResponse<SessionResponse>.Success(sessionData, "Session valid");
        return StatusCode(response.Code, response);
    }
}
