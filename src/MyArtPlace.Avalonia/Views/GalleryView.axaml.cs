using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MyArtPlace.Core.Models;
using MyArtPlace.ViewModels;
using MyArtPlace.ViewModels.Reusable;

namespace MyArtPlace.Avalonia.Views;

public partial class GalleryView : UserControl
{
    private readonly ArtGalleryViewModel _vm;
    private List<FilterField> _filters;
    private readonly List<TextBox> _filterInputs = new();
    private readonly List<ComboBox> _filterCombos = new();

    public GalleryView()
    {
        InitializeComponent();
        _vm = new ArtGalleryViewModel(App.ArtRepo);
        _filters = FilterBuilder.BuildFilters<ArtPiece>("Title", "Artist", "Category");
        BuildFilterControls();
        Loaded += async (_, _) => await LoadData();
    }

    private void BuildFilterControls()
    {
        FilterPanel.Children.Clear();
        _filterInputs.Clear();
        _filterCombos.Clear();

        foreach (var filter in _filters)
        {
            var stack = new StackPanel { Spacing = 3 };
            stack.Children.Add(new TextBlock { Text = filter.DisplayName, FontWeight = global::Avalonia.Media.FontWeight.SemiBold, FontSize = 12 });

            if (filter.IsEnum && filter.EnumValues is not null)
            {
                var combo = new ComboBox { MinWidth = 140, Tag = filter };
                combo.Items.Add("-- All --");
                foreach (var v in filter.EnumValues)
                    combo.Items.Add(v);
                combo.SelectedIndex = 0;
                stack.Children.Add(combo);
                _filterCombos.Add(combo);
            }
            else
            {
                var textBox = new TextBox { Watermark = $"Search {filter.DisplayName.ToLower()}...", MinWidth = 140, Tag = filter };
                stack.Children.Add(textBox);
                _filterInputs.Add(textBox);
            }

            FilterPanel.Children.Add(stack);
        }
    }

    private async Task LoadData()
    {
        await _vm.LoadArtAsync();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        foreach (var tb in _filterInputs)
        {
            var f = (FilterField)tb.Tag!;
            f.Value = string.IsNullOrWhiteSpace(tb.Text) ? null : tb.Text;
        }
        foreach (var cb in _filterCombos)
        {
            var f = (FilterField)cb.Tag!;
            f.Value = cb.SelectedIndex <= 0 ? null : cb.SelectedItem?.ToString();
        }

        var filtered = FilterBuilder.ApplyFilters(_vm.ArtPieces, _filters);
        ArtGrid.ItemsSource = filtered;
        StatusText.Text = $"Showing {filtered.Count} of {_vm.ArtPieces.Count} art pieces";
    }

    // ReSharper disable UnusedParameter.Local
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var applyBtn = this.FindControl<Button>("BtnApplyFilters");
        var clearBtn = this.FindControl<Button>("BtnClearFilters");
        var refreshBtn = this.FindControl<Button>("BtnRefresh");

        if (applyBtn is not null) applyBtn.Click += (_, _) => ApplyFilters();
        if (clearBtn is not null) clearBtn.Click += (_, _) =>
        {
            foreach (var tb in _filterInputs) tb.Text = "";
            foreach (var cb in _filterCombos) cb.SelectedIndex = 0;
            foreach (var f in _filters) f.Value = null;
            ApplyFilters();
        };
        if (refreshBtn is not null) refreshBtn.Click += async (_, _) => await LoadData();
    }
}
