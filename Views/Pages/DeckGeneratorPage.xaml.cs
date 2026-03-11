using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PotomacAnalyst.Models;
using PotomacAnalyst.Services;

namespace PotomacAnalyst.Views.Pages;

public sealed partial class DeckGeneratorPage : Page
{
    private readonly ApiService _api;
    private string? _presentationId;

    public DeckGeneratorPage()
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiService>();
    }

    private async void GenerateBtn_Click(object sender, RoutedEventArgs e)
    {
        var title = DeckTitle.Text.Trim();
        var brief = DeckBrief.Text.Trim();

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(brief))
        {
            var dlg = new ContentDialog
            {
                Title = "INPUT REQUIRED",
                Content = "Please enter a presentation title and content brief.",
                CloseButtonText = "OK", XamlRoot = XamlRoot,
            };
            await dlg.ShowAsync().AsTask();
            return;
        }

        SetGenerating(true);

        try
        {
            var slideCountMap = new Dictionary<int, int> { { 0, 5 }, { 1, 10 }, { 2, 15 }, { 3, 20 } };
            var count = slideCountMap.TryGetValue(SlideCount.SelectedIndex, out var sc) ? sc : 10;

            var req = new PresentationRequest
            {
                Prompt = $"{title}: {brief}",
                SlideCount = count,
                IncludeCharts = true,
            };

            await _api.StreamAsync(
                "/pptx/generate", req,
                onText: _ => { },
                onError: err => DispatcherQueue.TryEnqueue(() => ProgressStatus.Text = $"Error: {err}")
            );

            var presentations = await _api.GetAsync<List<ApiPresentation>>("/pptx/templates");
            var latest = presentations?.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

            if (latest is not null)
            {
                _presentationId        = latest.Id;
                DownloadBar.Visibility = Visibility.Visible;
                ProgressStatus.Text    = "Presentation ready to download";
            }
            else
            {
                DownloadBar.Visibility = Visibility.Visible;
                ProgressStatus.Text    = "Generation complete.";
            }
        }
        catch (Exception ex)
        {
            ProgressStatus.Text = $"Error: {ex.Message}";
        }
        finally
        {
            SetGenerating(false);
        }
    }

    private async void DownloadBtn_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_presentationId)) return;

        try
        {
            var bytes = await _api.GetBytesAsync($"/pptx/download/{_presentationId}");
            if (bytes is null || bytes.Length == 0) return;

            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.FileTypeChoices.Add("PowerPoint", new List<string> { ".pptx" });
            picker.SuggestedFileName = DeckTitle.Text.Trim().Replace(" ", "_");
            WinRT.Interop.InitializeWithWindow.Initialize(picker,
                WinRT.Interop.WindowNative.GetWindowHandle(GetMainWindow()!));

            var file = await picker.PickSaveFileAsync().AsTask();
            if (file is not null)
            {
                await Windows.Storage.FileIO.WriteBytesAsync(file, bytes);
                ProgressStatus.Text = $"Saved: {file.Name}";
            }
        }
        catch (Exception ex)
        {
            ProgressStatus.Text = $"Download error: {ex.Message}";
        }
    }

    private void SetGenerating(bool generating)
    {
        GenerateBtn.IsEnabled = !generating;
        ProgressPanel.Visibility = Visibility.Visible;
        if (generating)
        {
            GenerateProgress.IsIndeterminate = true;
            ProgressStatus.Text = "Generating presentation...";
        }
        else
        {
            GenerateProgress.IsIndeterminate = false;
        }
    }

    private void ChartDrop_DragOver(object sender, DragEventArgs e)
        => e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;

    private async void ChartDrop_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems)) return;
        var items = await e.DataView.GetStorageItemsAsync().AsTask();
        ChartFilesLabel.Text = $"{items.Count} image(s) attached";
    }

    private async void BrowseCharts_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        WinRT.Interop.InitializeWithWindow.Initialize(picker,
            WinRT.Interop.WindowNative.GetWindowHandle(GetMainWindow()!));
        var files = await picker.PickMultipleFilesAsync().AsTask();
        if (files?.Count > 0) ChartFilesLabel.Text = $"{files.Count} image(s) attached";
    }

    private MainWindow? GetMainWindow() => (Application.Current as App)?.MainAppWindow;
}
