using Api.Models;

namespace Api.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByEmailAsync(string email);
    Task<Client?> GetByIdAsync(long id);
    Task<List<Client>> GetAllAsync();
    Task<Client> CreateAsync(Client client);
    Task<Client> UpdateAsync(Client client);
    Task<Client> UpdateWsConnAsync(long id, string? connectionId);
}
