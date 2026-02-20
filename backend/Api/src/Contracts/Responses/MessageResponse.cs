using Api.Models;

namespace Api.Contracts.Responses;

public record MessageResponse(
    long Id,
    long ConversationId,
    int? UserId,
    long? ClientId,
    MessageType Type,
    string Content,
    string? SenderName,
    string? FileUrl,
    DateTime CreatedAt
);
