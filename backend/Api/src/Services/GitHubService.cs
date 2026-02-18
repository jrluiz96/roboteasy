using System.Net.Http.Headers;
using System.Text.Json;
using Api.Configuration;
using Api.Contracts.Responses;
using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Services;

public interface IGitHubService
{
    string GetAuthorizationUrl();
    Task<LoginResponse?> HandleCallbackAsync(string code);
}

public class GitHubService : IGitHubService
{
    private readonly GitHubSettings _settings;
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IJwtService _jwtService;

    private const string GitHubAuthorizeUrl = "https://github.com/login/oauth/authorize";
    private const string GitHubTokenUrl = "https://github.com/login/oauth/access_token";
    private const string GitHubUserUrl = "https://api.github.com/user";

    public GitHubService(
        IOptions<GitHubSettings> settings,
        AppDbContext context,
        HttpClient httpClient,
        IJwtService jwtService)
    {
        _settings = settings.Value;
        _context = context;
        _httpClient = httpClient;
        _jwtService = jwtService;
    }

    public string GetAuthorizationUrl()
    {
        var scope = "read:user user:email";
        return $"{GitHubAuthorizeUrl}?client_id={_settings.ClientId}&redirect_uri={Uri.EscapeDataString(_settings.CallbackUrl)}&scope={Uri.EscapeDataString(scope)}";
    }

    public async Task<LoginResponse?> HandleCallbackAsync(string code)
    {
        // 1. Trocar code por access_token
        var accessToken = await ExchangeCodeForTokenAsync(code);
        if (string.IsNullOrEmpty(accessToken))
            return null;

        // 2. Buscar dados do usuário no GitHub
        var githubUser = await GetGitHubUserAsync(accessToken);
        if (githubUser == null)
            return null;

        // 3. Criar ou atualizar usuário local
        var user = await GetOrCreateUserAsync(githubUser);

        // 4. Gera token de sessão de 32 caracteres e salva no banco
        var sessionToken = _jwtService.GenerateSessionToken();
        user.Token = sessionToken;
        user.SessionAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // 5. Gera JWT com o token de sessão
        var jwt = _jwtService.GenerateJwt(user.Id, user.Username, sessionToken);
        var expiresAt = _jwtService.GetExpirationDate();

        return new LoginResponse(user.Id, user.Username, user.Name, jwt, expiresAt);
    }

    private async Task<string?> ExchangeCodeForTokenAsync(string code)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, GitHubTokenUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["code"] = code,
            ["redirect_uri"] = _settings.CallbackUrl
        });
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        
        if (doc.RootElement.TryGetProperty("access_token", out var tokenElement))
            return tokenElement.GetString();

        return null;
    }

    private async Task<GitHubUser?> GetGitHubUserAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, GitHubUserUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.ParseAdd("RobotEasy-API");

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GitHubUser>(json, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
    }

    private async Task<User> GetOrCreateUserAsync(GitHubUser githubUser)
    {
        // Buscar usuário existente pelo GitHub ID
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.GitHubId == githubUser.Id);

        if (user != null)
        {
            // Atualizar dados do GitHub
            user.GitHubLogin = githubUser.Login;
            user.AvatarUrl = githubUser.AvatarUrl;
            user.Name = githubUser.Name ?? githubUser.Login;
            user.Email = githubUser.Email;
            user.UpdatedAt = DateTime.UtcNow;
            return user;
        }

        // Criar novo usuário
        user = new User
        {
            Name = githubUser.Name ?? githubUser.Login,
            Username = githubUser.Login.ToLowerInvariant(),
            Email = githubUser.Email,
            PasswordHash = string.Empty, // OAuth não usa senha
            GitHubId = githubUser.Id,
            GitHubLogin = githubUser.Login,
            AvatarUrl = githubUser.AvatarUrl,
            PermissionId = 1 // TODO: Definir permissão padrão para OAuth
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }
}

// DTO para resposta da API do GitHub
public class GitHubUser
{
    public long Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
}
