using Api.Contracts.Requests;
using Api.Contracts.Responses;

namespace Api.Services;

public interface IUserService
{
    Task<UserOptionsResponse> GetOptionsAsync();
    Task<IEnumerable<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByIdAsync(int id);
    Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> RestoreAsync(int id);
}
