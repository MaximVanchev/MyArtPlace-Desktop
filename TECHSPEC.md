# MyArtPlace — Technical Specification

**Project**: MyArtPlace  
**Version**: 1.0  
**Date**: March 2026  
**Platform**: .NET 10.0  
**Architecture**: MVVM (Model–View–ViewModel)

---

## 1. Overview

MyArtPlace is a cross-platform art gallery management application built with C# and the MVVM design pattern. It provides full CRUD (Create, Read, Update, Delete) operations for art pieces and artists, a reusable filtering system, a reusable dynamic list component, and runtime database switching between PostgreSQL and MySQL.

The application ships with **two UI front-ends** that share a common ViewModel layer:

| UI Technology | Type | Framework | Port / Runtime |
|---|---|---|---|
| **Blazor Server** | Web application | ASP.NET Core 10 | `http://localhost:5200` |
| **AvaloniaUI** | Desktop application | Avalonia 11.3.12 | Native window (Windows/macOS/Linux) |

---

## 2. Solution Structure

```
MyArtPlace/
├── MyArtPlace.slnx                     # Solution file
└── src/
    ├── MyArtPlace.Core/                # Domain models, enums, interfaces
    ├── MyArtPlace.Data/                # EF Core data access, repositories
    ├── MyArtPlace.ViewModels/          # Shared MVVM ViewModels
    ├── MyArtPlace.Blazor/              # Blazor Server web UI
    └── MyArtPlace.Avalonia/            # AvaloniaUI desktop UI
```

### 2.1 Project Dependency Graph

```
MyArtPlace.Core          (no dependencies)
       ▲
       │
MyArtPlace.Data          → Core
       ▲
       │
MyArtPlace.ViewModels    → Core, Data
       ▲           ▲
       │           │
  Blazor UI    Avalonia UI   → Core, Data, ViewModels
```

---

## 3. Technology Stack

| Component | Technology | Version |
|---|---|---|
| Runtime | .NET | 10.0 |
| ORM | Entity Framework Core | 10.0.5 |
| PostgreSQL Provider | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.1 |
| MySQL Provider | MySql.EntityFrameworkCore (Oracle) | 10.0.1 |
| MVVM Toolkit | CommunityToolkit.Mvvm | 8.4.1 |
| Web UI | Blazor Server (Interactive Server rendering) | .NET 10 |
| Desktop UI | AvaloniaUI | 11.3.12 |
| Desktop DataGrid | Avalonia.Controls.DataGrid | 11.3.12 |
| Desktop Theme | Avalonia.Themes.Fluent | 11.3.12 |
| Validation | System.ComponentModel.Annotations | 5.0.0 |
| Databases | PostgreSQL + MySQL 8.0 (Docker) | Latest |

---

## 4. Layer Details

### 4.1 MyArtPlace.Core — Domain Layer

Contains models, enumerations, and interfaces shared by all other layers. This project has no external dependencies beyond `System.ComponentModel.Annotations`.

#### Models

**ArtPiece**
| Property | Type | Constraints |
|---|---|---|
| Id | `int` | Primary Key |
| Title | `string` | Required, MaxLength(200) |
| Description | `string?` | MaxLength(500) |
| Artist | `string` | Required, MaxLength(100) |
| Category | `ArtCategory` | Enum |
| YearCreated | `int?` | Nullable |
| ImageUrl | `string?` | MaxLength(500) |
| Price | `decimal?` | Nullable |
| DateAdded | `DateTime` | Default: `DateTime.UtcNow` |

**Artist**
| Property | Type | Constraints |
|---|---|---|
| Id | `int` | Primary Key |
| Name | `string` | Required, MaxLength(100) |
| Country | `string?` | MaxLength(100) |
| Biography | `string?` | MaxLength(1000) |
| BirthYear | `int?` | Nullable |
| Email | `string?` | MaxLength(200) |
| DateRegistered | `DateTime` | Default: `DateTime.UtcNow` |

#### Enumerations

- **ArtCategory**: `Painting`, `Sculpture`, `Photography`, `DigitalArt`, `Drawing`, `Printmaking`, `MixedMedia`, `Other`
- **DatabaseProvider**: `PostgreSQL`, `MySQL`

#### Interfaces

- **`IRepository<T>`** — Generic repository: `GetAllAsync`, `GetByIdAsync`, `FindAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`
- **`IDatabaseProviderService`** — Database switching: `CurrentProvider`, `SwitchProvider(provider)`, `GetConnectionString()`

---

### 4.2 MyArtPlace.Data — Data Access Layer

Implements Entity Framework Core persistence with support for PostgreSQL and MySQL.

| Class | Responsibility |
|---|---|
| `AppDbContext` | EF Core DbContext with `DbSet<ArtPiece>` and `DbSet<Artist>`. Contains `OnModelCreating` seed data (3 artists, 5 art pieces). |
| `DbContextFactory` | Creates `AppDbContext` instances using the currently selected provider (UseNpgsql or UseMySQL). |
| `DatabaseProviderService` | Implements `IDatabaseProviderService`. Stores connection strings for both providers. Enables runtime switching. |
| `DatabaseInitializer` | Calls `EnsureCreatedAsync()` on both databases at startup. Seeds initial data into both providers. |
| `Repository<T>` | Generic EF Core repository. Uses `DbContextFactory` for per-operation context creation. Read operations use `AsNoTracking()`. |

#### Seed Data

**Artists**: Leonardo da Vinci (Italy, 1452), Vincent van Gogh (Netherlands, 1853), Frida Kahlo (Mexico, 1907)

**Art Pieces**: Mona Lisa, Starry Night, The Two Fridas, Self-Portrait with Thorn Necklace, Sunflowers

---

### 4.3 MyArtPlace.ViewModels — Shared MVVM Logic

All ViewModels inherit from `ObservableObject` (CommunityToolkit.Mvvm) and use `[ObservableProperty]` and `[RelayCommand]` attributes for data binding and command generation.

| ViewModel | Purpose | Key Properties | Key Commands |
|---|---|---|---|
| `ArtGalleryViewModel` | Gallery listing & deletion | `ArtPieces`, `SelectedArtPiece`, `IsLoading`, `ErrorMessage` | `LoadArtAsync`, `DeleteArtAsync` |
| `ArtEditorViewModel` | Create/edit art form | `Title`, `Artist`, `Category`, `YearCreated`, `Description`, `ImageUrl`, `Price`, `IsEditing` | `SaveAsync` |
| `ArtistListViewModel` | Artist listing & deletion | `Artists`, `SelectedArtist`, `IsLoading`, `ErrorMessage` | `LoadArtistsAsync`, `DeleteArtistAsync` |
| `DatabaseSwitchViewModel` | Provider switching | `CurrentProvider`, `StatusMessage`, `AvailableProviders` | `SwitchProvider` |

#### Reusable Components

**FilterBuilder** (`Reusable/FilterBuilder.cs`)

A reflection-based filter generation system that satisfies the reusable filter controls requirement.

- `BuildFilters<T>(params string[] propertyNames)` — Generates `FilterField[]` from model properties. Auto-detects property types (string, numeric, enum), nullable wrappers, and enum values.
- `ApplyFilters<T>(IEnumerable<T> source)` — Applies active filters to a collection:
  - **String**: Case-insensitive `Contains`
  - **Numeric**: Exact `Equals`
  - **Enum**: Name-based `Equals`

**DynamicListViewModel** (`Reusable/DynamicListViewModel.cs`)

A reflection-based dynamic table system that satisfies the reusable dynamic list requirement.

- `SetData<T>(IEnumerable<T> items, params string[] columnNames)` — Populates the list with items and specified columns.
- `ChangeColumns<T>(params string[] newColumns)` — Dynamically changes which columns are visible without reloading data.
- `GetSelectedObject<T>()` — Returns the full typed object from the selected row.
- `Clear()` — Resets columns, rows, and selection.

Internal classes:
- `ColumnDefinition`: `PropertyName`, `Header`
- `DynamicRow`: `FullObject` (original entity), `DisplayValues` (string list for current columns)

---

### 4.4 MyArtPlace.Blazor — Web UI

A Blazor Server application with Interactive Server rendering mode.

**Dependency Injection (Program.cs)**:
- `IDatabaseProviderService` → Singleton
- `DbContextFactory` → Singleton
- `IRepository<ArtPiece>`, `IRepository<Artist>` → Transient
- All ViewModels → Transient

#### Pages

| Page | Route | Description |
|---|---|---|
| Home (Gallery) | `/` | Art card grid with filter panel, delete buttons, navigation to editor |
| ArtEditor | `/add`, `/edit/{Id:int}` | Create/edit form with validation, delete (when editing) |
| Artists | `/artists` | Artist table with filter panel |
| DynamicListDemo | `/dynamic-list` | Dynamic list demo: load, switch columns, select, clear |
| Settings | `/settings` | Database provider switching UI |
| Error | (internal) | Error page with request ID |
| NotFound | (fallback) | 404 handler |

#### Shared Components

| Component | Parameters | Purpose |
|---|---|---|
| `FilterPanel` | `FilterField[]`, `OnFiltersChanged` callback | Renders type-aware filter inputs (text/number/dropdown). Reusable across any entity. |
| `DynamicListPanel` | `DynamicListViewModel`, `OnRowSelected` callback | Renders a dynamic table from ViewModel columns/rows. Row selection support. |
| `MainLayout` | — | Sidebar navigation + main content area |

#### Layout

- **Sidebar**: Fixed 240px dark panel (`#1a1a2e`) with navigation links
- **Content Area**: Flexible light panel with scrolling
- **Art Grid**: CSS Grid responsive cards

---

### 4.5 MyArtPlace.Avalonia — Desktop UI

A cross-platform desktop application using AvaloniaUI with the Fluent theme.

**Service Setup (App.axaml.cs)**: Static service locator pattern creating `DatabaseProviderService`, `DbContextFactory`, and repository instances.

#### Views

| View | Description |
|---|---|
| `MainWindow` | TabControl with 5 tabs, DockPanel layout, 1000×700px |
| `GalleryView` | DataGrid with programmatically-built filter controls (TextBox for string, ComboBox for enum) |
| `ArtEditorView` | Form with TextBox, ComboBox, TextArea inputs, save/clear buttons |
| `ArtistView` | DataGrid with filter controls for Name, Country, BirthYear |
| `DynamicListView` | Dynamic DataGrid, buttons for load/switch/clear, selection tracking |
| `DatabaseSettingsView` | Provider switch buttons with active highlighting |

All views use `x:CompileBindings="False"` to support dynamic DataGrid bindings.

---

## 5. Database Architecture

### 5.1 Providers

| Provider | Host | Port | Database | User | Container |
|---|---|---|---|---|---|
| PostgreSQL | localhost | 5432 | MyArtPlaceDb | postgres | PostgreContainer |
| MySQL 8.0 | localhost | 3306 | MyArtPlaceDb | root | MySqlContainer |

Both databases are managed via Docker Desktop.

### 5.2 Schema

Both databases contain identical schemas generated by EF Core `EnsureCreatedAsync()`:

**Table: ArtPieces**
| Column | Type | Nullable |
|---|---|---|
| Id | int (PK, auto-increment) | No |
| Title | varchar(200) | No |
| Description | varchar(500) | Yes |
| Artist | varchar(100) | No |
| Category | int (enum) | No |
| YearCreated | int | Yes |
| ImageUrl | varchar(500) | Yes |
| Price | decimal | Yes |
| DateAdded | datetime/timestamp | No |

**Table: Artists**
| Column | Type | Nullable |
|---|---|---|
| Id | int (PK, auto-increment) | No |
| Name | varchar(100) | No |
| Country | varchar(100) | Yes |
| Biography | varchar(1000) | Yes |
| BirthYear | int | Yes |
| Email | varchar(200) | Yes |
| DateRegistered | datetime/timestamp | No |

### 5.3 Runtime Switching

The application allows switching between PostgreSQL and MySQL at runtime via the Settings page:

1. User clicks a provider button (PostgreSQL / MySQL)
2. `DatabaseSwitchViewModel` calls `IDatabaseProviderService.SwitchProvider()`
3. `DatabaseProviderService` updates its `CurrentProvider` property
4. Subsequent repository operations use `DbContextFactory`, which reads the current provider to create the appropriate `DbContext`
5. No application restart required

---

## 6. Requirement Compliance Matrix

| # | Requirement | Implementation | Status |
|---|---|---|---|
| 1 | MVVM Pattern | CommunityToolkit.Mvvm: `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]` | ✅ |
| 2 | Two UI technologies sharing ViewModels | Blazor Server + AvaloniaUI, both consuming `MyArtPlace.ViewModels` | ✅ |
| 3 | Two DBMS with runtime switching | PostgreSQL + MySQL via `IDatabaseProviderService` and `DbContextFactory` | ✅ |
| 4a | Reusable filter: type-aware controls | `FilterBuilder.BuildFilters<T>()` detects string/numeric/enum → text/number/dropdown | ✅ |
| 4b | Reusable filter: auto labels | Property names auto-extracted via reflection | ✅ |
| 4c | Reusable filter: query generation | `FilterBuilder.ApplyFilters<T>()` with contains/equals logic | ✅ |
| 4d | Filter demonstrated on 2 tables | `ArtPiece` (Gallery page) + `Artist` (Artists page) | ✅ |
| 5a | Dynamic list: set data | `DynamicListViewModel.SetData<T>()` | ✅ |
| 5b | Dynamic list: specify columns | Column names passed as `params string[]` | ✅ |
| 5c | Dynamic list: change columns | `DynamicListViewModel.ChangeColumns<T>()` | ✅ |
| 5d | Dynamic list: return full object | `DynamicListViewModel.GetSelectedObject<T>()` | ✅ |
| 5e | Dynamic list: clear | `DynamicListViewModel.Clear()` | ✅ |
| 5f | Dynamic list on 2 tables | DynamicListDemo page loads ArtPieces and Artists | ✅ |

---

## 7. Design Patterns Used

| Pattern | Usage |
|---|---|
| **MVVM** | ViewModels with observable properties and commands, used by both Blazor and Avalonia |
| **Repository** | `IRepository<T>` / `Repository<T>` abstracts EF Core data access |
| **Factory** | `DbContextFactory` creates provider-specific DbContext instances |
| **Strategy** | `IDatabaseProviderService` switches between database strategies at runtime |
| **Dependency Injection** | Blazor uses ASP.NET Core DI; Avalonia uses static service locator |
| **Observer** | `ObservableObject` / `ObservableCollection<T>` for UI reactivity |

---

## 8. Data Flow

### 8.1 Read Operation (e.g., Load Gallery)

```
UI (Page/View)
  → ViewModel.LoadArtAsync()
    → IRepository<ArtPiece>.GetAllAsync()
      → DbContextFactory.CreateDbContext()
        → IDatabaseProviderService.GetConnectionString()
        → new AppDbContext(options)  [UseNpgsql or UseMySQL]
      → context.ArtPieces.AsNoTracking().ToListAsync()
    → ViewModel.ArtPieces = new ObservableCollection(results)
  → UI updates via data binding
```

### 8.2 Write Operation (e.g., Save Art)

```
UI (Form submit)
  → ViewModel.SaveAsync()
    → Validation (Title, Artist required)
    → IRepository<ArtPiece>.AddAsync(entity) or UpdateAsync(entity)
      → DbContextFactory.CreateDbContext()
      → context.Add/Update(entity)
      → context.SaveChangesAsync()
    → ViewModel.SuccessMessage = "Saved!"
  → UI updates via data binding
```

---

## 9. Application Configuration

### 9.1 Blazor — appsettings.json

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=MyArtPlaceDb;Username=postgres;Password=MyArtPlace123",
    "MySQL": "Server=localhost;Port=3306;Database=MyArtPlaceDb;User=root;Password=MyArtPlace123"
  }
}
```

### 9.2 Blazor — launchSettings.json

- **HTTP Profile**: `http://localhost:5200`
- **HTTPS Profile**: `https://localhost:7215`

### 9.3 Avalonia

Connection strings are hardcoded in `App.axaml.cs` with the same values.

---

## 10. Security Considerations

- Database credentials are stored in configuration files (appsettings.json) and code (Avalonia). In a production environment, these should be moved to environment variables or a secrets manager.
- Blazor Server uses ASP.NET Core's built-in antiforgery protection.
- EF Core parameterized queries prevent SQL injection.
- Input validation is enforced via `System.ComponentModel.Annotations` on models and ViewModel-level checks.

---

## 11. Known Limitations

1. **No authentication/authorization** — the application is an open management tool.
2. **Image URLs only** — images are referenced by URL, not uploaded.
3. **Artist is a string on ArtPiece** — not a foreign key relationship (simplified model).
4. **Avalonia uses static service locator** — no proper DI container (acceptable for desktop scope).
5. **EnsureCreated vs Migrations** — database schema uses `EnsureCreatedAsync()` instead of EF Core migrations.
