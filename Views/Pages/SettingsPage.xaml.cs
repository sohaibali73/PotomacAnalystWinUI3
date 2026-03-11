using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PotomacAnalyst.Services;

namespace PotomacAnalyst.Views.Pages;

public sealed partial class SettingsPage : Page
{
    private readonly AuthService    _auth;
    private readonly SessionService _session;
    private readonly Button[]       _settingsNavButtons;

    public SettingsPage()
    {
        InitializeComponent();
        _auth    = App.Services.GetRequiredService<AuthService>();
        _session = App.Services.GetRequiredService<SessionService>();
        _settingsNavButtons = new[] { NavProfile, NavApiKeys, NavAppearance, NavNotifications, NavSecurity, NavAbout };
        Loaded += SettingsPage_Loaded;
    }

    private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Populate from session first (fast)
            var email = _session.GetEmail() ?? string.Empty;
            ProfileEmail.Text = email;
            EmailBox.Text     = email;

            var namePart = email.Contains('@') ? email.Split('@')[0] : "User";
            ProfileName.Text     = namePart.ToUpper();
            ProfileInitials.Text = namePart.Length > 0 ? namePart[0].ToString().ToUpper() : "U";

            // Then load full profile from API
            var profile = await _auth.GetProfileAsync();
            if (profile is not null)
            {
                var display = profile.DisplayName;
                ProfileName.Text     = display.ToUpper();
                ProfileInitials.Text = display.Length > 0 ? display[0].ToString().ToUpper() : "U";
                ProfileEmail.Text    = profile.Email;
                EmailBox.Text        = profile.Email;
                FullNameBox.Text     = profile.Name ?? string.Empty;
            }
        }
        catch { /* Backend unreachable — session values remain */ }
    }

    // ── Settings nav ─────────────────────────────────────────────────────
    private void SettingsNav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int idx))
            SwitchSection(idx);
    }

    private void SwitchSection(int idx)
    {
        ProfileSection.Visibility       = idx == 0 ? Visibility.Visible : Visibility.Collapsed;
        ApiKeysSection.Visibility       = idx == 1 ? Visibility.Visible : Visibility.Collapsed;
        AppearanceSection.Visibility    = idx == 2 ? Visibility.Visible : Visibility.Collapsed;
        NotificationsSection.Visibility = idx == 3 ? Visibility.Visible : Visibility.Collapsed;
        SecuritySection.Visibility      = idx == 4 ? Visibility.Visible : Visibility.Collapsed;
        AboutSection.Visibility         = idx == 5 ? Visibility.Visible : Visibility.Collapsed;

        var yellow      = (SolidColorBrush)Application.Current.Resources["PotomacYellowBrush"];
        var darkGray    = (SolidColorBrush)Application.Current.Resources["PotomacDarkGrayBrush"];
        var transparent = (SolidColorBrush)Application.Current.Resources["TransparentBrush"];
        var secondary   = (SolidColorBrush)Application.Current.Resources["TextSecondaryBrush"];

        for (int i = 0; i < _settingsNavButtons.Length; i++)
        {
            _settingsNavButtons[i].Background = i == idx ? yellow    : transparent;
            _settingsNavButtons[i].Foreground = i == idx ? darkGray  : secondary;
        }
    }

    // ── Profile save ──────────────────────────────────────────────────────
    private async void SaveProfile_Click(object sender, RoutedEventArgs e)
    {
        var name = FullNameBox.Text.Trim();
        var (success, error) = await _auth.UpdateProfileAsync(name, null, null);

        var dlg = new ContentDialog
        {
            Title           = success ? "PROFILE UPDATED" : "UPDATE FAILED",
            Content         = success ? "Your profile has been saved." : error ?? "Unknown error",
            CloseButtonText = "OK",
            XamlRoot        = XamlRoot,
        };
        await dlg.ShowAsync().AsTask();

        if (success && !string.IsNullOrEmpty(name))
        {
            ProfileName.Text     = name.ToUpper();
            ProfileInitials.Text = name[0].ToString().ToUpper();
        }
    }

    // ── API Keys save ─────────────────────────────────────────────────────
    // PUT /auth/me with { claude_api_key, tavily_api_key }
    private async void SaveApiKeys_Click(object sender, RoutedEventArgs e)
    {
        var claudeKey  = ClaudeKeyBox.Password.Trim();
        var tavilyKey  = TavilyKeyBox.Password.Trim();

        if (string.IsNullOrEmpty(claudeKey) && string.IsNullOrEmpty(tavilyKey))
        {
            var warn = new ContentDialog
            {
                Title = "NO KEYS ENTERED", Content = "Please enter at least one API key.",
                CloseButtonText = "OK", XamlRoot = XamlRoot,
            };
            await warn.ShowAsync().AsTask();
            return;
        }

        var (success, error) = await _auth.UpdateProfileAsync(
            null,
            string.IsNullOrEmpty(claudeKey) ? null : claudeKey,
            string.IsNullOrEmpty(tavilyKey) ? null : tavilyKey);

        var dlg = new ContentDialog
        {
            Title           = success ? "API KEYS SAVED" : "SAVE FAILED",
            Content         = success ? "Your API keys have been stored securely." : error ?? "Unknown error",
            CloseButtonText = "OK",
            XamlRoot        = XamlRoot,
        };
        await dlg.ShowAsync().AsTask();
    }
}
