namespace Api.Contracts.Responses;

public record UserResponse(
    int Id,
    string Name,
    string Username,
    int PermissionId,
    string? PermissionName,
    DateTime CreatedAt,
    DateTime? SessionAt
);
