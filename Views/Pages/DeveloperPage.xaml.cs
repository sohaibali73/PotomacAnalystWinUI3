using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using PotomacAnalyst.Services;

namespace PotomacAnalyst.Views.Pages;

public sealed partial class DeveloperPage : Page
{
    private readonly SessionService _session;
    private readonly ApiService     _api;

    public DeveloperPage()
    {
        InitializeComponent();
        _session = App.Services.GetRequiredService<SessionService>();
        _api     = App.Services.GetRequiredService<ApiService>();
        Loaded  += DeveloperPage_Loaded;
    }

    private async void DeveloperPage_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Session info
            var token = _session.GetToken() ?? "(none)";
            TokenDisplay.Text = token.Length > 80 ? token.Substring(0, 80) + "..." : token;
            AuthStatus.Text   = _session.IsAuthenticated() ? "Yes" : "No";

            // Test API health
            await TestApiHealth();
        }
        catch { /* Backend unreachable — leave defaults */ }
    }

    private async Task TestApiHealth()
    {
        try
        {
            var ok = await _api.IsHealthyAsync();
            // Update status indicator
        }
        catch { }
    }

    private async void TestApi_Click(object sender, RoutedEventArgs e)
    {
        ResponseBox.Text = "Testing connection...";
        try
        {
            var ok = await _api.IsHealthyAsync();
            ResponseBox.Text = ok
                ? "200 OK - Backend is healthy and reachable"
                : "Backend returned non-200 status";
        }
        catch (Exception ex)
        {
            ResponseBox.Text = $"Connection failed: {ex.Message}";
        }
    }

    private async void SendRequest_Click(object sender, RoutedEventArgs e)
    {
        var method   = (HttpMethod.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "GET";
        var endpoint = EndpointBox.Text.Trim();
        if (string.IsNullOrEmpty(endpoint)) return;

        ResponseBox.Text = $"Sending {method} {endpoint}...";
        try
        {
            if (method == "GET")
            {
                var result = await _api.GetAsync<object>(endpoint);
                ResponseBox.Text = result?.ToString() ?? "(empty response)";
            }
            else
            {
                ResponseBox.Text = $"{method} requests require a body — use the chat/AFL pages for testing";
            }
        }
        catch (Exception ex)
        {
            ResponseBox.Text = $"Error: {ex.Message}";
        }
    }
}
