using Api.Contracts.Requests;
using Api.Contracts.Responses;
using Api.Hubs;
using Api.Models;
using Api.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace Api.Services;

public class ChatService : IChatService
{
    private readonly IClientRepository _clientRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IJwtService _jwtService;
    private readonly IHubContext<ChatHub> _hub;

    public ChatService(IClientRepository clientRepository, IConversationRepository conversationRepository, IJwtService jwtService, IHubContext<ChatHub> hub)
    {
        _clientRepository = clientRepository;
        _conversationRepository = conversationRepository;
        _jwtService = jwtService;
        _hub = hub;
    }

    public async Task<ChatStartResponse> StartAsync(ChatStartRequest request)
    {
        // Busca cliente pelo email (se informado) ou cria um novo
        Client? client = null;
        if (!string.IsNullOrWhiteSpace(request.Email))
            client = await _clientRepository.GetByEmailAsync(request.Email);

        if (client == null)
        {
            client = await _clientRepository.CreateAsync(new Client
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Cpf = request.Cpf,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            client.Name = request.Name;
            if (request.Phone != null) client.Phone = request.Phone;
            if (request.Cpf != null) client.Cpf = request.Cpf;
            await _clientRepository.UpdateAsync(client);
        }

        // Retoma conversa aberta ou cria uma nova
        var existingConversation = await _conversationRepository.GetOpenByClientIdAsync(client.Id);
        var isNew = existingConversation == null;

        var conversation = existingConversation ?? await _conversationRepository.CreateAsync(new Conversation
        {
            ClientId = client.Id,
            CreatedAt = DateTime.UtcNow
        });

        // Notifica todos os atendentes online sobre a nova conversa
        if (isNew)
        {
            await _hub.Clients.Group("attendants")
                .SendAsync(ChatEvents.ConversationCreated, new
                {
                    id          = conversation.Id,
                    clientId    = client.Id,
                    clientName  = client.Name,
                    clientEmail = client.Email,
                    createdAt   = conversation.CreatedAt,
                    status      = "waiting"
                });
        }

        var messages = conversation.Messages
            .Select(m => new MessageResponse(m.Id, m.ConversationId, m.UserId, m.ClientId, m.Type, m.Content, m.FileUrl, m.CreatedAt))
            .ToList()
            .AsReadOnly();

        var clientToken = _jwtService.GenerateClientToken(client.Id);

        return new ChatStartResponse(client.Id, clientToken, conversation.Id, isNew, messages);
    }
}
