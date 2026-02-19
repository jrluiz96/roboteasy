using Api.Data;
using Api.Models;
using Api.Repositories;
using Api.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Hubs;

/// <summary>
/// Hub único que aceita dois tipos de conexão:
/// - Usuário (atendente): autenticado via JWT Bearer → query param "access_token"
/// - Cliente externo:     autenticado via client token → query param "client_token"
/// </summary>
public class ChatHub : Hub
{
    private readonly AppDbContext _context;
    private readonly IClientRepository _clientRepository;
    private readonly IJwtService _jwtService;

    public ChatHub(AppDbContext context, IClientRepository clientRepository, IJwtService jwtService)
    {
        _context = context;
        _clientRepository = clientRepository;
        _jwtService = jwtService;
    }

    // ── Conexão ──────────────────────────────────────────────────────────────

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var clientId = GetClientId();

        if (userId != null)
        {
            // Atendente conectou
            var user = await _context.Users.FindAsync(userId.Value);
            if (user != null)
            {
                user.WsConn = Context.ConnectionId;
                await _context.SaveChangesAsync();
            }

            await Clients.Others.SendAsync(ChatEvents.UserOnline, new
            {
                UserId = userId.Value,
                ConnectionId = Context.ConnectionId
            });
        }
        else if (clientId != null)
        {
            // Cliente externo conectou
            await _clientRepository.UpdateWsConnAsync(clientId.Value, Context.ConnectionId);
        }
        else
        {
            // Sem identificação válida — recusa conexão
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var clientId = GetClientId();

        if (userId != null)
        {
            var user = await _context.Users.FindAsync(userId.Value);
            if (user != null)
            {
                user.WsConn = null;
                await _context.SaveChangesAsync();
            }

            await Clients.Others.SendAsync(ChatEvents.UserOffline, new
            {
                UserId = userId.Value
            });
        }
        else if (clientId != null)
        {
            await _clientRepository.UpdateWsConnAsync(clientId.Value, null);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ── Conversa ─────────────────────────────────────────────────────────────

    public async Task JoinConversation(long conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));
    }

    public async Task LeaveConversation(long conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));
    }

    // ── Mensagem ─────────────────────────────────────────────────────────────

    public async Task SendMessage(long conversationId, string content)
    {
        var userId = GetUserId();
        var clientId = GetClientId();

        if (userId == null && clientId == null) return;

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.FinishedAt == null);

        if (conversation == null) return;

        var message = new Message
        {
            ConversationId = conversationId,
            UserId = userId,
            ClientId = clientId,
            Type = MessageType.Text,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        await Clients.Group(ConversationGroup(conversationId))
            .SendAsync(ChatEvents.ReceiveMessage, new
            {
                message.Id,
                message.ConversationId,
                message.UserId,
                message.ClientId,
                message.Type,
                message.Content,
                message.CreatedAt
            });
    }

    // ── Typing ───────────────────────────────────────────────────────────────

    public async Task Typing(long conversationId)
    {
        await Clients.OthersInGroup(ConversationGroup(conversationId))
            .SendAsync(ChatEvents.Typing, new
            {
                ConversationId = conversationId,
                UserId = GetUserId(),
                ClientId = GetClientId()
            });
    }

    public async Task StopTyping(long conversationId)
    {
        await Clients.OthersInGroup(ConversationGroup(conversationId))
            .SendAsync(ChatEvents.StopTyping, new
            {
                ConversationId = conversationId,
                UserId = GetUserId(),
                ClientId = GetClientId()
            });
    }

    // ── Leitura ──────────────────────────────────────────────────────────────

    public async Task MarkAsRead(long conversationId, long lastMessageId)
    {
        await Clients.OthersInGroup(ConversationGroup(conversationId))
            .SendAsync(ChatEvents.MessageRead, new
            {
                ConversationId = conversationId,
                UserId = GetUserId(),
                ClientId = GetClientId(),
                LastMessageId = lastMessageId
            });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Retorna userId se for atendente (JWT padrão)</summary>
    private int? GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? Context.User?.FindFirst("sub")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }

    /// <summary>Retorna clientId se for cliente externo (client_token)</summary>
    private long? GetClientId()
    {
        var clientToken = Context.GetHttpContext()?.Request.Query["client_token"].ToString();
        if (string.IsNullOrEmpty(clientToken)) return null;
        return _jwtService.ValidateClientToken(clientToken);
    }

    private static string ConversationGroup(long conversationId) =>
        $"conversation:{conversationId}";
}
