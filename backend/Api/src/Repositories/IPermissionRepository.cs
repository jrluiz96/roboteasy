using Api.Models;

namespace Api.Repositories;

public interface IPermissionRepository
{
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<Permission?> GetByIdAsync(int id);
    Task<Permission?> GetByNameAsync(string name);
}