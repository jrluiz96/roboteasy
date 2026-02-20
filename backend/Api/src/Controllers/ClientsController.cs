using Api.Contracts.Responses;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/clients")]
[Tags("Clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    /// <summary>
    /// Lista todos os clientes com estatísticas
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clients = await _clientService.GetAllAsync();
        var response = ApiResponse<IEnumerable<ClientResponse>>.Success(clients, "Clients listed successfully");
        return StatusCode(response.Code, response);
    }
}
