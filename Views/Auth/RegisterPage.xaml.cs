using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PotomacAnalyst.Models;
using PotomacAnalyst.Services;

namespace PotomacAnalyst.Views.Auth;

public sealed partial class RegisterPage : Page
{
    private readonly AuthService _auth;

    public RegisterPage()
    {
        InitializeComponent();
        _auth = App.Services.GetRequiredService<AuthService>();
    }

    private async void RegisterBtn_Click(object sender, RoutedEventArgs e)
    {
        var fullName  = FullNameBox.Text.Trim();
        var email     = EmailBox.Text.Trim();
        var password  = PasswordBox.Password;
        var confirm   = ConfirmPasswordBox.Password;

        if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
        {
            ShowError("Please fill in all fields.");
            return;
        }

        if (password != confirm)
        {
            ShowError("Passwords do not match.");
            return;
        }

        SetLoading(true);
        HideMessages();

        var req = new RegisterRequest
        {
            FullName        = fullName,
            Email           = email,
            Password        = password,
            ConfirmPassword = confirm,
        };

        var (success, error) = await _auth.RegisterAsync(req);
        SetLoading(false);

        if (success)
        {
            ShowSuccess("Account created! You can now sign in.");
            RegisterBtn.IsEnabled = false;
        }
        else
        {
            ShowError(error ?? "Registration failed.");
        }
    }

    private void BackToLoginBtn_Click(object sender, RoutedEventArgs e)
    {
        GetMainWindow()?.ShowLoginPage();
    }

    private void ShowError(string msg)
    {
        ErrorText.Text          = msg;
        ErrorBanner.Visibility  = Visibility.Visible;
        SuccessBanner.Visibility = Visibility.Collapsed;
    }

    private void ShowSuccess(string msg)
    {
        SuccessText.Text         = msg;
        SuccessBanner.Visibility = Visibility.Visible;
        ErrorBanner.Visibility   = Visibility.Collapsed;
    }

    private void HideMessages()
    {
        ErrorBanner.Visibility   = Visibility.Collapsed;
        SuccessBanner.Visibility = Visibility.Collapsed;
    }

    private void SetLoading(bool loading)
    {
        RegisterBtn.IsEnabled  = !loading;
        LoadingBar.Visibility  = loading ? Visibility.Visible : Visibility.Collapsed;
    }

    private MainWindow? GetMainWindow()
        => (Application.Current as App)?.MainAppWindow;
}
