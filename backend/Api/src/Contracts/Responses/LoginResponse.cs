namespace Api.Contracts.Responses;

public record LoginResponse(int UserId, string Username, string Name, string Token, DateTime ExpiresAt);
