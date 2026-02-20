using Api.Contracts.Requests;
using Api.Contracts.Responses;
using Api.Models;
using Api.Repositories;

namespace Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository repository, IPermissionRepository permissionRepository, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _permissionRepository = permissionRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserOptionsResponse> GetOptionsAsync()
    {
        var permissions = await _permissionRepository.GetAllAsync();
        var permissionOptions = permissions.Select(p => new PermissionOptionResponse
        {
            Id = p.Id,
            Name = p.Name
        }).ToList();

        return new UserOptionsResponse
        {
            Permissions = permissionOptions
        };
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(ToResponse);
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
            Username = request.Username.ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            PermissionId = request.PermissionId
        };

        var created = await _repository.CreateAsync(user);
        return ToResponse(created);
    }

    public async Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null) return null;

        if (request.Name != null) user.Name = request.Name;
        if (request.Username != null) user.Username = request.Username.ToLowerInvariant();
        if (request.Password != null) user.PasswordHash = _passwordHasher.Hash(request.Password);
        if (request.PermissionId != null) user.PermissionId = request.PermissionId.Value;
        if (request.Email != null) user.Email = request.Email;

        await _repository.UpdateAsync(user);
        return ToResponse(user);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> RestoreAsync(int id)
    {
        return await _repository.RestoreAsync(id);
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.Name, user.Username, user.Email, user.AvatarUrl, user.PermissionId, user.Permission?.Name, user.CreatedAt, user.SessionAt, user.DeletedAt);
}
