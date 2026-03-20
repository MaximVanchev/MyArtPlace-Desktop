using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyArtPlace.Core.Interfaces;
using MyArtPlace.Core.Models;
using MyArtPlace.Core.Enums;

namespace MyArtPlace.ViewModels;

public partial class ArtistListViewModel : ObservableObject
{
    private readonly IRepository<Artist> _artistRepo;

    [ObservableProperty]
    private ObservableCollection<Artist> _artists = new();

    [ObservableProperty]
    private Artist? _selectedArtist;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public ArtistListViewModel(IRepository<Artist> artistRepo)
    {
        _artistRepo = artistRepo;
    }

    [RelayCommand]
    public async Task LoadArtistsAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            var items = await _artistRepo.GetAllAsync();
            Artists = new ObservableCollection<Artist>(items);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load artists: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task DeleteArtistAsync(Artist artist)
    {
        try
        {
            await _artistRepo.DeleteAsync(artist.Id);
            Artists.Remove(artist);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to delete: {ex.Message}";
        }
    }

    public void SelectArtist(Artist artist)
    {
        SelectedArtist = artist;
    }
}
