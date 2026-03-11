using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PotomacAnalyst.Services;

namespace PotomacAnalyst.Views.Auth;

public sealed partial class ForgotPasswordPage : Page
{
    private readonly AuthService _auth;

    public ForgotPasswordPage()
    {
        InitializeComponent();
        _auth = App.Services.GetRequiredService<AuthService>();
    }

    private async void ResetBtn_Click(object sender, RoutedEventArgs e)
    {
        var email = EmailBox.Text.Trim();
        if (string.IsNullOrEmpty(email))
        {
            ShowError("Please enter your email address.");
            return;
        }

        SetLoading(true);
        HideMessages();

        var (success, error) = await _auth.ForgotPasswordAsync(email);
        SetLoading(false);

        if (success)
        {
            ShowSuccess("Reset link sent! Check your email inbox.");
            ResetBtn.IsEnabled = false;
        }
        else
        {
            ShowError(error ?? "Could not send reset link. Please try again.");
        }
    }

    private void BackBtn_Click(object sender, RoutedEventArgs e)
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
        ResetBtn.IsEnabled    = !loading;
        LoadingBar.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
    }

    private MainWindow? GetMainWindow()
        => (Application.Current as App)?.MainAppWindow;
}
