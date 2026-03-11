using Microsoft.UI.Xaml.Input;
using PotomacAnalyst.Services;
using Windows.System;

namespace PotomacAnalyst.Views.Auth;

public sealed partial class LoginPage : Page
{
    private readonly AuthService _auth;

    public LoginPage()
    {
        InitializeComponent();
        _auth = App.Services.GetRequiredService<AuthService>();
        // GlowPulse.Begin() must be called AFTER the page enters the live visual tree.
        // Calling it in the constructor (before Navigate() attaches the page to the Frame)
        // throws a COMException because Storyboard.TargetName resolution requires the
        // named elements to be in the visual tree — crashing the app with a black screen.
        Loaded += (_, _) => GlowPulse.Begin();
    }

    private async void SignInBtn_Click(object sender, RoutedEventArgs e) => await AttemptLogin();

    private async void Input_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter) await AttemptLogin();
    }

    private async Task AttemptLogin()
    {
        var email = EmailBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter your email and password.");
            return;
        }

        SetLoading(true);
        HideError();

        var (success, error) = await _auth.LoginAsync(email, password);
        SetLoading(false);

        if (success)
        {
            // Always try to get profile, but fall back to a minimal one from email
            var profile = await _auth.GetProfileAsync();
            profile ??= new Models.UserProfile { Email = email };

            var win = GetMainWindow();
            if (win is not null)
                win.ShowMainLayout(profile);
        }
        else
        {
            ShowError(error ?? "Login failed. Please check your credentials and try again.");
        }
    }

    private void ForgotBtn_Click(object sender, RoutedEventArgs e)
        => GetMainWindow()?.ShowForgotPasswordPage();

    private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        => GetMainWindow()?.ShowRegisterPage();

    private void ShowError(string msg)
    {
        ErrorText.Text = msg;
        ErrorBanner.Visibility = Visibility.Visible;
    }

    private void HideError() => ErrorBanner.Visibility = Visibility.Collapsed;

    private void SetLoading(bool loading)
    {
        SignInBtn.IsEnabled = !loading;
        LoadingBar.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
    }

    private MainWindow? GetMainWindow() => (Application.Current as App)?.MainAppWindow;
}