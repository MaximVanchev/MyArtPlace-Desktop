using Microsoft.EntityFrameworkCore;
using MyArtPlace.Core.Models;

namespace MyArtPlace.Data;

public class AppDbContext : DbContext
{
    public DbSet<ArtPiece> ArtPieces => Set<ArtPiece>();
    public DbSet<Artist> Artists => Set<Artist>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ArtPiece>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Artist).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImageContentType).HasMaxLength(100);
        });

        modelBuilder.Entity<Artist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });

        // Seed data
        modelBuilder.Entity<Artist>().HasData(
            new Artist { Id = 1, Name = "Leonardo da Vinci", Country = "Italy", BirthYear = 1452, Biography = "Italian polymath of the Renaissance." },
            new Artist { Id = 2, Name = "Vincent van Gogh", Country = "Netherlands", BirthYear = 1853, Biography = "Dutch Post-Impressionist painter." },
            new Artist { Id = 3, Name = "Frida Kahlo", Country = "Mexico", BirthYear = 1907, Biography = "Mexican painter known for self-portraits." }
        );

        modelBuilder.Entity<ArtPiece>().HasData(
            new ArtPiece { Id = 1, Title = "Mona Lisa", Artist = "Leonardo da Vinci", Category = ArtCategory.Painting, YearCreated = 1503, Description = "Half-length portrait painting.", Price = 850000000m, DateAdded = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ArtPiece { Id = 2, Title = "Starry Night", Artist = "Vincent van Gogh", Category = ArtCategory.Painting, YearCreated = 1889, Description = "Depicts the view from his asylum room at night.", Price = 100000000m, DateAdded = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ArtPiece { Id = 3, Title = "The Two Fridas", Artist = "Frida Kahlo", Category = ArtCategory.Painting, YearCreated = 1939, Description = "Double self-portrait.", Price = 35000000m, DateAdded = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ArtPiece { Id = 4, Title = "Self-Portrait with Thorn Necklace", Artist = "Frida Kahlo", Category = ArtCategory.Painting, YearCreated = 1940, Description = "Oil on canvas self-portrait.", Price = 25000000m, DateAdded = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new ArtPiece { Id = 5, Title = "Sunflowers", Artist = "Vincent van Gogh", Category = ArtCategory.Painting, YearCreated = 1888, Description = "Series of still life paintings.", Price = 39000000m, DateAdded = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
