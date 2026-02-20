namespace Api.Contracts.Responses;

public record ConversationAttendantResponse(
    int UserId,
    string Name,
    string? AvatarUrl
);
