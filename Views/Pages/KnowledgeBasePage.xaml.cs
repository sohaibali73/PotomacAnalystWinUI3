using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PotomacAnalyst.Models;
using PotomacAnalyst.Services;

namespace PotomacAnalyst.Views.Pages;

public sealed partial class KnowledgeBasePage : Page
{
    private readonly ApiService _api;

    public KnowledgeBasePage()
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiService>();
        Loaded += KnowledgeBasePage_Loaded;
    }

    private async void KnowledgeBasePage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DocumentsTab.Visibility == Visibility.Visible)
                await LoadDocumentsAsync();
        }
        catch { /* Backend unreachable */ }
    }

    private async void TabDiscover_Click(object sender, RoutedEventArgs e)  => SwitchTab(0);
    private async void TabDocuments_Click(object sender, RoutedEventArgs e) { SwitchTab(1); await LoadDocumentsAsync(); }
    private void TabUpload_Click(object sender, RoutedEventArgs e)           => SwitchTab(2);

    private void SwitchTab(int idx)
    {
        DiscoverTab.Visibility  = idx == 0 ? Visibility.Visible : Visibility.Collapsed;
        DocumentsTab.Visibility = idx == 1 ? Visibility.Visible : Visibility.Collapsed;
        UploadTab.Visibility    = idx == 2 ? Visibility.Visible : Visibility.Collapsed;

        TabDiscover.Style  = idx == 0
            ? (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonActiveStyle"]
            : (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonStyle"];
        TabDocuments.Style = idx == 1
            ? (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonActiveStyle"]
            : (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonStyle"];
        TabUpload.Style    = idx == 2
            ? (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonActiveStyle"]
            : (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonStyle"];
    }

    private async Task LoadDocumentsAsync()
    {
        try
        {
            var docs = await _api.GetAsync<List<ApiDocument>>("/brain/documents");
            DocumentsList.Children.Clear();

            if (docs is null || docs.Count == 0)
            {
                var empty = new Border { Padding = new Thickness(20, 16, 20, 16) };
                empty.Child = new TextBlock
                {
                    Text = "No documents uploaded yet.",
                    FontFamily = (FontFamily)Application.Current.Resources["QuicksandRegular"],
                    FontSize = 13,
                    Foreground = (SolidColorBrush)Application.Current.Resources["TextSecondaryBrush"],
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                DocumentsList.Children.Add(empty);
                return;
            }

            foreach (var doc in docs)
            {
                var item = new Border
                {
                    Padding = new Thickness(16, 12, 16, 12),
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderBrush"],
                    BorderThickness = new Thickness(0, 0, 0, 1),
                };

                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });

                var info = new StackPanel { Spacing = 2 };
                info.Children.Add(new TextBlock
                {
                    Text = doc.Title,
                    FontFamily = (FontFamily)Application.Current.Resources["QuicksandMedium"],
                    FontSize = 13,
                    Foreground = (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"],
                });
                info.Children.Add(new TextBlock
                {
                    Text = doc.Category ?? "General",
                    FontFamily = (FontFamily)Application.Current.Resources["QuicksandRegular"],
                    FontSize = 11,
                    Foreground = (SolidColorBrush)Application.Current.Resources["TextMutedBrush"],
                });

                var deleteBtn = new Button
                {
                    Style = (Microsoft.UI.Xaml.Style)Application.Current.Resources["DangerButtonStyle"],
                    Content = "DELETE", Padding = new Thickness(10, 6, 10, 6), FontSize = 10, Tag = doc.Id,
                };
                deleteBtn.Click += DeleteDocument_Click;

                Grid.SetColumn(info, 0);
                Grid.SetColumn(deleteBtn, 1);
                row.Children.Add(info);
                row.Children.Add(deleteBtn);
                item.Child = row;
                DocumentsList.Children.Add(item);
            }
        }
        catch { }
    }

    private async void DeleteDocument_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string id)
        {
            try { await _api.DeleteAsync($"/brain/documents/{id}"); await LoadDocumentsAsync(); }
            catch { }
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) { }
    private void UploadNew_Click(object sender, RoutedEventArgs e) => SwitchTab(2);

    private void DropZone_DragOver(object sender, DragEventArgs e)
        => e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems)) return;
        var items = await e.DataView.GetStorageItemsAsync().AsTask();
        foreach (var item in items)
            if (item is Windows.Storage.StorageFile f)
                await UploadDocumentAsync(f);
        SwitchTab(1); await LoadDocumentsAsync();
    }

    private async void BrowseFiles_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.FileTypeFilter.Add(".pdf");
        picker.FileTypeFilter.Add(".txt");
        picker.FileTypeFilter.Add(".docx");
        picker.FileTypeFilter.Add(".csv");
        WinRT.Interop.InitializeWithWindow.Initialize(picker,
            WinRT.Interop.WindowNative.GetWindowHandle(GetMainWindow()!));
        var files = await picker.PickMultipleFilesAsync().AsTask();
        if (files?.Count > 0)
        {
            foreach (var file in files) await UploadDocumentAsync(file);
            SwitchTab(1); await LoadDocumentsAsync();
        }
    }

    private async Task UploadDocumentAsync(Windows.Storage.StorageFile file)
    {
        try
        {
            var bytes = (await Windows.Storage.FileIO.ReadBufferAsync(file)).ToArray();
            await _api.PostFileAsync<ApiDocument>("/brain/upload", bytes, file.Name, "file",
                new Dictionary<string, string> { { "category", "general" } });
        }
        catch { }
    }

    private MainWindow? GetMainWindow() => (Application.Current as App)?.MainAppWindow;
}
