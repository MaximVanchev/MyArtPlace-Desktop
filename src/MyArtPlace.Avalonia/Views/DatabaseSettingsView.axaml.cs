using Avalonia.Controls;
using Avalonia.Interactivity;
using MyArtPlace.Core.Enums;
using MyArtPlace.ViewModels;

namespace MyArtPlace.Avalonia.Views;

public partial class DatabaseSettingsView : UserControl
{
    private readonly DatabaseSwitchViewModel _vm;

    public DatabaseSettingsView()
    {
        InitializeComponent();
        _vm = new DatabaseSwitchViewModel(App.DbProviderService);
        UpdateDisplay();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        BtnPostgres.Click += (_, _) => SwitchTo(DatabaseProvider.PostgreSQL);
        BtnMySql.Click += (_, _) => SwitchTo(DatabaseProvider.MySQL);
    }

    private void SwitchTo(DatabaseProvider provider)
    {
        _vm.SwitchProviderCommand.Execute(provider);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        TxtCurrentDb.Text = _vm.CurrentProvider.ToString();
        BtnPostgres.Background = _vm.CurrentProvider == DatabaseProvider.PostgreSQL
            ? global::Avalonia.Media.Brushes.SteelBlue
            : global::Avalonia.Media.Brushes.Gray;
        BtnPostgres.Foreground = global::Avalonia.Media.Brushes.White;
        BtnMySql.Background = _vm.CurrentProvider == DatabaseProvider.MySQL
            ? global::Avalonia.Media.Brushes.SteelBlue
            : global::Avalonia.Media.Brushes.Gray;
        BtnMySql.Foreground = global::Avalonia.Media.Brushes.White;
        TxtStatus.Text = _vm.StatusMessage;
    }
}
