namespace Api.Contracts.Responses;

public record ConversationDetailResponse(
    long Id,
    long ClientId,
    string ClientName,
    string? ClientEmail,
    string? ClientPhone,
    DateTime CreatedAt,
    DateTime? FinishedAt,
    int? AttendanceTime,
    string Status,
    IReadOnlyList<MessageResponse> Messages,
    IReadOnlyList<ConversationAttendantResponse> Attendants
);
