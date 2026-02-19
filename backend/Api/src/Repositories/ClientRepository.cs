using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByEmailAsync(string email)
    {
        return await _context.Clients
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<Client?> GetByIdAsync(long id)
    {
        return await _context.Clients.FindAsync(id);
    }

    public async Task<Client> CreateAsync(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<Client> UpdateAsync(Client client)
    {
        client.UpdatedAt = DateTime.UtcNow;
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<Client> UpdateWsConnAsync(long id, string? connectionId)
    {
        var client = await _context.Clients.FindAsync(id)
            ?? throw new InvalidOperationException($"Client {id} not found");

        client.WsConn = connectionId;
        client.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return client;
    }
}
