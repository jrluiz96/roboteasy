using Api.Configuration;
using Api.Contracts.Requests;
using Api.Contracts.Responses;
using Api.Repositories;
using Microsoft.Extensions.Options;

namespace Api.Services;

public class SessionService : ISessionService
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionViewRepository _permissionViewRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public SessionService(
        IUserRepository userRepository,
        IPermissionRepository permissionRepository,
        IPermissionViewRepository permissionViewRepository,
        IPasswordHasher passwordHasher, 
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _permissionRepository = permissionRepository;
        _permissionViewRepository = permissionViewRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var username = request.Username.ToLowerInvariant();
        var user = await _userRepository.GetByUsernameAsync(username);

        if (user == null)
            return null;

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        // Gera token de sessão de 32 caracteres e salva no banco
        var sessionToken = _jwtService.GenerateSessionToken();
        user.Token = sessionToken;
        user.SessionAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Gera JWT com o token de sessão
        var jwt = _jwtService.GenerateJwt(user.Id, user.Username, sessionToken);
        var expiresAt = _jwtService.GetExpirationDate();

        return new LoginResponse(user.Id, user.Username, user.Name, jwt, expiresAt);
    }

    public async Task<SessionResponse?> GetSessionAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        var views = await _permissionViewRepository.GetViewsByPermissionIdAsync(user.PermissionId);
        var viewResponses = views.Select(v => new ViewResponse
        {
            Id = v.Id,
            Name = v.Name,
            Route = v.Route,
            Icon = v.Icon
        }).ToList();

        return new SessionResponse
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            PermissionId = user.PermissionId,
            SessionAt = user.SessionAt,
            Views = viewResponses
        };
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            // Invalida o token removendo do banco
            user.Token = null;
            await _userRepository.UpdateAsync(user);
        }
    }

    public async Task<bool> FinishTutorialAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        // Verifica se o usuário é rookie
        var rookiePermission = await _permissionRepository.GetByNameAsync("rookie");
        if (rookiePermission == null || user.PermissionId != rookiePermission.Id) return false;

        // Promove para operator
        var operatorPermission = await _permissionRepository.GetByNameAsync("operator");
        if (operatorPermission == null) return false;

        user.PermissionId = operatorPermission.Id;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
    {
        var username = request.Username.ToLowerInvariant().Trim();
        var name = request.Name.Trim();

        // Verifica se já existe
        var existing = await _userRepository.GetByUsernameAsync(username);
        if (existing != null)
            return null;

        // Busca permissão rookie
        var rookiePermission = await _permissionRepository.GetByNameAsync("rookie");
        if (rookiePermission == null)
            return null;

        var user = new Models.User
        {
            Name = name,
            Username = username,
            Email = request.Email.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            PermissionId = rookiePermission.Id,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        // Gera sessão e retorna logado
        var sessionToken = _jwtService.GenerateSessionToken();
        user.Token = sessionToken;
        user.SessionAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        var jwt = _jwtService.GenerateJwt(user.Id, user.Username, sessionToken);
        var expiresAt = _jwtService.GetExpirationDate();

        return new LoginResponse(user.Id, user.Username, user.Name, jwt, expiresAt);
    }
}
