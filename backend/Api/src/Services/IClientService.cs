using Api.Contracts.Responses;

namespace Api.Services;

public interface IClientService
{
    Task<IEnumerable<ClientResponse>> GetAllAsync();
}
