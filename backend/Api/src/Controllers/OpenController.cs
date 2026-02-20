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
    private readonly IChatService _chatService;

    public OpenController(ISessionService sessionService, IChatService chatService)
    {
        _sessionService = sessionService;
        _chatService = chatService;
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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            var error = ApiResponse<object>.BadRequest("Todos os campos são obrigatórios");
            return StatusCode(error.Code, error);
        }

        if (request.Password.Length < 4)
        {
            var error = ApiResponse<object>.BadRequest("A senha deve ter no mínimo 4 caracteres");
            return StatusCode(error.Code, error);
        }

        var result = await _sessionService.RegisterAsync(request);
        if (result == null)
        {
            var error = ApiResponse<object>.BadRequest("Usuário já existe");
            return StatusCode(error.Code, error);
        }

        var response = ApiResponse<LoginResponse>.Success(result, "Cadastro realizado com sucesso");
        return StatusCode(response.Code, response);
    }

    [HttpGet("swagger")]
    public IActionResult Swagger()
    {
        return Redirect("/swagger");
    }

    /// <summary>
    /// Cria ou busca um cliente pelo email e retorna o token de acesso ao WebSocket
    /// </summary>
    [HttpPost("chat/start")]
    public async Task<IActionResult> ChatStart([FromBody] ChatStartRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            var error = ApiResponse<object>.BadRequest("Nome é obrigatório");
            return StatusCode(error.Code, error);
        }

        var result = await _chatService.StartAsync(request);
        var response = ApiResponse<ChatStartResponse>.Success(result, "Chat iniciado com sucesso");
        return StatusCode(response.Code, response);
    }
}
