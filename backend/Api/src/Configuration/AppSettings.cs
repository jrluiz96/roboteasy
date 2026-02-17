namespace Api.Configuration;

/// <summary>
/// Configurações da aplicação - mapeadas do appsettings.json ou ENV vars
/// </summary>
public class AppSettings
{
    public string JwtSecret { get; set; } = string.Empty;
    public int JwtExpirationHours { get; set; }
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}
