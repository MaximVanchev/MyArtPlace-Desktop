using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MyArtPlace.Core.Models;
using MyArtPlace.ViewModels;
using MyArtPlace.ViewModels.Reusable;

namespace MyArtPlace.Avalonia.Views;

public partial class ArtistView : UserControl
{
    private readonly ArtistListViewModel _vm;
    private List<FilterField> _filters;
    private readonly List<TextBox> _filterInputs = new();

    public ArtistView()
    {
        InitializeComponent();
        _vm = new ArtistListViewModel(App.ArtistRepo);
        _filters = FilterBuilder.BuildFilters<Artist>("Name", "Country", "BirthYear");
        BuildFilterControls();
        Loaded += async (_, _) =>
        {
            await _vm.LoadArtistsAsync();
            ApplyFilters();
        };
    }

    private void BuildFilterControls()
    {
        FilterPanel.Children.Clear();
        _filterInputs.Clear();

        foreach (var filter in _filters)
        {
            var stack = new StackPanel { Spacing = 3 };
            stack.Children.Add(new TextBlock { Text = filter.DisplayName, FontWeight = global::Avalonia.Media.FontWeight.SemiBold, FontSize = 12 });

            var textBox = new TextBox { Watermark = $"Search {filter.DisplayName.ToLower()}...", MinWidth = 140, Tag = filter };
            stack.Children.Add(textBox);
            _filterInputs.Add(textBox);

            FilterPanel.Children.Add(stack);
        }
    }

    private void ApplyFilters()
    {
        foreach (var tb in _filterInputs)
        {
            var f = (FilterField)tb.Tag!;
            f.Value = string.IsNullOrWhiteSpace(tb.Text) ? null : tb.Text;
        }

        var filtered = FilterBuilder.ApplyFilters(_vm.Artists, _filters);
        ArtistGrid.ItemsSource = filtered;
        StatusText.Text = $"Showing {filtered.Count} of {_vm.Artists.Count} artists";
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        BtnApply.Click += (_, _) => ApplyFilters();
        BtnClear.Click += (_, _) =>
        {
            foreach (var tb in _filterInputs) tb.Text = "";
            foreach (var f in _filters) f.Value = null;
            ApplyFilters();
        };
    }
}
