using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MyArtPlace.Core.Enums;
using MyArtPlace.ViewModels;

namespace MyArtPlace.Avalonia.Views;

public partial class ArtEditorView : UserControl
{
    private readonly ArtEditorViewModel _vm;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];

    public ArtEditorView()
    {
        InitializeComponent();
        _vm = new ArtEditorViewModel(App.ArtRepo);
        _vm.LoadForCreate();

        foreach (var cat in Enum.GetValues<ArtCategory>())
            CmbCategory.Items.Add(cat.ToString());
        CmbCategory.SelectedIndex = 0;

        // Wire up drag-and-drop
        DropZone.AddHandler(DragDrop.DropEvent, OnDrop);
        DropZone.AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        BtnSave.Click += async (_, _) => await SaveArt();
        BtnClear.Click += (_, _) => ClearForm();
        BtnRemoveImage.Click += (_, _) => RemoveImage();
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
#pragma warning disable CS0618 // Using legacy DragDrop API for compatibility
        e.DragEffects = e.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
#pragma warning restore CS0618
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
#pragma warning disable CS0618
        if (!e.Data.Contains(DataFormats.Files)) return;

        var files = e.Data.GetFiles()?.ToList();
#pragma warning restore CS0618
        var file = files?.FirstOrDefault();
        if (file is null) return;

        var path = file.Path?.LocalPath;
        if (path is null) return;

        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            TxtMessage.Text = "Only image files (jpg, png, gif, bmp, webp) are allowed.";
            TxtMessage.Foreground = Brushes.Red;
            TxtMessage.IsVisible = true;
            return;
        }

        var fileInfo = new FileInfo(path);
        if (fileInfo.Length > 5 * 1024 * 1024)
        {
            TxtMessage.Text = "Image must be smaller than 5 MB.";
            TxtMessage.Foreground = Brushes.Red;
            TxtMessage.IsVisible = true;
            return;
        }

        var data = File.ReadAllBytes(path);
        var contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        _vm.SetImage(data, contentType);
        ShowImagePreview(data);
    }

    private void ShowImagePreview(byte[] data)
    {
        using var ms = new MemoryStream(data);
        ImgPreview.Source = new Bitmap(ms);
        ImagePreviewPanel.IsVisible = true;
        DropPrompt.IsVisible = false;
    }

    private void RemoveImage()
    {
        _vm.ClearImage();
        ImgPreview.Source = null;
        ImagePreviewPanel.IsVisible = false;
        DropPrompt.IsVisible = true;
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
        RemoveImage();
    }
}
