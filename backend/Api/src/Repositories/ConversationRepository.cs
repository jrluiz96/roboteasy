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

    public async Task<List<Conversation>> GetActiveListAsync()
    {
        return await _context.Conversations
            .Include(c => c.Client)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .Include(c => c.UserConversations)
            .Where(c => c.FinishedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Conversation>> GetHistoryListAsync()
    {
        return await _context.Conversations
            .Include(c => c.Client)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .Include(c => c.UserConversations)
            .Where(c => c.FinishedAt != null)
            .OrderByDescending(c => c.FinishedAt)
            .ToListAsync();
    }

    public async Task<Conversation?> GetByIdWithMessagesAsync(long id)
    {
        return await _context.Conversations
            .Include(c => c.Client)
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Conversation?> FinishAsync(long id)
    {
        var conversation = await _context.Conversations.FindAsync(id);
        if (conversation == null || conversation.FinishedAt != null) return null;

        conversation.FinishedAt = DateTime.UtcNow;
        if (conversation.CreatedAt != default)
            conversation.AttendanceTime = (int)(conversation.FinishedAt.Value - conversation.CreatedAt).TotalSeconds;

        await _context.SaveChangesAsync();
        return conversation;
    }
}
