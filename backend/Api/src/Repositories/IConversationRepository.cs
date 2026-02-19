using Api.Models;

namespace Api.Repositories;

public interface IConversationRepository
{
    /// <summary>
    /// Cria uma nova conversa
    /// </summary>
    Task<Conversation> CreateAsync(Conversation conversation);

    /// <summary>
    /// Retorna a conversa aberta (FinishedAt == null) de um cliente, incluindo mensagens ordenadas.
    /// Retorna null se não existir nenhuma conversa aberta.
    /// </summary>
    Task<Conversation?> GetOpenByClientIdAsync(long clientId);
}
