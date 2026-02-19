using Api.Models;

namespace Api.Repositories;

public interface IPermissionViewRepository
{
    Task<IEnumerable<View>> GetViewsByPermissionIdAsync(int permissionId);
}