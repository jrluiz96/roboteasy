using Api.Models;

namespace Api.Repositories;

public interface IConversationRepository
{
    Task<Conversation> CreateAsync(Conversation conversation);

    /// <summary>Conversa aberta de um cliente (usado no chat/start)</summary>
    Task<Conversation?> GetOpenByClientIdAsync(long clientId);

    /// <summary>Lista conversas abertas — aba Chats do atendente</summary>
    Task<List<Conversation>> GetActiveListAsync();

    /// <summary>Lista conversas finalizadas — aba Histórico do atendente</summary>
    Task<List<Conversation>> GetHistoryListAsync();

    /// <summary>Conversa com mensagens completas</summary>
    Task<Conversation?> GetByIdWithMessagesAsync(long id);

    /// <summary>Finaliza uma conversa</summary>
    Task<Conversation?> FinishAsync(long id);
}
