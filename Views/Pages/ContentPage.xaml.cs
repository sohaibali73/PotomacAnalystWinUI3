using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PotomacAnalyst.Views.Pages;

public sealed partial class ContentPage : Page
{
    private readonly Button[] _tabs;

    public ContentPage()
    {
        InitializeComponent();
        _tabs = new[] { TabChat, TabSlides, TabArticles, TabDocuments, TabDashboards, TabSkills, TabAnalytics };
    }

    private void Tab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int idx))
            SwitchTab(idx);
    }

    private void SwitchTab(int idx)
    {
        ChatTab.Visibility       = idx == 0 ? Visibility.Visible : Visibility.Collapsed;
        SlidesTab.Visibility     = idx == 1 ? Visibility.Visible : Visibility.Collapsed;
        ArticlesTab.Visibility   = idx == 2 ? Visibility.Visible : Visibility.Collapsed;
        DocumentsTab2.Visibility = idx == 3 ? Visibility.Visible : Visibility.Collapsed;
        DashboardsTab.Visibility = idx == 4 ? Visibility.Visible : Visibility.Collapsed;
        SkillsTab2.Visibility    = idx == 5 ? Visibility.Visible : Visibility.Collapsed;
        AnalyticsTab.Visibility  = idx == 6 ? Visibility.Visible : Visibility.Collapsed;

        for (int i = 0; i < _tabs.Length; i++)
        {
            _tabs[i].Style = i == idx
                ? (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonActiveStyle"]
                : (Microsoft.UI.Xaml.Style)Application.Current.Resources["TabButtonStyle"];
        }
    }

    private void GenerateContent_Click(object sender, RoutedEventArgs e) { }
    private void CreateSlideDeck_Click(object sender, RoutedEventArgs e)
        => GetMainWindow()?.NavigateTo("DeckGenerator");

    private MainWindow? GetMainWindow()
        => (Application.Current as App)?.MainAppWindow;
}
