using Api.Contracts.Responses;
using Api.Repositories;

namespace Api.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClientResponse>> GetAllAsync()
    {
        var clients = await _repository.GetAllAsync();

        return clients.Select(c => new ClientResponse(
            c.Id,
            c.Name,
            c.Email,
            c.Phone,
            c.Cpf,
            c.Conversations.Count,
            c.Messages.Count,
            c.Conversations
                .OrderByDescending(conv => conv.CreatedAt)
                .Select(conv => (DateTime?)conv.CreatedAt)
                .FirstOrDefault(),
            c.WsConn != null,
            c.CreatedAt
        ));
    }
}
