using Api.Contracts.Requests;
using Api.Contracts.Responses;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Tags("Users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Retorna as opcoes para criacao de usuarios (permissoes disponíveis)
    /// </summary>
    [HttpGet("options")]
    public async Task<IActionResult> GetOptions()
    {
        var options = await _userService.GetOptionsAsync();
        var response = ApiResponse<UserOptionsResponse>.Success(options, "Options retrieved successfully");
        return StatusCode(response.Code, response);
    }

    /// <summary>
    /// Lista todos os usuários em ordem alfabética
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        // Garantir ordem ascendente por nome
        var orderedUsers = users.OrderBy(u => u.Name);
        var response = ApiResponse<IEnumerable<UserResponse>>.Success(orderedUsers, "Users listed successfully");
        return StatusCode(response.Code, response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            var error = ApiResponse<object>.NotFound("User not found");
            return StatusCode(error.Code, error);
        }

        var response = ApiResponse<UserResponse>.Success(user, "User found");
        return StatusCode(response.Code, response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateAsync(request);
        var response = ApiResponse<UserResponse>.Created(user, "User created successfully");
        return StatusCode(response.Code, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateAsync(id, request);
        if (user == null)
        {
            var error = ApiResponse<object>.NotFound("User not found");
            return StatusCode(error.Code, error);
        }

        var response = ApiResponse<UserResponse>.Success(user, "User updated successfully");
        return StatusCode(response.Code, response);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _userService.DeleteAsync(id);
        if (!deleted)
        {
            var error = ApiResponse<object>.NotFound("User not found");
            return StatusCode(error.Code, error);
        }

        var response = ApiResponse.Ok("User deleted successfully");
        return StatusCode(response.Code, response);
    }

    [HttpPatch("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var restored = await _userService.RestoreAsync(id);
        if (!restored)
        {
            var error = ApiResponse<object>.NotFound("User not found or already active");
            return StatusCode(error.Code, error);
        }

        var response = ApiResponse.Ok("User restored successfully");
        return StatusCode(response.Code, response);
    }
}
