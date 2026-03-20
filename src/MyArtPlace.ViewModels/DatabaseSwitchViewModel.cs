using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyArtPlace.Core.Enums;
using MyArtPlace.Core.Interfaces;

namespace MyArtPlace.ViewModels;

public partial class DatabaseSwitchViewModel : ObservableObject
{
    private readonly IDatabaseProviderService _providerService;

    [ObservableProperty]
    private DatabaseProvider _currentProvider;

    [ObservableProperty]
    private string? _statusMessage;

    public DatabaseProvider[] AvailableProviders => Enum.GetValues<DatabaseProvider>();

    public DatabaseSwitchViewModel(IDatabaseProviderService providerService)
    {
        _providerService = providerService;
        _currentProvider = providerService.CurrentProvider;
    }

    [RelayCommand]
    public void SwitchProvider(DatabaseProvider provider)
    {
        _providerService.SwitchProvider(provider);
        CurrentProvider = provider;
        StatusMessage = $"Switched to {provider}";
    }
}
