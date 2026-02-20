namespace Api.Contracts.Responses;

public record ClientResponse(
    long Id,
    string Name,
    string? Email,
    string? Phone,
    string? Cpf,
    int ConversationCount,
    int MessageCount,
    DateTime? LastConversationAt,
    bool IsOnline,
    DateTime CreatedAt
);
