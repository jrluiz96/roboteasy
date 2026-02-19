using Api.Contracts.Requests;
using Api.Contracts.Responses;
using Api.Models;
using Api.Repositories;

namespace Api.Services;

public class ChatService : IChatService
{
    private readonly IClientRepository _clientRepository;
    private readonly IJwtService _jwtService;

    public ChatService(IClientRepository clientRepository, IJwtService jwtService)
    {
        _clientRepository = clientRepository;
        _jwtService = jwtService;
    }

    public async Task<ChatStartResponse> StartAsync(ChatStartRequest request)
    {
        // Busca cliente pelo email ou cria um novo
        var client = await _clientRepository.GetByEmailAsync(request.Email);

        if (client == null)
        {
            client = await _clientRepository.CreateAsync(new Client
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            // Atualiza nome e telefone caso tenha mudado
            client.Name = request.Name;
            if (request.Phone != null) client.Phone = request.Phone;
        }

        var clientToken = _jwtService.GenerateClientToken(client.Id);

        return new ChatStartResponse(client.Id, clientToken);
    }
}
