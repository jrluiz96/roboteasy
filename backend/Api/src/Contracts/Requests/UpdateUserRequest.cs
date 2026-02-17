namespace Api.Contracts.Requests;

public record UpdateUserRequest(string? Name, string? Username, int? PermissionId);
