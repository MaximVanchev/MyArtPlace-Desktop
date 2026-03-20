using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyArtPlace.Core.Interfaces;
using MyArtPlace.Core.Models;
using MyArtPlace.Core.Enums;

namespace MyArtPlace.ViewModels;

public partial class ArtEditorViewModel : ObservableObject
{
    private readonly IRepository<ArtPiece> _artRepo;

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string _artist = string.Empty;

    [ObservableProperty]
    private ArtCategory _category;

    [ObservableProperty]
    private int? _yearCreated;

    [ObservableProperty]
    private string? _imageUrl;

    [ObservableProperty]
    private decimal? _price;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    public ArtCategory[] Categories => Enum.GetValues<ArtCategory>();

    public ArtEditorViewModel(IRepository<ArtPiece> artRepo)
    {
        _artRepo = artRepo;
    }

    public void LoadForEdit(ArtPiece art)
    {
        Id = art.Id;
        Title = art.Title;
        Description = art.Description;
        Artist = art.Artist;
        Category = art.Category;
        YearCreated = art.YearCreated;
        ImageUrl = art.ImageUrl;
        Price = art.Price;
        IsEditing = true;
        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void LoadForCreate()
    {
        Id = 0;
        Title = string.Empty;
        Description = null;
        Artist = string.Empty;
        Category = ArtCategory.Painting;
        YearCreated = null;
        ImageUrl = null;
        Price = null;
        IsEditing = false;
        ErrorMessage = null;
        SuccessMessage = null;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        try
        {
            ErrorMessage = null;
            SuccessMessage = null;

            if (string.IsNullOrWhiteSpace(Title))
            {
                ErrorMessage = "Title is required.";
                return;
            }
            if (string.IsNullOrWhiteSpace(Artist))
            {
                ErrorMessage = "Artist is required.";
                return;
            }

            var art = new ArtPiece
            {
                Id = Id,
                Title = Title.Trim(),
                Description = Description?.Trim(),
                Artist = Artist.Trim(),
                Category = Category,
                YearCreated = YearCreated,
                ImageUrl = ImageUrl?.Trim(),
                Price = Price,
                DateAdded = IsEditing ? DateTime.UtcNow : DateTime.UtcNow
            };

            if (IsEditing)
            {
                await _artRepo.UpdateAsync(art);
                SuccessMessage = "Art piece updated successfully!";
            }
            else
            {
                await _artRepo.AddAsync(art);
                SuccessMessage = "Art piece added successfully!";
                Id = art.Id;
                IsEditing = true;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Save failed: {ex.Message}";
        }
    }
}
