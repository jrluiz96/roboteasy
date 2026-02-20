using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Message> CreateAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<bool> ConversationExistsAsync(long conversationId)
    {
        return await _context.Conversations
            .AnyAsync(c => c.Id == conversationId && c.FinishedAt == null);
    }

    public async Task<bool> IsUserInConversationAsync(long conversationId, int userId)
    {
        return await _context.Set<UserConversation>()
            .AnyAsync(uc => uc.ConversationId == conversationId
                         && uc.UserId == userId
                         && uc.FinishedAt == null);
    }

    public async Task AddUserToConversationAsync(long conversationId, int userId)
    {
        _context.Set<UserConversation>().Add(new UserConversation
        {
            UserId         = userId,
            ConversationId = conversationId,
            StartedAt      = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
