using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using MyArtPlace.Core.Enums;
using MyArtPlace.ViewModels;

namespace MyArtPlace.Avalonia.Views;

public partial class ArtEditorView : UserControl
{
    private readonly ArtEditorViewModel _vm;

    public ArtEditorView()
    {
        InitializeComponent();
        _vm = new ArtEditorViewModel(App.ArtRepo);
        _vm.LoadForCreate();

        foreach (var cat in Enum.GetValues<ArtCategory>())
            CmbCategory.Items.Add(cat.ToString());
        CmbCategory.SelectedIndex = 0;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        BtnSave.Click += async (_, _) => await SaveArt();
        BtnClear.Click += (_, _) => ClearForm();
    }

    private async Task SaveArt()
    {
        _vm.Title = TxtTitle.Text ?? "";
        _vm.Artist = TxtArtist.Text ?? "";
        _vm.Category = Enum.TryParse<ArtCategory>(CmbCategory.SelectedItem?.ToString(), out var cat) ? cat : ArtCategory.Painting;
        _vm.YearCreated = int.TryParse(TxtYear.Text, out var y) ? y : null;
        _vm.Description = TxtDescription.Text;
        _vm.ImageUrl = TxtImageUrl.Text;
        _vm.Price = decimal.TryParse(TxtPrice.Text, out var p) ? p : null;

        await _vm.SaveAsync();

        if (_vm.ErrorMessage is not null)
        {
            TxtMessage.Text = _vm.ErrorMessage;
            TxtMessage.Foreground = Brushes.Red;
            TxtMessage.IsVisible = true;
        }
        else if (_vm.SuccessMessage is not null)
        {
            TxtMessage.Text = _vm.SuccessMessage;
            TxtMessage.Foreground = Brushes.Green;
            TxtMessage.IsVisible = true;
        }
    }

    private void ClearForm()
    {
        _vm.LoadForCreate();
        TxtTitle.Text = "";
        TxtArtist.Text = "";
        CmbCategory.SelectedIndex = 0;
        TxtYear.Text = "";
        TxtDescription.Text = "";
        TxtImageUrl.Text = "";
        TxtPrice.Text = "";
        TxtMessage.IsVisible = false;
    }
}
