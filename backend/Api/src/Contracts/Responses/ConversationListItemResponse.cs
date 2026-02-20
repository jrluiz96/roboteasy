namespace Api.Contracts.Responses;

public record ConversationListItemResponse(
    long Id,
    long ClientId,
    string ClientName,
    string? ClientEmail,
    string? LastMessage,
    DateTime? LastMessageAt,
    int MessageCount,
    DateTime CreatedAt,
    DateTime? FinishedAt,
    /// <summary>waiting = sem atendente, active = com atendente, finished = encerrada</summary>
    string Status,
    IReadOnlyList<ConversationAttendantResponse> Attendants
);
