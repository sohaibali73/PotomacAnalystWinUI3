using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using PotomacAnalyst.Models;
using PotomacAnalyst.Services;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;

namespace PotomacAnalyst.Views.Pages;

/// <summary>
/// AFL Generator page — integrates with the backend /afl endpoints.
/// The XAML has a rich UI with Optimize/Debug/Explain buttons, tabs, and a code panel.
/// </summary>
public sealed partial class AflGeneratorPage : Page
{
    private readonly ApiService _api;
    private string? _lastGeneratedCode;

    public AflGeneratorPage()
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiService>();
    }

    // ── Primary generate (from chat input) ─────────────────────────────
    private async void GenerateCode_Click(object sender, RoutedEventArgs e) => await GenerateAsync();

    private async void ChatInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter) { e.Handled = true; await GenerateAsync(); }
    }

    private async Task GenerateAsync()
    {
        var text = ChatInput.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        WelcomePanel.Visibility = Visibility.Collapsed;
        AddChatBubble(text, isUser: true);
        ChatInput.Text    = string.Empty;
        SendBtn.IsEnabled = false;

        var thinking = AddChatBubble("Generating AFL code...", isUser: false);

        try
        {
            var req = new AflGenerateRequest { Prompt = text, StrategyType = "trend_following" };
            var result = await _api.PostAsync<AflGenerateRequest, AflCode>("/afl/generate", req);

            MessageList.Children.Remove(thinking);

            if (result is not null && !string.IsNullOrEmpty(result.Code))
            {
                CodeOutput.Text    = result.Code;
                _lastGeneratedCode = result.Code;
                AddChatBubble(result.Explanation ?? "AFL code generated successfully.", isUser: false);
            }
            else
            {
                AddChatBubble("Generation returned empty result. Please try again.", isUser: false);
            }
        }
        catch (Exception ex)
        {
            MessageList.Children.Remove(thinking);
            AddChatBubble($"Error: {ex.Message}", isUser: false);
        }
        finally
        {
            SendBtn.IsEnabled = true;
            ChatScrollViewer.ChangeView(null, double.MaxValue, null);
        }
    }

    // ── Action toolbar buttons ──────────────────────────────────────────
    private async void Optimize_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(CodeOutput.Text)) return;
        await RunCodeAction("/afl/optimize", CodeOutput.Text, "Optimizing AFL code...");
    }

    private async void Debug_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(CodeOutput.Text)) return;
        await RunCodeAction("/afl/debug", CodeOutput.Text, "Debugging AFL code...");
    }

    private async void Explain_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(CodeOutput.Text)) return;
        AddChatBubble("Explain the current AFL code.", isUser: true);
        try
        {
            var req = new { code = CodeOutput.Text };
            var res = await _api.PostAsync<object, AflCode>("/afl/explain", req);
            AddChatBubble(res?.Explanation ?? "Explanation not available.", isUser: false);
        }
        catch (Exception ex)
        {
            AddChatBubble($"Explain error: {ex.Message}", isUser: false);
        }
    }

    private void Feedback_Click(object sender, RoutedEventArgs e)
    {
        // Feedback dialog — placeholder for future implementation
        AddChatBubble("Feedback submitted. Thank you!", isUser: false);
    }

    private async Task RunCodeAction(string endpoint, string code, string loadingMsg)
    {
        AddChatBubble(loadingMsg, isUser: true);
        var thinking = AddChatBubble("Working...", isUser: false);
        try
        {
            var req = new { code };
            var res = await _api.PostAsync<object, AflCode>(endpoint, req);
            MessageList.Children.Remove(thinking);
            if (res is not null && !string.IsNullOrEmpty(res.Code))
            {
                CodeOutput.Text    = res.Code;
                _lastGeneratedCode = res.Code;
                AddChatBubble(res.Explanation ?? "Done.", isUser: false);
            }
        }
        catch (Exception ex)
        {
            MessageList.Children.Remove(thinking);
            AddChatBubble($"Error: {ex.Message}", isUser: false);
        }
    }

    // ── UI panel controls ───────────────────────────────────────────────
    private void ToggleCodePanel_Click(object sender, RoutedEventArgs e)
    {
        // Toggle code panel visibility (left/right split)
        if (CodePanel is not null)
            CodePanel.Visibility = CodePanel.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
    }

    private void NewAflChat_Click(object sender, RoutedEventArgs e)
    {
        MessageList.Children.Clear();
        MessageList.Children.Add(WelcomePanel);
        WelcomePanel.Visibility = Visibility.Visible;
        CodeOutput.Text = string.Empty;
        _lastGeneratedCode = null;
        ChatInput.Focus(FocusState.Programmatic);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Filter session list — placeholder
    }

    private void CompositeToggle_Toggled(object sender, RoutedEventArgs e)
    {
        // Toggle between simple/composite mode — placeholder
    }

    // ── Mode tabs ───────────────────────────────────────────────────────
    private void TabStandalone_Click(object sender, RoutedEventArgs e) { }
    private void TabEntry_Click(object sender, RoutedEventArgs e) { }
    private void TabExit_Click(object sender, RoutedEventArgs e) { }

    // ── Code output ─────────────────────────────────────────────────────
    private void CodeOutput_TextChanged(object sender, TextChangedEventArgs e)
        => LineCountLabel.Text = $"{CodeOutput.Text.Split('\n').Length} LINES";

    private void CopyCode_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(CodeOutput.Text)) return;
        var pkg = new DataPackage();
        pkg.SetText(CodeOutput.Text);
        Clipboard.SetContent(pkg);
    }

    private void ClearAll_Click(object sender, RoutedEventArgs e)
    {
        CodeOutput.Text = string.Empty;
        _lastGeneratedCode = null;
        MessageList.Children.Clear();
        MessageList.Children.Add(WelcomePanel);
        WelcomePanel.Visibility = Visibility.Visible;
    }

    // ── Quick navigation ─────────────────────────────────────────────────
    private void SendToBacktest_Click(object sender, RoutedEventArgs e)
        => GetMainWindow()?.NavigateTo("Backtest");

    private void SaveToKnowledge_Click(object sender, RoutedEventArgs e)
        => GetMainWindow()?.NavigateTo("KnowledgeBase");

    // ── Chat bubble helper ───────────────────────────────────────────────
    private Border AddChatBubble(string text, bool isUser)
    {
        var bubble = new Border
        {
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255,
                isUser ? (byte)0x2A : (byte)0x1A,
                isUser ? (byte)0x2A : (byte)0x1A,
                isUser ? (byte)0x2A : (byte)0x2E)),
            BorderBrush     = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x42, 0x42, 0x42)),
            BorderThickness = new Thickness(1),
            CornerRadius    = new CornerRadius(8),
            Padding         = new Thickness(10, 8, 10, 8),
            Margin          = new Thickness(0, 4, 0, 4),
            HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
        };
        bubble.Child = new TextBlock
        {
            Text         = text,
            FontFamily   = (FontFamily)Application.Current.Resources["QuicksandRegular"],
            FontSize     = 13,
            Foreground   = (SolidColorBrush)Application.Current.Resources["TextPrimaryBrush"],
            TextWrapping = TextWrapping.Wrap, MaxWidth = 240,
        };
        MessageList.Children.Add(bubble);
        return bubble;
    }

    private MainWindow? GetMainWindow() => (Application.Current as App)?.MainAppWindow;
}
