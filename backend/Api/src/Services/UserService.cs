using Api.Contracts.Requests;
using Api.Contracts.Responses;
using Api.Models;
using Api.Repositories;

namespace Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(u => ToResponse(u));
    }

    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user == null ? null : ToResponse(user);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Name = request.Name,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PermissionId = request.PermissionId
        };

        var created = await _repository.CreateAsync(user);
        return ToResponse(created);
    }

    public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        if (request.Name != null) existing.Name = request.Name;
        if (request.Username != null) existing.Username = request.Username;
        if (request.PermissionId != null) existing.PermissionId = request.PermissionId.Value;

        var updated = await _repository.UpdateAsync(existing);
        return updated == null ? null : ToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Name, user.Username, user.PermissionId, user.Permission?.Name, user.CreatedAt, user.SessionAt);
}
