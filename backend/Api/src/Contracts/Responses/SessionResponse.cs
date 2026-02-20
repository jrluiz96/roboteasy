namespace Api.Contracts.Responses;

/// <summary>
/// Resposta com dados da sessão do usuário
/// </summary>
public class SessionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public int PermissionId { get; set; }
    public DateTime? SessionAt { get; set; }
    public List<ViewResponse> Views { get; set; } = new List<ViewResponse>();
}

/// <summary>
/// Resposta com dados de uma view
/// </summary>
public class ViewResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}