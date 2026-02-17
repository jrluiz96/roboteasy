using Api.Contracts.Requests;
using Api.Contracts.Responses;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/open")]
[Tags("Open")]
public class OpenController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public OpenController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _sessionService.LoginAsync(request);
        if (result == null)
        {
            var error = ApiResponse<object>.Unauthorized("Usuário ou senha inválidos");
            return StatusCode(error.Code, error);
        }

        var response = ApiResponse<LoginResponse>.Success(result, "Login realizado com sucesso");
        return StatusCode(response.Code, response);
    }

    [HttpGet("swagger")]
    public IActionResult Swagger()
    {
        return Redirect("/swagger");
    }
}
