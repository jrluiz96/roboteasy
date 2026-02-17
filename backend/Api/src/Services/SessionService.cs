using Api.Configuration;
using Api.Contracts.Requests;
using Api.Contracts.Responses;
using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Services;

public class SessionService : ISessionService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AppSettings _appSettings;

    public SessionService(AppDbContext context, IPasswordHasher passwordHasher, IOptions<AppSettings> appSettings)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _appSettings = appSettings.Value;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            return null;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        // Atualiza última sessão
        user.SessionAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // TODO: Implementar geração de JWT real
        var expiresAt = DateTime.UtcNow.AddHours(_appSettings.JwtExpirationHours);
        var token = "jwt_placeholder";

        return new LoginResponse(user.Id, user.Username, user.Name, token, expiresAt);
    }

    public async Task LogoutAsync(int userId)
    {
        // TODO: Implementar invalidação de token se necessário
        await Task.CompletedTask;
    }
}
