using Api.Contracts.Responses;
using Api.Models;
using Api.Repositories;

namespace Api.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _repository;

    public ConversationService(IConversationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ConversationListItemResponse>> GetActiveAsync()
    {
        var conversations = await _repository.GetActiveListAsync();
        return conversations.Select(ToListItem).ToList();
    }

    public async Task<List<ConversationListItemResponse>> GetHistoryAsync()
    {
        var conversations = await _repository.GetHistoryListAsync();
        return conversations.Select(ToListItem).ToList();
    }

    public async Task<ConversationDetailResponse?> GetByIdAsync(long id)
    {
        var conv = await _repository.GetByIdWithMessagesAsync(id);
        if (conv == null) return null;

        var messages = conv.Messages
            .Select(m => new MessageResponse(
                m.Id, m.ConversationId, m.UserId, m.ClientId,
                m.Type, m.Content, m.FileUrl, m.CreatedAt))
            .ToList();

        return new ConversationDetailResponse(
            conv.Id,
            conv.Client.Id,
            conv.Client.Name,
            conv.Client.Email,
            conv.Client.Phone,
            conv.CreatedAt,
            conv.FinishedAt,
            conv.AttendanceTime,
            ResolveStatus(conv),
            messages
        );
    }

    public async Task<bool> FinishAsync(long id)
    {
        var result = await _repository.FinishAsync(id);
        return result != null;
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static ConversationListItemResponse ToListItem(Conversation conv)
    {
        var last = conv.Messages.FirstOrDefault();
        return new ConversationListItemResponse(
            conv.Id,
            conv.Client.Id,
            conv.Client.Name,
            conv.Client.Email,
            last?.Content,
            last?.CreatedAt,
            conv.Messages.Count,
            conv.CreatedAt,
            conv.FinishedAt,
            ResolveStatus(conv)
        );
    }

    private static string ResolveStatus(Conversation conv)
    {
        if (conv.FinishedAt != null) return "finished";
        // "waiting" = ninguém entrou ainda; "active" = tem UserConversation sem FinishedAt
        var hasAttendant = conv.UserConversations?.Any(uc => uc.FinishedAt == null) == true;
        return hasAttendant ? "active" : "waiting";
    }
}
