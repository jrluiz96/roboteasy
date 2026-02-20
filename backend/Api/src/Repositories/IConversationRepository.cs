using Api.Models;

namespace Api.Repositories;

public interface IConversationRepository
{
    Task<Conversation> CreateAsync(Conversation conversation);

    /// <summary>Conversa aberta de um cliente (usado no chat/start)</summary>
    Task<Conversation?> GetOpenByClientIdAsync(long clientId);

    /// <summary>Lista conversas visíveis ao atendente: waiting (todas) + active (só as vinculadas)</summary>
    Task<List<(Conversation Conv, int MessageCount)>> GetActiveListAsync(int userId);

    /// <summary>Lista TODAS as conversas ativas (waiting + active) — usado pelo monitoramento</summary>
    Task<List<(Conversation Conv, int MessageCount)>> GetAllActiveListAsync();

    /// <summary>Lista conversas finalizadas — aba Histórico do atendente</summary>
    Task<List<(Conversation Conv, int MessageCount)>> GetHistoryListAsync();

    /// <summary>Conversa com mensagens e atendentes vinculados</summary>
    Task<Conversation?> GetByIdWithMessagesAsync(long id);

    /// <summary>Finaliza uma conversa</summary>
    Task<Conversation?> FinishAsync(long id);

    /// <summary>Atendente puxa a conversa para si (cria UserConversation)</summary>
    Task<bool> JoinAsync(long conversationId, int userId);

    /// <summary>Convida outro atendente para participar da conversa</summary>
    /// <summary>Returns (Success, WsConn of invited user). WsConn may be null if user is offline.</summary>
    Task<(bool Success, string? WsConn)> InviteAttendantAsync(long conversationId, int invitedUserId);

    /// <summary>Atendente sai da conversa (sem finalizá-la). Retorna o nome do atendente ou null se não encontrado.</summary>
    Task<string?> LeaveAsync(long conversationId, int userId);
}
