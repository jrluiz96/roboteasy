using Api.Contracts.Responses;
using Api.Hubs;
using Api.Models;
using Api.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace Api.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _repository;
    private readonly IHubContext<ChatHub> _hub;

    public ConversationService(IConversationRepository repository, IHubContext<ChatHub> hub)
    {
        _repository = repository;
        _hub = hub;
    }

    public async Task<List<ConversationListItemResponse>> GetActiveAsync(int userId)
    {
        var conversations = await _repository.GetActiveListAsync(userId);
        return conversations.Select(x => ToListItem(x.Conv, x.MessageCount)).ToList();
    }

    public async Task<List<ConversationListItemResponse>> GetAllActiveAsync()
    {
        var conversations = await _repository.GetAllActiveListAsync();
        return conversations.Select(x => ToListItem(x.Conv, x.MessageCount)).ToList();
    }

    public async Task<List<ConversationListItemResponse>> GetHistoryAsync()
    {
        var conversations = await _repository.GetHistoryListAsync();
        return conversations.Select(x => ToListItem(x.Conv, x.MessageCount)).ToList();
    }

    public async Task<ConversationDetailResponse?> GetByIdAsync(long id)
    {
        var conv = await _repository.GetByIdWithMessagesAsync(id);
        if (conv == null) return null;

        var messages = conv.Messages
            .Select(m => new MessageResponse(
                m.Id, m.ConversationId, m.UserId, m.ClientId,
                m.Type, m.Content, m.User?.Name ?? m.Client?.Name, m.FileUrl, m.CreatedAt))
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
            messages,
            conv.UserConversations
                .Where(uc => uc.FinishedAt == null)
                .Select(uc => new ConversationAttendantResponse(uc.UserId, uc.User.Name, null))
                .ToList()
                .AsReadOnly()
        );
    }

    public async Task<bool> FinishAsync(long id)
    {
        var result = await _repository.FinishAsync(id);
        if (result == null) return false;

        // Notifica todos os participantes do grupo (incluindo o cliente)
        await _hub.Clients
            .Group($"conversation:{id}")
            .SendAsync(ChatEvents.ConversationFinished, new { conversationId = id });

        return true;
    }

    public Task<bool> JoinAsync(long id, int userId)
        => _repository.JoinAsync(id, userId);

    public async Task<bool> InviteAttendantAsync(long id, int invitedUserId)
    {
        var (success, wsConn) = await _repository.InviteAttendantAsync(id, invitedUserId);
        if (!success) return false;

        var payload = new { conversationId = id, invitedUserId };

        // Notifica o atendente convidado diretamente (se estiver online)
        if (wsConn != null)
            await _hub.Clients.Client(wsConn)
                .SendAsync(ChatEvents.ConversationInvited, payload);

        // Também notifica todos no grupo da conversa (redundância: operadores que
        // entraram no grupo via ConversationCreated recebem o evento mesmo se o
        // wsConn direto estiver desatualizado).
        await _hub.Clients.Group($"conversation:{id}")
            .SendAsync(ChatEvents.ConversationInvited, payload);

        return true;
    }

    public async Task<bool> LeaveAsync(long id, int userId)
    {
        var userName = await _repository.LeaveAsync(id, userId);
        if (userName == null) return false;

        await _hub.Clients
            .Group($"conversation:{id}")
            .SendAsync(ChatEvents.AttendantLeft, new
            {
                conversationId = id,
                userId,
                userName
            });

        return true;
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static ConversationListItemResponse ToListItem(Conversation conv, int messageCount)
    {
        var last = conv.Messages.FirstOrDefault();
        var attendants = conv.UserConversations?
            .Select(uc => uc.User)
            .Where(u => u != null)
            .DistinctBy(u => u.Id)
            .Select(u => new ConversationAttendantResponse(u.Id, u.Name, u.AvatarUrl))
            .ToList()
            .AsReadOnly()
            ?? (IReadOnlyList<ConversationAttendantResponse>)Array.Empty<ConversationAttendantResponse>();

        return new ConversationListItemResponse(
            conv.Id,
            conv.Client.Id,
            conv.Client.Name,
            conv.Client.Email,
            last?.Content,
            last?.CreatedAt,
            messageCount,
            conv.CreatedAt,
            conv.FinishedAt,
            ResolveStatus(conv),
            attendants
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
