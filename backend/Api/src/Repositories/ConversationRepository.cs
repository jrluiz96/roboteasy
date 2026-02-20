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
                .ThenInclude(m => m.User)
            .Where(c => c.ClientId == clientId && c.FinishedAt == null)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<(Conversation Conv, int MessageCount)>> GetActiveListAsync(int userId)
    {
        var conversations = await _context.Conversations
            .Include(c => c.Client)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .Include(c => c.UserConversations)
                .ThenInclude(uc => uc.User)
            .Where(c => c.FinishedAt == null &&
                (
                    // waiting: sem nenhum atendente vinculado → visível a todos
                    !c.UserConversations.Any(uc => uc.FinishedAt == null) ||
                    // active: visível apenas para atendentes vinculados
                    c.UserConversations.Any(uc => uc.FinishedAt == null && uc.UserId == userId)
                ))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var ids = conversations.Select(c => c.Id).ToList();
        var counts = await _context.Messages
            .Where(m => ids.Contains(m.ConversationId))
            .GroupBy(m => m.ConversationId)
            .Select(g => new { ConversationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ConversationId, x => x.Count);

        return conversations.Select(c => (c, counts.GetValueOrDefault(c.Id, 0))).ToList();
    }

    public async Task<List<(Conversation Conv, int MessageCount)>> GetHistoryListAsync()
    {
        var conversations = await _context.Conversations
            .Include(c => c.Client)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .Include(c => c.UserConversations)
                .ThenInclude(uc => uc.User)
            .Where(c => c.FinishedAt != null)
            .OrderByDescending(c => c.FinishedAt)
            .ToListAsync();

        var ids = conversations.Select(c => c.Id).ToList();
        var counts = await _context.Messages
            .Where(m => ids.Contains(m.ConversationId))
            .GroupBy(m => m.ConversationId)
            .Select(g => new { ConversationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ConversationId, x => x.Count);

        return conversations.Select(c => (c, counts.GetValueOrDefault(c.Id, 0))).ToList();
    }

    public async Task<Conversation?> GetByIdWithMessagesAsync(long id)
    {
        return await _context.Conversations
            .Include(c => c.Client)
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .ThenInclude(m => m.User)
            .Include(c => c.Messages)
                .ThenInclude(m => m.Client)
            .Include(c => c.UserConversations)
                .ThenInclude(uc => uc.User)
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

    public async Task<bool> JoinAsync(long conversationId, int userId)
    {
        var conversation = await _context.Conversations
            .Include(c => c.UserConversations)
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.FinishedAt == null);
        if (conversation == null) return false;

        // Já está vinculado — idempotente
        if (conversation.UserConversations.Any(uc => uc.UserId == userId && uc.FinishedAt == null))
            return true;

        _context.Set<UserConversation>().Add(new UserConversation
        {
            UserId         = userId,
            ConversationId = conversationId,
            StartedAt      = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string? WsConn)> InviteAttendantAsync(long conversationId, int invitedUserId)
    {
        var conversation = await _context.Conversations
            .Include(c => c.UserConversations)
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.FinishedAt == null);
        if (conversation == null) return (false, null);

        // Já está vinculado — idempotente, mas sem re-notificar
        if (conversation.UserConversations.Any(uc => uc.UserId == invitedUserId && uc.FinishedAt == null))
            return (true, null);

        _context.Set<UserConversation>().Add(new UserConversation
        {
            UserId         = invitedUserId,
            ConversationId = conversationId,
            StartedAt      = DateTime.UtcNow
        });

        var user = await _context.Users.FindAsync(invitedUserId);
        await _context.SaveChangesAsync();
        return (true, user?.WsConn);
    }

    public async Task<string?> LeaveAsync(long conversationId, int userId)
    {
        var uc = await _context.Set<UserConversation>()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.ConversationId == conversationId
                                   && x.UserId == userId
                                   && x.FinishedAt == null);
        if (uc == null) return null;

        uc.FinishedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return uc.User.Name;
    }
}
