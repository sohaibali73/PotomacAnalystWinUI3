using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PotomacAnalyst.Views.Pages;

public sealed partial class AutopilotPage : Page
{
    public AutopilotPage()
    {
        InitializeComponent();
    }

    private async void CreateTask_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TaskDesc.Text))
        {
            var dlg = new ContentDialog
            {
                Title           = "INPUT REQUIRED",
                Content         = "Please describe the task you want to automate.",
                CloseButtonText = "OK",
                XamlRoot        = XamlRoot,
            };
            await dlg.ShowAsync();
            return;
        }
        // TODO: Create autopilot task via API
    }

    private void EnableAutomation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn) btn.Content = "ENABLED";
    }
}
