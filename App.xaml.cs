using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PotomacAnalyst.Services;
using System;

namespace PotomacAnalyst;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>The single MainWindow instance, accessible app-wide.</summary>
    public MainWindow? MainAppWindow { get; private set; }

    public App()
    {
        InitializeComponent();

        // Global last-resort exception handler — prevents silent crashes.
        UnhandledException += OnUnhandledException;

        // Catch exceptions on background threads that WinUI's handler misses.
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            System.Diagnostics.Debug.WriteLine(
                $"[App] AppDomain unhandled: {args.ExceptionObject}");
        };

        // Prevent unobserved Task exceptions from crashing the process.
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            args.SetObserved();
            System.Diagnostics.Debug.WriteLine(
                $"[App] Unobserved task exception: {args.Exception?.Message}");
        };

        // Build dependency injection container
        var services = new ServiceCollection();

        // Register HttpClient as a singleton so all services share one instance
        services.AddSingleton<System.Net.Http.HttpClient>();
        services.AddSingleton<ApiService>();
        services.AddSingleton<SessionService>();
        services.AddSingleton<AuthService>();

        Services = services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            MainAppWindow = new MainWindow();
            MainAppWindow.Activate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] FATAL OnLaunched: {ex}");
            // Create a minimal fallback window so the user sees something
            var fallback = new Window();
            fallback.Title = "Analyst by Potomac — Error";
            fallback.Content = new TextBlock
            {
                Text = $"Startup error:\n{ex.Message}\n\nPlease restart the application.",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(40),
                FontSize = 14,
            };
            fallback.Activate();
        }
    }

    /// <summary>
    /// Catches any exception that would otherwise crash the process
    /// with STATUS_STOWED_EXCEPTION (0xc000027b).
    /// Mark handled so the app stays alive; show login if recovery is possible.
    /// </summary>
    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        System.Diagnostics.Debug.WriteLine($"[App] Unhandled exception caught: {e.Exception}");

        // If the main window is alive, fall back to login
        try { MainAppWindow?.ShowLoginPage(); }
        catch { }
    }
}
