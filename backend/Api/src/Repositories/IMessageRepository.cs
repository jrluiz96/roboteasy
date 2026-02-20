using Api.Models;

namespace Api.Repositories;

public interface IMessageRepository
{
    Task<Message> CreateAsync(Message message);
    Task<bool> ConversationExistsAsync(long conversationId);
    Task<bool> IsUserInConversationAsync(long conversationId, int userId);
    Task AddUserToConversationAsync(long conversationId, int userId);
}
