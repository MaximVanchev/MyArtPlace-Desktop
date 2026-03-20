using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MyArtPlace.Core.Enums;
using MyArtPlace.Core.Interfaces;
using MyArtPlace.Core.Models;
using MyArtPlace.Data;
using MyArtPlace.Data.Repositories;
using MyArtPlace.ViewModels;
using MyArtPlace.ViewModels.Reusable;

namespace MyArtPlace.Avalonia;

public partial class App : Application
{
    // Simple service locator for the desktop app
    public static IDatabaseProviderService DbProviderService { get; private set; } = null!;
    public static DbContextFactory DbFactory { get; private set; } = null!;
    public static IRepository<ArtPiece> ArtRepo { get; private set; } = null!;
    public static IRepository<Artist> ArtistRepo { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        // Configure services (same connection strings as Blazor)
        const string postgresConn = "Host=localhost;Port=5432;Database=MyArtPlaceDb;Username=postgres;Password=MyArtPlace123";
        const string mysqlConn = "Server=localhost;Port=3306;Database=MyArtPlaceDb;User=root;Password=MyArtPlace123";

        DbProviderService = new DatabaseProviderService(postgresConn, mysqlConn, DatabaseProvider.PostgreSQL);
        DbFactory = new DbContextFactory(DbProviderService);
        ArtRepo = new Repository<ArtPiece>(DbFactory);
        ArtistRepo = new Repository<Artist>(DbFactory);

        // Initialize both databases
        await DatabaseInitializer.InitializeAsync(DbFactory, DbProviderService);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}