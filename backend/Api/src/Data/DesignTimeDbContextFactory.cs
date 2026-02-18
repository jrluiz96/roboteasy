using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Api.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Connection string apenas para migrations em design-time
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=roboteasy;Username=roboteasy;Password=roboteasy123");

        return new AppDbContext(optionsBuilder.Options);
    }
}
