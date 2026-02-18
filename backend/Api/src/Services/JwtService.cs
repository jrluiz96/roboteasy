using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services;

public interface IJwtService
{
    /// <summary>
    /// Gera um token de sessão de 32 caracteres aleatórios
    /// </summary>
    string GenerateSessionToken();
    
    /// <summary>
    /// Gera um JWT contendo o token de sessão e informações do usuário
    /// </summary>
    string GenerateJwt(int userId, string username, string sessionToken);
    
    /// <summary>
    /// Valida um JWT e extrai o token de sessão
    /// </summary>
    (bool isValid, int userId, string? sessionToken) ValidateJwt(string jwt);
    
    /// <summary>
    /// Retorna a data de expiração do token
    /// </summary>
    DateTime GetExpirationDate();
}

public class JwtService : IJwtService
{
    private readonly AppSettings _settings;

    public JwtService(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateSessionToken()
    {
        // Gera 24 bytes = 32 caracteres em Base64
        var bytes = new byte[24];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    public string GenerateJwt(int userId, string username, string sessionToken)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = GetExpirationDate();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim("session_token", sessionToken),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: "roboteasy",
            audience: "roboteasy",
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public (bool isValid, int userId, string? sessionToken) ValidateJwt(string jwt)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSecret));
            var handler = new JwtSecurityTokenHandler();

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "roboteasy",
                ValidAudience = "roboteasy",
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var principal = handler.ValidateToken(jwt, parameters, out _);
            
            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub) 
                ?? principal.FindFirst(ClaimTypes.NameIdentifier);
            var sessionTokenClaim = principal.FindFirst("session_token");

            if (userIdClaim == null || sessionTokenClaim == null)
                return (false, 0, null);

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return (false, 0, null);

            return (true, userId, sessionTokenClaim.Value);
        }
        catch
        {
            return (false, 0, null);
        }
    }

    public DateTime GetExpirationDate()
    {
        return DateTime.UtcNow.AddHours(_settings.JwtExpirationHours);
    }
}
