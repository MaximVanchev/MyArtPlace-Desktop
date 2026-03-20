using MyArtPlace.Blazor.Components;
using MyArtPlace.Core.Enums;
using MyArtPlace.Core.Interfaces;
using MyArtPlace.Core.Models;
using MyArtPlace.Data;
using MyArtPlace.Data.Repositories;
using MyArtPlace.ViewModels;
using MyArtPlace.ViewModels.Reusable;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Database provider service
var postgresConn = builder.Configuration.GetConnectionString("PostgreSQL")!;
var mysqlConn = builder.Configuration.GetConnectionString("MySQL")!;
var dbProviderService = new DatabaseProviderService(postgresConn, mysqlConn, DatabaseProvider.PostgreSQL);
builder.Services.AddSingleton<IDatabaseProviderService>(dbProviderService);
builder.Services.AddSingleton<DbContextFactory>();

// Repositories
builder.Services.AddTransient<IRepository<ArtPiece>, Repository<ArtPiece>>();
builder.Services.AddTransient<IRepository<Artist>, Repository<Artist>>();

// ViewModels (requirement 1: MVVM + requirement 2: shared VMs)
builder.Services.AddTransient<ArtGalleryViewModel>();
builder.Services.AddTransient<ArtEditorViewModel>();
builder.Services.AddTransient<ArtistListViewModel>();
builder.Services.AddTransient<DatabaseSwitchViewModel>();
builder.Services.AddTransient<DynamicListViewModel>();

var app = builder.Build();

// Initialize both databases with seed data
var factory = app.Services.GetRequiredService<DbContextFactory>();
var providerSvc = app.Services.GetRequiredService<IDatabaseProviderService>();
await DatabaseInitializer.InitializeAsync(factory, providerSvc);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
