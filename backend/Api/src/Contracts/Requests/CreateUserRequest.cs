namespace Api.Contracts.Requests;

public record CreateUserRequest(string Name, string Username, string Password, int PermissionId);
