using Api.Contracts.Requests;
using Api.Contracts.Responses;

namespace Api.Services;

public interface ISessionService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task LogoutAsync(int userId);
    Task<SessionResponse?> GetSessionAsync(int userId);
}
