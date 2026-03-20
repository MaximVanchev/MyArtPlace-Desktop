using Microsoft.EntityFrameworkCore;
using MyArtPlace.Core.Enums;
using MyArtPlace.Core.Interfaces;

namespace MyArtPlace.Data;

public class DbContextFactory
{
    private readonly IDatabaseProviderService _providerService;

    public DbContextFactory(IDatabaseProviderService providerService)
    {
        _providerService = providerService;
    }

    public AppDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = _providerService.GetConnectionString();

        switch (_providerService.CurrentProvider)
        {
            case DatabaseProvider.PostgreSQL:
                optionsBuilder.UseNpgsql(connectionString);
                break;
            case DatabaseProvider.MySQL:
                optionsBuilder.UseMySQL(connectionString);
                break;
        }

        return new AppDbContext(optionsBuilder.Options);
    }
}
