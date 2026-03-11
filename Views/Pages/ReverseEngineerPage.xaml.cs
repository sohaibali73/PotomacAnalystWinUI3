using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PotomacAnalyst.Models;
using PotomacAnalyst.Services;
using Windows.ApplicationModel.DataTransfer;

namespace PotomacAnalyst.Views.Pages;

public sealed partial class ReverseEngineerPage : Page
{
    private readonly ApiService _api;

    public ReverseEngineerPage()
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiService>();
    }

    // ── Generate AFL from natural-language strategy description ───────────
    private async void GenerateAfl_Click(object sender, RoutedEventArgs e)
    {
        var desc = StrategyDesc.Text.Trim();
        if (string.IsNullOrEmpty(desc))
        {
            var dlg = new ContentDialog
            {
                Title           = "INPUT REQUIRED",
                Content         = "Please enter a strategy description.",
                CloseButtonText = "OK",
                XamlRoot        = XamlRoot,
            };
            await dlg.ShowAsync().AsTask();
            return;
        }

        SetLoading(true);
        AflOutput.Text = string.Empty;

        try
        {
            // ── Step 1: Start a reverse-engineer session ──────────────────
            ReverseEngineerStrategy? session = null;
            try
            {
                session = await _api.PostAsync<ReverseEngineerStartRequest, ReverseEngineerStrategy>(
                    "/reverse-engineer/start",
                    new ReverseEngineerStartRequest { Query = desc });
            }
            catch { /* endpoint may not exist — fall through to AFL fallback */ }

            // ── Step 2: If we have a session, try to generate code from it ─
            if (session is not null && !string.IsNullOrEmpty(session.Id))
            {
                try
                {
                    var codeResult = await _api.PostAsync<object, ReverseEngineerStrategy>(
                        $"/reverse-engineer/generate-code/{Uri.EscapeDataString(session.Id)}",
                        new { });

                    if (!string.IsNullOrEmpty(codeResult?.Code))
                    {
                        AflOutput.Text = codeResult.Code;
                        SetLoading(false);
                        return;
                    }

                    // code might already be on the initial session response
                    if (!string.IsNullOrEmpty(session.Code))
                    {
                        AflOutput.Text = session.Code;
                        SetLoading(false);
                        return;
                    }
                }
                catch { /* code generation step failed — fall through */ }
            }

            // ── Step 3: Fallback — use /afl/generate with the description ─
            var aflReq = new AflGenerateRequest
            {
                Prompt       = desc,
                StrategyType = "reverse_engineer",
            };

            // Include additional context from the UI if provided
            var context = AdditionalContext?.Text.Trim();
            if (!string.IsNullOrEmpty(context))
                aflReq.Prompt = $"{desc}\n\nAdditional context: {context}";

            var aflResult = await _api.PostAsync<AflGenerateRequest, AflCode>(
                "/afl/generate", aflReq);

            if (aflResult is not null && !string.IsNullOrEmpty(aflResult.Code))
            {
                AflOutput.Text = aflResult.Code;
            }
            else
            {
                AflOutput.Text = "// No code was generated. Please refine your strategy description and try again.";
            }
        }
        catch (Exception ex)
        {
            // Last-resort: show a helpful error with the original prompt commented in
            AflOutput.Text =
                $"// Error generating AFL code: {ex.Message}\n" +
                $"// Strategy: {desc.Replace("\n", " ").Substring(0, Math.Min(80, desc.Length))}\n\n" +
                "// Please check your API keys in Settings and try again.";
        }
        finally
        {
            SetLoading(false);
        }
    }

    // ── UI state helpers ──────────────────────────────────────────────────
    private void SetLoading(bool loading)
    {
        GenerateBtn.IsEnabled = !loading;
        LoadingBar.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Copy / Clear ──────────────────────────────────────────────────────
    private void CopyCode_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(AflOutput.Text)) return;
        var pkg = new DataPackage();
        pkg.SetText(AflOutput.Text);
        Clipboard.SetContent(pkg);
    }

    private void ClearCode_Click(object sender, RoutedEventArgs e)
    {
        AflOutput.Text         = string.Empty;
        StrategyDesc.Text      = string.Empty;
        AdditionalContext.Text = string.Empty;
    }

    // ── Navigation ────────────────────────────────────────────────────────
    private void SendToAfl_Click(object sender, RoutedEventArgs e)
        => GetMainWindow()?.NavigateTo("AflGenerator");

    private void SendToBacktest_Click(object sender, RoutedEventArgs e)
        => GetMainWindow()?.NavigateTo("Backtest");

    private MainWindow? GetMainWindow()
        => (Application.Current as App)?.MainAppWindow;
}
