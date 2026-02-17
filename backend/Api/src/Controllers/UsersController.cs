using Api.Contracts.Requests;
using Api.Contracts.Responses;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Tags("Users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        var response = ApiResponse<IEnumerable<UserResponse>>.Success(users, "Usuários listados com sucesso");
        return StatusCode(response.Code, response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            var error = ApiResponse<object>.NotFound("Usuário não encontrado");
            return StatusCode(error.Code, error);
        }

        var response = ApiResponse<UserResponse>.Success(user, "Usuário encontrado");
        return StatusCode(response.Code, response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateAsync(request);
        var response = ApiResponse<UserResponse>.Created(user, "Usuário criado com sucesso");
        return StatusCode(response.Code, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateAsync(id, request);
        if (user == null)
        {
            var error = ApiResponse<object>.NotFound("Usuário não encontrado");
            return StatusCode(error.Code, error);
        }

        var response = ApiResponse<UserResponse>.Success(user, "Usuário atualizado com sucesso");
        return StatusCode(response.Code, response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeleteAsync(id);
        if (!deleted)
        {
            var error = ApiResponse<object>.NotFound("Usuário não encontrado");
            return StatusCode(error.Code, error);
        }

        var response = ApiResponse.Ok("Usuário removido com sucesso");
        return StatusCode(response.Code, response);
    }
}
