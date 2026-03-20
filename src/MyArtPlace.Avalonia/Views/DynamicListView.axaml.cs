using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MyArtPlace.Core.Models;
using MyArtPlace.ViewModels.Reusable;

namespace MyArtPlace.Avalonia.Views;

public partial class DynamicListView : UserControl
{
    private readonly DynamicListViewModel _dynList = new();
    private string _currentTable = "";
    private bool _columnsExpanded;

    public DynamicListView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        BtnShowArt.Click += async (_, _) => await ShowArtPieces();
        BtnShowArtists.Click += async (_, _) => await ShowArtists();
        BtnChangeColumns.Click += (_, _) => ChangeColumns();
        BtnClear.Click += (_, _) => ClearAll();
    }

    private async Task ShowArtPieces()
    {
        var items = await App.ArtRepo.GetAllAsync();
        _dynList.SetData(items, "Title", "Artist", "Category", "Price");
        _currentTable = "Art Pieces";
        _columnsExpanded = false;
        RefreshGrid();
    }

    private async Task ShowArtists()
    {
        var items = await App.ArtistRepo.GetAllAsync();
        _dynList.SetData(items, "Name", "Country", "BirthYear");
        _currentTable = "Artists";
        _columnsExpanded = false;
        RefreshGrid();
    }

    private void ChangeColumns()
    {
        if (_currentTable == "Art Pieces")
        {
            if (!_columnsExpanded)
                _dynList.ChangeColumns<ArtPiece>("Title", "Artist", "Category", "YearCreated", "Price", "Description");
            else
                _dynList.ChangeColumns<ArtPiece>("Title", "Price");
            _columnsExpanded = !_columnsExpanded;
        }
        else if (_currentTable == "Artists")
        {
            if (!_columnsExpanded)
                _dynList.ChangeColumns<Artist>("Name", "Country", "BirthYear", "Biography", "Email");
            else
                _dynList.ChangeColumns<Artist>("Name", "Email");
            _columnsExpanded = !_columnsExpanded;
        }
        RefreshGrid();
    }

    private void ClearAll()
    {
        _dynList.Clear();
        _currentTable = "";
        DynGrid.Columns.Clear();
        DynGrid.ItemsSource = null;
        TxtInfo.Text = "";
        TxtSelected.Text = "";
    }

    private void RefreshGrid()
    {
        DynGrid.Columns.Clear();
        foreach (var col in _dynList.Columns)
        {
            DynGrid.Columns.Add(new DataGridTextColumn
            {
                Header = col.Header,
                Binding = new global::Avalonia.Data.Binding($"DisplayValues[{_dynList.Columns.IndexOf(col)}]"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
        }
        DynGrid.ItemsSource = _dynList.Rows.ToList();
        TxtInfo.Text = $"Showing: {_currentTable} | Columns: {string.Join(", ", _dynList.Columns.Select(c => c.Header))}";
        TxtSelected.Text = "";
    }

    private void OnGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DynGrid.SelectedItem is DynamicRow row)
        {
            _dynList.SelectedRow = row;
            if (_currentTable == "Art Pieces")
            {
                var art = _dynList.GetSelectedObject<ArtPiece>();
                if (art is not null)
                    TxtSelected.Text = $"Selected: Id={art.Id}, Title='{art.Title}', Artist='{art.Artist}', Category={art.Category}, Year={art.YearCreated}, Price={art.Price}";
            }
            else if (_currentTable == "Artists")
            {
                var artist = _dynList.GetSelectedObject<Artist>();
                if (artist is not null)
                    TxtSelected.Text = $"Selected: Id={artist.Id}, Name='{artist.Name}', Country='{artist.Country}', BirthYear={artist.BirthYear}";
            }
        }
    }
}
