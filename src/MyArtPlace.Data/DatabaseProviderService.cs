using Microsoft.EntityFrameworkCore;
using MyArtPlace.Core.Enums;
using MyArtPlace.Core.Interfaces;

namespace MyArtPlace.Data;

public class DatabaseProviderService : IDatabaseProviderService
{
    private DatabaseProvider _currentProvider;
    private readonly string _postgresConnection;
    private readonly string _mysqlConnection;

    public DatabaseProviderService(string postgresConnection, string mysqlConnection, DatabaseProvider defaultProvider = DatabaseProvider.PostgreSQL)
    {
        _postgresConnection = postgresConnection;
        _mysqlConnection = mysqlConnection;
        _currentProvider = defaultProvider;
    }

    public DatabaseProvider CurrentProvider => _currentProvider;

    public void SwitchProvider(DatabaseProvider provider) => _currentProvider = provider;

    public string GetConnectionString() => _currentProvider switch
    {
        DatabaseProvider.PostgreSQL => _postgresConnection,
        DatabaseProvider.MySQL => _mysqlConnection,
        _ => throw new ArgumentOutOfRangeException()
    };
}
