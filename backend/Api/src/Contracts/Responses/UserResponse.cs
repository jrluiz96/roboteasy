namespace Api.Contracts.Responses;

public record UserResponse(
    int Id,
    string Name,
    string Username,
    string? Email,
    string? AvatarUrl,
    int PermissionId,
    string? PermissionName,
    DateTime CreatedAt,
    DateTime? SessionAt,
    DateTime? DeletedAt,
    bool IsOnline
);
