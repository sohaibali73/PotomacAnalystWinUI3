using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PotomacAnalyst.Models;
using PotomacAnalyst.Services;
using System.Text;
using Windows.System;

namespace PotomacAnalyst.Views.Pages;

public sealed partial class ResearcherPage : Page
{
    private readonly ApiService _api;
    private readonly Button[] _tabs;

    public ResearcherPage()
    {
        InitializeComponent();
        _api  = App.Services.GetRequiredService<ApiService>();
        _tabs = new[] { TabQuickSearch, TabCompany, TabPeer, TabStrategy };
    }

    // ── Tab switching ──────────────────────────────────────────────────────
    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int idx))
            SwitchTab(idx);
    }

    private void SwitchTab(int idx)
    {
        QuickSearchTab.Visibility = idx == 0 ? Visibility.Visible : Visibility.Collapsed;
        CompanyTab.Visibility     = idx == 1 ? Visibility.Visible : Visibility.Collapsed;
        PeerTab.Visibility        = idx == 2 ? Visibility.Visible : Visibility.Collapsed;
        StrategyTab.Visibility    = idx == 3 ? Visibility.Visible : Visibility.Collapsed;

        for (int i = 0; i < _tabs.Length; i++)
        {
            _tabs[i].Style = i == idx
                ? (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonActiveStyle"]
                : (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonStyle"];
        }
    }

    // ── Quick search ───────────────────────────────────────────────────────
    private void QuickSearch_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter) QuickSearch_Click(sender, e);
    }

    private void QuickSearch_Click(object sender, RoutedEventArgs e)
    {
        var q = QuickSearchBox.Text.Trim();
        if (string.IsNullOrEmpty(q)) return;
        CompanySearchBox.Text = q;
        SwitchTab(1);
        CompanyResearch_Click(sender, e);
    }

    private void QuickChip_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
        {
            QuickSearchBox.Text = btn.Tag?.ToString() ?? string.Empty;
            QuickSearch_Click(sender, e);
        }
    }

    private void GoToCompany_Click(object sender, RoutedEventArgs e)  => SwitchTab(1);
    private void GoToPeer_Click(object sender, RoutedEventArgs e)      => SwitchTab(2);
    private void GoToStrategy_Click(object sender, RoutedEventArgs e)  => SwitchTab(3);

    // ── Company research ───────────────────────────────────────────────────
    private async void CompanyResearch_Click(object sender, RoutedEventArgs e)
    {
        var symbol = CompanySearchBox.Text.Trim().ToUpper();
        if (string.IsNullOrEmpty(symbol)) return;

        CompanyResults.Visibility  = Visibility.Visible;
        CompanyResultsText.Text    = $"Researching {symbol}...";
        CompanyResultsText.Foreground =
            (SolidColorBrush)Application.Current.Resources["TextMutedBrush"];

        try
        {
            var result = await _api.GetAsync<ResearcherCompanyResult>(
                $"/researcher/company/{Uri.EscapeDataString(symbol)}");

            if (result is null)
            {
                ShowCompanyError($"No data found for {symbol}.");
                return;
            }

            var sb = new StringBuilder();

            // Company identity line
            var identity = string.IsNullOrEmpty(result.Name)
                ? result.Symbol
                : $"{result.Symbol}  —  {result.Name}";
            sb.AppendLine(identity);

            // Price
            if (result.Price > 0)
                sb.AppendLine($"Price: ${result.Price:F2}");

            // Sector / Industry
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(result.Sector))   parts.Add($"Sector: {result.Sector}");
            if (!string.IsNullOrEmpty(result.Industry)) parts.Add($"Industry: {result.Industry}");
            if (parts.Count > 0) sb.AppendLine(string.Join("   |   ", parts));

            // Market cap
            if (result.MarketCap > 0)
                sb.AppendLine($"Market Cap: {FormatMarketCap(result.MarketCap)}");

            CompanyResultsText.Text = sb.ToString().Trim();
            CompanyResultsText.Foreground =
                (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"];
        }
        catch (Exception ex)
        {
            ShowCompanyError($"Error: {ex.Message}");
        }
    }

    private void ShowCompanyError(string msg)
    {
        CompanyResults.Visibility    = Visibility.Visible;
        CompanyResultsText.Text      = msg;
        CompanyResultsText.Foreground =
            (SolidColorBrush)Application.Current.Resources["PotomacPinkBrush"];
    }

    // ── Peer comparison ────────────────────────────────────────────────────
    private async void PeerCompare_Click(object sender, RoutedEventArgs e)
    {
        var input = PeerTickers.Text.Trim();
        if (string.IsNullOrEmpty(input)) return;

        var tickers = input
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToUpper())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToArray();

        if (tickers.Length == 0) return;

        ShowPeerResult("Comparing " + string.Join(", ", tickers) + "...", muted: true);

        try
        {
            var req = new PeerComparisonRequest
            {
                Symbol = tickers[0],
                Peers  = tickers.Length > 1 ? tickers.Skip(1).ToArray() : null,
            };

            var result = await _api.PostAsync<PeerComparisonRequest, PeerComparisonResult>(
                "/researcher/comparison", req);

            var text = !string.IsNullOrWhiteSpace(result?.Analysis)
                ? result!.Analysis!
                : $"Comparison loaded for {string.Join(", ", tickers)}.";

            ShowPeerResult(text, muted: false);
        }
        catch (Exception ex)
        {
            ShowPeerResult($"Error: {ex.Message}", muted: false, error: true);
        }
    }

    private void ShowPeerResult(string text, bool muted, bool error = false)
    {
        PeerResultBorder.Visibility = Visibility.Visible;
        PeerResultText.Text         = text;
        PeerResultText.Foreground   = error
            ? (SolidColorBrush)Application.Current.Resources["PotomacPinkBrush"]
            : muted
                ? (SolidColorBrush)Application.Current.Resources["TextMutedBrush"]
                : (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"];
    }

    // ── Strategy analysis ──────────────────────────────────────────────────
    private async void StrategyAnalyze_Click(object sender, RoutedEventArgs e)
    {
        var desc = StrategyInput.Text.Trim();
        if (string.IsNullOrEmpty(desc)) return;

        ShowStrategyResult("Analyzing strategy...", muted: true);

        try
        {
            var req = new StrategyAnalysisRequest
            {
                Symbol            = "GENERAL",
                StrategyType      = "custom",
                Timeframe         = "Daily",
                AdditionalContext = desc,
            };

            var result = await _api.PostAsync<StrategyAnalysisRequest, StrategyAnalysisResult>(
                "/researcher/strategy-analysis", req);

            var text = !string.IsNullOrWhiteSpace(result?.Analysis)
                ? result!.Analysis!
                : "Strategy analysis complete.";

            ShowStrategyResult(text, muted: false);
        }
        catch (Exception ex)
        {
            ShowStrategyResult($"Error: {ex.Message}", muted: false, error: true);
        }
    }

    private void ShowStrategyResult(string text, bool muted, bool error = false)
    {
        StrategyResultBorder.Visibility = Visibility.Visible;
        StrategyResultText.Text         = text;
        StrategyResultText.Foreground   = error
            ? (SolidColorBrush)Application.Current.Resources["PotomacPinkBrush"]
            : muted
                ? (SolidColorBrush)Application.Current.Resources["TextMutedBrush"]
                : (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"];
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private static string FormatMarketCap(double cap) =>
        cap >= 1e12 ? $"${cap / 1e12:F2}T" :
        cap >= 1e9  ? $"${cap / 1e9:F2}B"  :
        cap >= 1e6  ? $"${cap / 1e6:F2}M"  :
                      $"${cap:F0}";
}
