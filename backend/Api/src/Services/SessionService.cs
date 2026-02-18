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
    private readonly IJwtService _jwtService;

    public SessionService(AppDbContext context, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var username = request.Username.ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return null;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        // Gera token de sessão de 32 caracteres e salva no banco
        var sessionToken = _jwtService.GenerateSessionToken();
        user.Token = sessionToken;
        user.SessionAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Gera JWT com o token de sessão
        var jwt = _jwtService.GenerateJwt(user.Id, user.Username, sessionToken);
        var expiresAt = _jwtService.GetExpirationDate();

        return new LoginResponse(user.Id, user.Username, user.Name, jwt, expiresAt);
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            // Invalida o token removendo do banco
            user.Token = null;
            await _context.SaveChangesAsync();
        }
    }
}
