using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PotomacAnalyst.Models;
using PotomacAnalyst.Services;

namespace PotomacAnalyst.Views.Pages;

public sealed partial class BacktestPage : Page
{
    private readonly ApiService _api;

    public BacktestPage()
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiService>();
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int idx))
            SwitchTab(idx);
    }

    private void SwitchTab(int idx)
    {
        OverviewTab.Visibility = idx == 0 ? Visibility.Visible : Visibility.Collapsed;
        DetailTab.Visibility   = idx == 1 ? Visibility.Visible : Visibility.Collapsed;
        CompareTab.Visibility  = idx == 2 ? Visibility.Visible : Visibility.Collapsed;

        TabOverview.Style = idx == 0
            ? (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonActiveStyle"]
            : (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonStyle"];
        TabDetail.Style   = idx == 1
            ? (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonActiveStyle"]
            : (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonStyle"];
        TabCompare.Style  = idx == 2
            ? (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonActiveStyle"]
            : (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonStyle"];
    }

    private void DropZone_DragOver(object sender, DragEventArgs e)
        => e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;

    private async void DropZone_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems)) return;
        var items = await e.DataView.GetStorageItemsAsync().AsTask();
        if (items.Count == 0) return;

        var file = items[0] as Windows.Storage.StorageFile;
        if (file is null) return;

        await UploadBacktestFileAsync(file);
    }

    private async void UploadFile_Click(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.FileTypeFilter.Add(".csv");
        picker.FileTypeFilter.Add(".json");
        WinRT.Interop.InitializeWithWindow.Initialize(picker,
            WinRT.Interop.WindowNative.GetWindowHandle(GetMainWindow()!));
        var file = await picker.PickSingleFileAsync().AsTask();
        if (file is null) return;

        await UploadBacktestFileAsync(file);
    }

    private async Task UploadBacktestFileAsync(Windows.Storage.StorageFile file)
    {
        try
        {
            SetLoading(true);
            var bytes = (await Windows.Storage.FileIO.ReadBufferAsync(file)).ToArray();

            var result = await _api.PostFileAsync<BacktestResult>(
                "/backtest/upload", bytes, file.Name, "file");

            if (result is not null)
                ShowResults(result);
        }
        catch (Exception ex)
        {
            ShowError($"Upload failed: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void UseDemoData_Click(object sender, RoutedEventArgs e)
    {
        ShowResults(new BacktestResult
        {
            TotalReturn  = 24.7,
            SharpeRatio  = 1.82,
            MaxDrawdown  = -8.3,
            WinRate      = 58.4,
            TotalTrades  = 142,
            AiInsights   =
                "The strategy demonstrates strong risk-adjusted returns with a Sharpe ratio of 1.82. " +
                "Maximum drawdown of 8.3% is within acceptable limits. The 58.4% win rate combined " +
                "with a favorable reward/risk ratio indicates a robust strategy.",
        });
    }

    private void ShowResults(BacktestResult r)
    {
        MetricsPanel.Visibility = Visibility.Visible;
        MetricReturn.Text   = $"{(r.TotalReturn >= 0 ? "+" : "")}{r.TotalReturn:F1}%";
        MetricSharpe.Text   = r.SharpeRatio.ToString("F2");
        MetricDrawdown.Text = $"{r.MaxDrawdown:F1}%";
        MetricWinRate.Text  = $"{r.WinRate:F1}%";

        if (!string.IsNullOrEmpty(r.AiInsights))
            AiInsightsText.Text = r.AiInsights;

        // Color total return
        MetricReturn.Foreground = r.TotalReturn >= 0
            ? (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["PotomacYellowBrush"]
            : (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["PotomacPinkBrush"];

        MetricDrawdown.Foreground =
            (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["PotomacPinkBrush"];
    }

    private void ShowError(string msg)
    {
        MetricsPanel.Visibility = Visibility.Collapsed;
        AiInsightsText.Text     = msg;
    }

    private void SetLoading(bool loading)
    {
        // Could show/hide a loading indicator
    }

    private MainWindow? GetMainWindow() => (Application.Current as App)?.MainAppWindow;
}
