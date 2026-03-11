using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
//using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using PotomacAnalyst.Models;
using PotomacAnalyst.Services;
using PotomacAnalyst.Views.Auth;
using PotomacAnalyst.Views.Pages;
using System;
using System.Collections.Generic;
using Windows.UI;

namespace PotomacAnalyst;

public sealed partial class MainWindow : Window
{
    private readonly AuthService _auth;
    private readonly SessionService _session;

    // ── Page map: Tag → Type ───────────────────────────────────────────────
    private static readonly Dictionary<string, Type> PageMap = new()
    {
        { "Dashboard",       typeof(DashboardPage)       },
        { "AflGenerator",    typeof(AflGeneratorPage)    },
        { "Chat",            typeof(ChatPage)            },
        { "KnowledgeBase",   typeof(KnowledgeBasePage)   },
        { "Backtest",        typeof(BacktestPage)        },
        { "ReverseEngineer", typeof(ReverseEngineerPage) },
        { "Content",         typeof(ContentPage)         },
        { "DeckGenerator",   typeof(DeckGeneratorPage)   },
        { "Autopilot",       typeof(AutopilotPage)       },
        { "Skills",          typeof(SkillsPage)          },
        { "Researcher",      typeof(ResearcherPage)      },
        { "Developer",       typeof(DeveloperPage)       },
        { "Settings",        typeof(SettingsPage)        },
    };

    // ── O(1) nav-item lookup — built once in BuildNavItemCache() ──────────
    private readonly Dictionary<string, NavigationViewItem> _navItemCache = new();

    public MainWindow()
    {
        InitializeComponent();

        // Force dark theme on root before first layout pass.
        if (Content is FrameworkElement fe)
            fe.RequestedTheme = ElementTheme.Dark;

        // ── Mica backdrop ─────────────────────────────────────────────────
        if (MicaController.IsSupported())
            SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };

        // ── Custom title bar ──────────────────────────────────────────────
        ConfigureTitleBar();

        AppWindow.Resize(new Windows.Graphics.SizeInt32(1280, 800));

        _auth = App.Services.GetRequiredService<AuthService>();
        _session = App.Services.GetRequiredService<SessionService>();

        // Loaded fires exactly once after first layout — correct hook for startup.
        RootGrid.Loaded += OnRootLoaded;
    }

    // ── Title bar ─────────────────────────────────────────────────────────
    private void ConfigureTitleBar()
    {
        if (!AppWindowTitleBar.IsCustomizationSupported()) return;

        var tb = AppWindow.TitleBar;
        tb.ExtendsContentIntoTitleBar = true;

        tb.ButtonBackgroundColor = Color.FromArgb(0, 0, 0, 0);
        tb.ButtonInactiveBackgroundColor = Color.FromArgb(0, 0, 0, 0);
        tb.ButtonHoverBackgroundColor = Color.FromArgb(30, 255, 255, 255);
        tb.ButtonPressedBackgroundColor = Color.FromArgb(50, 255, 255, 255);
        tb.ButtonForegroundColor = Color.FromArgb(255, 255, 255, 255);
        tb.ButtonInactiveForegroundColor = Color.FromArgb(100, 255, 255, 255);
    }

    // ── Startup ───────────────────────────────────────────────────────────
    private async void OnRootLoaded(object sender, RoutedEventArgs e)
    {
        RootGrid.Loaded -= OnRootLoaded;

        // FIX: ApiService has a 60-second HttpClient timeout. Without a startup-specific
        // timeout, if the backend is unreachable the app shows a blank window for 60 seconds
        // which looks like a crash.  Use a 10-second cancellation token for the initial
        // profile check only — anything longer and the user should just hit login.
        using var startupCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            if (_session.IsAuthenticated())
            {
                var profile = await _auth.GetProfileAsync(startupCts.Token);
                if (profile is not null)
                {
                    ShowMainLayout(profile);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainWindow] Startup error: {ex.Message}");
            // Session may be stale, backend unreachable, or startup timed out — clear and show login.
            _session.ClearSession();
            _ = _auth.LogoutAsync(); // fire-and-forget cleanup
        }

        ShowLoginPage();
    }

    // ── Auth helpers ───────────────────────────────────────────────────────
    public void ShowLoginPage()
    {
        AuthFrame.Visibility = Visibility.Visible;
        AuthFrame.Navigate(typeof(LoginPage), this, new DrillInNavigationTransitionInfo());
    }

    public void ShowRegisterPage()
        => AuthFrame.Navigate(typeof(RegisterPage), this,
               new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });

    public void ShowForgotPasswordPage()
        => AuthFrame.Navigate(typeof(ForgotPasswordPage), this,
               new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });

    public void ShowMainLayout(UserProfile profile)
    {
        var displayName = profile.DisplayName;
        UserNameText.Text = displayName;
        UserEmailText.Text = profile.Email;
        UserInitialsText.Text = GetInitials(displayName);

        // Build O(1) nav-item lookup before first NavigateTo call.
        BuildNavItemCache();

        // FadeInElement sets Opacity=0 itself before starting the animation,
        // so we only need to make the element Visible first.
        NavView.Visibility = Visibility.Visible;
        FadeInElement(NavView, durationMs: 300);

        // Fade out and collapse the auth frame simultaneously.
        FadeOutThenCollapse(AuthFrame, durationMs: 200);

        NavigateTo("Dashboard");
    }

    // ── Composition animations ─────────────────────────────────────────────
    // Windows.UI.Composition only — no WPF, no Storyboard.
    //
    // CRITICAL PATTERN: When a Composition ScalarKeyFrameAnimation on "Opacity"
    // finishes, the visual reverts to the XAML-driven base value. Fix this by
    // using a ScopedBatch and writing the final XAML Opacity in the Completed
    // callback, which hands ownership back from the Composition layer cleanly.
    //
    // FadeInElement always writes Opacity=0 to the XAML property before
    // starting the animation so callers never need to pre-zero it themselves.

    private void FadeInElement(UIElement element, int durationMs)
    {
        // Zero the XAML property first so there is no 1-frame flash at full
        // opacity before the Composition animation takes control.
        element.Opacity = 0d;

        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;
        var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

        var anim = compositor.CreateScalarKeyFrameAnimation();
        anim.Duration = TimeSpan.FromMilliseconds(durationMs);
        anim.InsertKeyFrame(0f, 0f, compositor.CreateLinearEasingFunction());
        anim.InsertKeyFrame(1f, 1f, compositor.CreateLinearEasingFunction());

        visual.StartAnimation("Opacity", anim);

        // Bake final value into the XAML property so the Composition layer
        // releases the property cleanly at full opacity.
        batch.Completed += (_, _) =>
            DispatcherQueue.TryEnqueue(() => element.Opacity = 1d);

        batch.End();
    }

    private void FadeOutThenCollapse(UIElement element, int durationMs)
    {
        var visual = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;
        var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

        var anim = compositor.CreateScalarKeyFrameAnimation();
        anim.Duration = TimeSpan.FromMilliseconds(durationMs);
        anim.InsertKeyFrame(0f, 1f, compositor.CreateLinearEasingFunction());
        anim.InsertKeyFrame(1f, 0f, compositor.CreateLinearEasingFunction());

        visual.StartAnimation("Opacity", anim);

        batch.Completed += (_, _) =>
            DispatcherQueue.TryEnqueue(() =>
            {
                element.Visibility = Visibility.Collapsed;
                element.Opacity = 1d; // reset so it's ready when shown again
            });

        batch.End();
    }

    // ── Nav-item cache ─────────────────────────────────────────────────────
    private void BuildNavItemCache()
    {
        _navItemCache.Clear();

        void Collect(IList<object> items)
        {
            foreach (var item in items)
                if (item is NavigationViewItem nvi && nvi.Tag is string tag)
                    _navItemCache[tag] = nvi;
        }

        Collect(NavView.MenuItems);
        Collect(NavView.FooterMenuItems);
    }

    // ── NavigationView selection ────────────────────────────────────────────
    // IsSettingsVisible="False" in XAML means args.IsSettingsSelected is
    // never true — Settings is a regular FooterMenuItems entry handled below.
    private void NavView_SelectionChanged(NavigationView sender,
                                          NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer is NavigationViewItem item
            && item.Tag is string tag
            && PageMap.TryGetValue(tag, out var pageType))
        {
            ContentFrame.Navigate(pageType);
        }
    }

    // O(1) — single dictionary lookup, no list scan.
    public void NavigateTo(string tag)
    {
        if (_navItemCache.TryGetValue(tag, out var nvi))
        {
            NavView.SelectedItem = nvi;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[MainWindow] NavigateTo: unknown tag '{tag}'");
        }
    }

    // ── Logout ─────────────────────────────────────────────────────────────
    private async void LogoutBtn_Click(object sender, RoutedEventArgs e)
    {
        // Guard against double-clicks firing a second LogoutAsync mid-flight.
        LogoutBtn.IsEnabled = false;

        try
        {
            await _auth.LogoutAsync();

            // Start both transitions simultaneously — no arbitrary Task.Delay needed.
            // FadeInElement pre-zeros AuthFrame.Opacity, so there is no flash.
            FadeOutThenCollapse(NavView, durationMs: 200);
            ShowLoginPage();
            FadeInElement(AuthFrame, durationMs: 300);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainWindow] Logout error: {ex.Message}");
            // Force show login even if the logout API call failed.
            try { ShowLoginPage(); } catch { /* best-effort */ }
        }
        finally
        {
            // Re-enable in case the user returns to this window without a full restart.
            LogoutBtn.IsEnabled = true;
        }
    }

    // ── Frame error guards ─────────────────────────────────────────────────
    // CRITICAL FIX: Never throw from NavigationFailed handlers.
    // Throwing from a WinUI event handler causes a STATUS_STOWED_EXCEPTION (0xc000027b)
    // process crash that bypasses Application.UnhandledException entirely.
    // Log and mark handled so the app stays alive.
    private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        e.Handled = true;
        System.Diagnostics.Debug.WriteLine(
            $"[ContentFrame] Navigation failed: {e.SourcePageType?.FullName} — {e.Exception?.Message}");
    }

    private void AuthFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        e.Handled = true;
        System.Diagnostics.Debug.WriteLine(
            $"[AuthFrame] Navigation failed: {e.SourcePageType?.FullName} — {e.Exception?.Message}");
        // If auth navigation fails, fall back to login page directly.
        try { ShowLoginPage(); } catch { }
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private static string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "U";

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1
            ? parts[0][0].ToString().ToUpper()
            : $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }
}