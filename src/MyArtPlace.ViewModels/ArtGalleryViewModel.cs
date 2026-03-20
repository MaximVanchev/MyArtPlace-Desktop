using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyArtPlace.Core.Interfaces;
using MyArtPlace.Core.Models;
using MyArtPlace.Core.Enums;

namespace MyArtPlace.ViewModels;

public partial class ArtGalleryViewModel : ObservableObject
{
    private readonly IRepository<ArtPiece> _artRepo;

    [ObservableProperty]
    private ObservableCollection<ArtPiece> _artPieces = new();

    [ObservableProperty]
    private ArtPiece? _selectedArtPiece;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public ArtGalleryViewModel(IRepository<ArtPiece> artRepo)
    {
        _artRepo = artRepo;
    }

    [RelayCommand]
    public async Task LoadArtAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            var items = await _artRepo.GetAllAsync();
            ArtPieces = new ObservableCollection<ArtPiece>(items);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load art: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteArtAsync(ArtPiece art)
    {
        try
        {
            await _artRepo.DeleteAsync(art.Id);
            ArtPieces.Remove(art);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete: {ex.Message}";
        }
    }

    public void SelectArt(ArtPiece art)
    {
        SelectedArtPiece = art;
    }
}
