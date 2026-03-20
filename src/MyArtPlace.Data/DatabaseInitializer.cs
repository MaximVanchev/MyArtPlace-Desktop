using Microsoft.EntityFrameworkCore;
using MyArtPlace.Core.Enums;
using MyArtPlace.Core.Interfaces;

namespace MyArtPlace.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(DbContextFactory factory, IDatabaseProviderService providerService)
    {
        // Initialize both databases
        var original = providerService.CurrentProvider;

        foreach (DatabaseProvider provider in Enum.GetValues<DatabaseProvider>())
        {
            providerService.SwitchProvider(provider);
            using var db = factory.CreateDbContext();
            await db.Database.EnsureCreatedAsync();
        }

        providerService.SwitchProvider(original);
    }
}
