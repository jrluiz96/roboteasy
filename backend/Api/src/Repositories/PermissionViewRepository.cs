using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class PermissionViewRepository : IPermissionViewRepository
{
    private readonly AppDbContext _context;

    public PermissionViewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<View>> GetViewsByPermissionIdAsync(int permissionId)
    {
        return await _context.PermissionViews
            .Include(pv => pv.View)
            .Where(pv => pv.PermissionId == permissionId)
            .Select(pv => pv.View)
            .OrderBy(v => v.Name)
            .ToListAsync();
    }
}