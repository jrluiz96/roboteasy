using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;

    public ConversationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation> CreateAsync(Conversation conversation)
    {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();
        return conversation;
    }

    public async Task<Conversation?> GetOpenByClientIdAsync(long clientId)
    {
        return await _context.Conversations
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .Where(c => c.ClientId == clientId && c.FinishedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
