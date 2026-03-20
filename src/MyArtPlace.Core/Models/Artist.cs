using System.ComponentModel.DataAnnotations;

namespace MyArtPlace.Core.Models;

public class Artist
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(1000)]
    public string? Biography { get; set; }

    public int? BirthYear { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
}
