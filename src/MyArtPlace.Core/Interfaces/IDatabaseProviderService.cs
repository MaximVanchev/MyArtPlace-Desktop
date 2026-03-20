using MyArtPlace.Core.Enums;

namespace MyArtPlace.Core.Interfaces;

public interface IDatabaseProviderService
{
    DatabaseProvider CurrentProvider { get; }
    void SwitchProvider(DatabaseProvider provider);
    string GetConnectionString();
}
