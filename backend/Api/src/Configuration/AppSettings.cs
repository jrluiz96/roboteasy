namespace Api.Configuration;

/// <summary>
/// Configurações da aplicação - mapeadas do appsettings.json ou ENV vars
/// </summary>
public class AppSettings
{
    public string JwtSecret { get; set; } = string.Empty;
    public int JwtExpirationMinutes { get; set; } = 60;
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
}
