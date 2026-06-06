using System;
using System.IO;

namespace ClockInstaller.Utilities;

/// <summary>
/// Application-wide constants. Centralised so a single edit propagates everywhere.
/// </summary>
public static class Constants
{
    // ── Identity ─────────────────────────────────────────────────────────────
    public const string AppName        = "Clock Installer";
    public const string AppVersion     = "1.0.0";
    public const string ProductName    = "Clock Overlays";

    // ── Branding ─────────────────────────────────────────────────────────────
    public const string WebsiteUrl     = "https://deepdey.vercel.app";
    public const string SupportEmail   = "team.deepdey@gmail.com";
    public const string LogoUrl        = "https://clock.qlynk.me/assets/images/logo.png";

    // ── GitHub ───────────────────────────────────────────────────────────────
    public const string RepoOwner         = "deepdeyiitgn";
    public const string RepoName          = "Clock-Overlays";
    public const string GitHubReleasesApi =
        "https://api.github.com/repos/deepdeyiitgn/Clock-Overlays/releases";
    public const string GitHubIssuesUrl   =
        "https://github.com/deepdeyiitgn/Clock-Overlays/issues/new";

    // ── System requirements ──────────────────────────────────────────────────
    public const long MinRamMB          = 2_048;   // 2 GB
    public const long RecommendedRamMB  = 4_096;   // 4 GB
    public const long MinDiskMB         = 512;     // 500 MB
    public const long RecommendedDiskMB = 2_048;   // 2 GB

    // ── Paths ─────────────────────────────────────────────────────────────────
    public static string UserDownloads =>
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) is { } p
            ? Path.Combine(p, "Downloads")
            : Path.GetTempPath();

    public static string LogFolder =>
        Path.Combine(UserDownloads, "ClockOverlays-Logs");

    // ── Networking ───────────────────────────────────────────────────────────
    public const int HttpTimeoutSeconds     = 30;
    public const int DownloadBufferBytes    = 81_920; // 80 KB stream buffer
    public const string UserAgentHeader     = "ClockInstaller/1.0 (+https://deepdey.vercel.app)";

    // ── UI ────────────────────────────────────────────────────────────────────
    public const int MainFormWidth   = 880;
    public const int MainFormHeight  = 640;
    public const int PagePaddingH    = 48;
    public const int PagePaddingV    = 36;
}
