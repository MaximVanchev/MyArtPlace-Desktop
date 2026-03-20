using System.ComponentModel.DataAnnotations;

namespace MyArtPlace.Core.Models;

public class ArtPiece
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string Artist { get; set; } = string.Empty;

    public ArtCategory Category { get; set; }

    public int? YearCreated { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public decimal? Price { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}
