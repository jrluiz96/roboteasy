namespace Api.Contracts.Responses;

/// <summary>
/// Resposta com opcoes para usuarios
/// </summary>
public class UserOptionsResponse
{
    public List<PermissionOptionResponse> Permissions { get; set; } = new List<PermissionOptionResponse>();
}

/// <summary>
/// Resposta com opcao de permissao
/// </summary>
public class PermissionOptionResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}