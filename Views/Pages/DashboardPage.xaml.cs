using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PotomacAnalyst.Models;
using PotomacAnalyst.Services;

namespace PotomacAnalyst.Views.Pages;

public sealed partial class DashboardPage : Page
{
    private readonly ApiService _api;
    private readonly SessionService _session;

    // Cancelled on Unloaded so in-flight requests don't race against a dead page.
    private CancellationTokenSource? _cts;

    public DashboardPage()
    {
        InitializeComponent();
        _api = App.Services.GetRequiredService<ApiService>();
        _session = App.Services.GetRequiredService<SessionService>();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    // ── Lifecycle ───────────────────────────────────────────────────────────

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _cts = new CancellationTokenSource();
        try
        {
            await LoadDashboardAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Load error: {ex.Message}");
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    // ── Data loading ────────────────────────────────────────────────────────

    private async Task LoadDashboardAsync(CancellationToken ct)
    {
        // Resolve display name synchronously — no async needed.
        var email = _session.GetEmail() ?? string.Empty;
        WelcomeName.Text = email.Contains('@') ? email.Split('@')[0].ToUpper() : "USER";

        try
        {
            // Fire all three requests concurrently — previously conversations
            // was fetched TWICE (once for stats, once for the list). Single call now.
            var convosTask = _api.GetAsync<List<ApiConversation>>("/chat/conversations");
            var codesTask = _api.GetAsync<List<AflCode>>("/afl/codes");
            var docsTask = _api.GetAsync<List<ApiDocument>>("/brain/documents");

            // Await all off the UI thread so the compositor keeps running.
            await Task.WhenAll(convosTask, codesTask, docsTask).ConfigureAwait(false);

            ct.ThrowIfCancellationRequested();

            var convos = convosTask.Result;
            var codes = codesTask.Result;
            var docs = docsTask.Result;

            // Marshal all visual updates in a single DispatcherQueue batch.
            DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                // Stats
                StatConversations.Text = convos?.Count.ToString() ?? "0";
                StatStrategies.Text = codes?.Count.ToString() ?? "0";
                StatDocuments.Text = docs?.Count.ToString() ?? "0";
                StatBacktests.Text = "0";

                // Conversation list — ItemsRepeater.ItemsSource replaces
                // all the manual Border/StackPanel/TextBlock construction.
                if (convos is { Count: > 0 })
                {
                    EmptyConversationsState.Visibility = Visibility.Collapsed;
                    RecentConversationsList.ItemsSource = convos.Take(5).ToList();
                }
            });
        }
        catch (OperationCanceledException)
        {
            // Page was navigated away from; silently discard.
        }
        catch
        {
            // Network/deserialisation failure — leave placeholder dashes visible.
        }
    }

    // ── Navigation ──────────────────────────────────────────────────────────

    private void StartGenerating_Click(object sender, RoutedEventArgs e) => Navigate("AflGenerator");
    private void ViewAllChats_Click(object sender, RoutedEventArgs e) => Navigate("Chat");
    private void NavToAfl_Click(object sender, RoutedEventArgs e) => Navigate("AflGenerator");
    private void NavToChat_Click(object sender, RoutedEventArgs e) => Navigate("Chat");
    private void NavToResearcher_Click(object sender, RoutedEventArgs e) => Navigate("Researcher");
    private void NavToBacktest_Click(object sender, RoutedEventArgs e) => Navigate("Backtest");
    private void NavToDeck_Click(object sender, RoutedEventArgs e) => Navigate("DeckGenerator");
    private void NavToKnowledge_Click(object sender, RoutedEventArgs e) => Navigate("KnowledgeBase");

    private void Navigate(string tag) => GetMainWindow()?.NavigateTo(tag);
    private MainWindow? GetMainWindow() => (Application.Current as App)?.MainAppWindow;
}
