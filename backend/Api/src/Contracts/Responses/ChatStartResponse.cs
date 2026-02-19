namespace Api.Contracts.Responses;

public record ChatStartResponse(
    long ClientId,
    string ClientToken,
    long ConversationId,
    /// <summary>
    /// true  = conversa recém-criada (sem histórico)
    /// false = conversa existente foi retomada (histórico disponível em Messages)
    /// </summary>
    bool IsNewConversation,
    IReadOnlyList<MessageResponse> Messages
);
