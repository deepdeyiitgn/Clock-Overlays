using System.Net.Http;
using ClockInstaller.Logging;
using ClockInstaller.Models;
using ClockInstaller.Utilities;

namespace ClockInstaller.Services;

/// <summary>
/// Simple service locator / bootstrap. Initialises all singletons once at startup.
/// WinForms constructor injection would require passing many objects; this pattern
/// keeps pages clean while retaining a single point of construction.
/// </summary>
public static class AppServices
{
    private static bool _initialised;

    public static AppLogger         Logger        { get; private set; } = null!;
    public static AppState          State         { get; private set; } = null!;
    public static IGitHubService    GitHub        { get; private set; } = null!;
    public static ISystemCheckService SystemCheck { get; private set; } = null!;
    public static IDownloadService  Download      { get; private set; } = null!;
    public static IInstallerService Installer     { get; private set; } = null!;

    /// <summary>Call once from Program.Main before Application.Run.</summary>
    public static void Initialize()
    {
        if (_initialised) return;
        _initialised = true;

        Logger      = new AppLogger();
        State       = new AppState { InstallerVersion = Constants.AppVersion };

        var http = BuildHttpClient();
        GitHub      = new GitHubService(http, Logger);
        SystemCheck = new SystemCheckService(http, Logger);
        Download    = new DownloadService(Logger);
        Installer   = new InstallerService(Logger);
    }

    private static HttpClient BuildHttpClient()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10
        };
        var client = new HttpClient(handler)
        {
            Timeout = System.TimeSpan.FromSeconds(Constants.HttpTimeoutSeconds)
        };
        client.DefaultRequestHeaders.Add("User-Agent", Constants.UserAgentHeader);
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        return client;
    }
}
