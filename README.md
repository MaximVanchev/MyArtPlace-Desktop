# MyArtPlace

A cross-platform art gallery management application built with C# and the **MVVM** pattern. Features a **Blazor Server** web interface and an **AvaloniaUI** desktop interface, both sharing the same ViewModels. Supports **PostgreSQL** and **MySQL** with runtime database switching.

---

## Features

- **CRUD operations** for Art Pieces and Artists
- **Two UI front-ends**: Blazor Server (web) + AvaloniaUI (desktop)
- **Shared ViewModels** across both UIs (MVVM architecture)
- **Dual database support**: PostgreSQL and MySQL with runtime switching
- **Reusable filter system**: Type-aware filters (text/number/dropdown) auto-generated from model properties
- **Reusable dynamic list**: Load any entity, switch columns, select rows, retrieve full objects
- **Mobile responsive web UI**: Hamburger menu, collapsible sidebar, adaptive card grid, touch-friendly controls
- **Seed data**: Pre-loaded with famous artists and artworks

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for databases)

---

## Database Setup

The application uses two databases running in Docker containers.

### 1. Start PostgreSQL

```bash
docker run -d \
  --name PostgreContainer \
  -e POSTGRES_PASSWORD=MyArtPlace123 \
  -p 5432:5432 \
  postgres
```

Create the database:

```bash
docker exec -it PostgreContainer psql -U postgres -c "CREATE DATABASE \"MyArtPlaceDb\";"
```

### 2. Start MySQL

```bash
docker run -d \
  --name MySqlContainer \
  -e MYSQL_ROOT_PASSWORD=MyArtPlace123 \
  -p 3306:3306 \
  mysql:8.0
```

Create the database (wait ~15 seconds for MySQL to initialize):

```bash
docker exec -it MySqlContainer mysql -uroot -pMyArtPlace123 -e "CREATE DATABASE MyArtPlaceDb;"
```

### 3. Verify containers are running

```bash
docker container ls
```

You should see both `PostgreContainer` and `MySqlContainer` running.

---

## Build & Run

### Clone and build

```bash
cd MyArtPlace
dotnet build
```

### Run the Blazor web app

```bash
cd src/MyArtPlace.Blazor
dotnet run
```

Open **http://localhost:5200** in your browser.

### Run the Avalonia desktop app

```bash
cd src/MyArtPlace.Avalonia
dotnet run
```

A native desktop window will open.

> **Note**: Both apps initialize and seed both databases automatically on startup.

---

## Project Structure

```
MyArtPlace/
├── MyArtPlace.slnx
├── README.md
├── TECHSPEC.md
└── src/
    ├── MyArtPlace.Core/            # Domain models, enums, interfaces
    │   ├── Models/
    │   │   ├── ArtPiece.cs
    │   │   └── Artist.cs
    │   ├── Enums/
    │   │   ├── ArtCategory.cs
    │   │   └── DatabaseProvider.cs
    │   └── Interfaces/
    │       ├── IRepository.cs
    │       └── IDatabaseProviderService.cs
    │
    ├── MyArtPlace.Data/            # EF Core data access
    │   ├── AppDbContext.cs
    │   ├── DbContextFactory.cs
    │   ├── DatabaseProviderService.cs
    │   ├── DatabaseInitializer.cs
    │   └── Repositories/
    │       └── Repository.cs
    │
    ├── MyArtPlace.ViewModels/      # Shared MVVM ViewModels
    │   ├── ArtGalleryViewModel.cs
    │   ├── ArtEditorViewModel.cs
    │   ├── ArtistListViewModel.cs
    │   ├── DatabaseSwitchViewModel.cs
    │   └── Reusable/
    │       ├── FilterBuilder.cs
    │       └── DynamicListViewModel.cs
    │
    ├── MyArtPlace.Blazor/          # Web UI (Blazor Server)
    │   ├── Program.cs
    │   ├── appsettings.json
    │   ├── Components/
    │   │   ├── Pages/
    │   │   │   ├── Home.razor          (Gallery)
    │   │   │   ├── ArtEditor.razor     (Add/Edit)
    │   │   │   ├── Artists.razor       (Artist list)
    │   │   │   ├── DynamicListDemo.razor
    │   │   │   └── Settings.razor      (DB switching)
    │   │   ├── Shared/
    │   │   │   ├── FilterPanel.razor
    │   │   │   └── DynamicListPanel.razor
    │   │   └── Layout/
    │   │       └── MainLayout.razor
    │   └── wwwroot/
    │       └── app.css
    │
    └── MyArtPlace.Avalonia/        # Desktop UI (AvaloniaUI)
        ├── Program.cs
        ├── App.axaml / App.axaml.cs
        ├── MainWindow.axaml
        └── Views/
            ├── GalleryView.axaml
            ├── ArtEditorView.axaml
            ├── ArtistView.axaml
            ├── DynamicListView.axaml
            └── DatabaseSettingsView.axaml
```

---

## Architecture

### MVVM Pattern

```
┌─────────────┐     ┌──────────────────┐     ┌─────────────┐
│   Blazor    │────▶│   ViewModels     │◀────│  Avalonia   │
│  (Web UI)   │     │  (Shared logic)  │     │ (Desktop UI)│
└─────────────┘     └────────┬─────────┘     └─────────────┘
                             │
                    ┌────────▼─────────┐
                    │   Repository<T>  │
                    │   (Data layer)   │
                    └────────┬─────────┘
                             │
                    ┌────────▼─────────┐
                    │  DbContextFactory │
                    └───┬──────────┬───┘
                        │          │
               ┌────────▼──┐  ┌───▼────────┐
               │ PostgreSQL │  │   MySQL    │
               │  (Docker)  │  │  (Docker)  │
               └────────────┘  └────────────┘
```

### Design Patterns

| Pattern | Where |
|---|---|
| MVVM | ViewModels with ObservableObject, RelayCommand |
| Repository | `IRepository<T>` / `Repository<T>` |
| Factory | `DbContextFactory` for provider-specific DbContext |
| Strategy | `IDatabaseProviderService` for runtime DB switching |
| Dependency Injection | ASP.NET Core DI (Blazor), static locator (Avalonia) |

---

## Pages / Views

| Page | Route | Description |
|---|---|---|
| **Gallery** | `/` | Art pieces displayed as cards with filters and delete |
| **Add/Edit Art** | `/add`, `/edit/{id}` | Form to create or update an art piece |
| **Artists** | `/artists` | Artist table with filters |
| **Dynamic List** | `/dynamic-list` | Demo of the reusable dynamic list component |
| **Settings** | `/settings` | Switch between PostgreSQL and MySQL |

---

## Reusable Components

### FilterBuilder

Generates type-aware filter controls from any model class:

```csharp
// Generate filters for specific properties
var filters = FilterBuilder.BuildFilters<ArtPiece>("Title", "Artist", "Category", "YearCreated");

// Apply active filters to a collection
var filtered = FilterBuilder.ApplyFilters(allArt, filters);
```

- **String** properties → text input (case-insensitive contains)
- **Numeric** properties → number input (exact match)
- **Enum** properties → dropdown with all values

### DynamicListViewModel

A data-agnostic table that can display any entity:

```csharp
var vm = new DynamicListViewModel();

// Load data with specific columns
vm.SetData(artPieces, "Title", "Artist", "Price");

// Change visible columns without reloading
vm.ChangeColumns<ArtPiece>("Title", "Category", "YearCreated");

// Get the full object from a selected row
var selected = vm.GetSelectedObject<ArtPiece>();

// Reset everything
vm.Clear();
```

---

## Database Configuration

Connection strings in `src/MyArtPlace.Blazor/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=MyArtPlaceDb;Username=postgres;Password=MyArtPlace123",
    "MySQL": "Server=localhost;Port=3306;Database=MyArtPlaceDb;User=root;Password=MyArtPlace123"
  }
}
```

Both databases are auto-initialized with seed data on first run.

---

## Tech Stack

| Component | Technology |
|---|---|
| Language | C# 13 / .NET 10 |
| ORM | Entity Framework Core 10 |
| Web UI | Blazor Server |
| Desktop UI | AvaloniaUI 11.3 |
| MVVM | CommunityToolkit.Mvvm |
| PostgreSQL | Npgsql provider |
| MySQL | Oracle MySql.EntityFrameworkCore |
| Databases | Docker Desktop |

---

## Seed Data

The application starts with pre-loaded data:

**Artists**: Leonardo da Vinci, Vincent van Gogh, Frida Kahlo

**Art Pieces**: Mona Lisa, Starry Night, The Two Fridas, Self-Portrait with Thorn Necklace, Sunflowers

---

## License

This project was created as a university assignment (Technical University — Software Engineering, 2026).
