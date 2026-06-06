using System.Collections.Generic;
namespace ClockInstaller.Models;

public class AppState
{
    public string InstallerVersion { get; set; } = "1.0.0";
    public bool IncludeDiagnostics { get; set; }
    public List<GitHubRelease> AllReleases { get; set; } = new();
    public GitHubRelease? SelectedRelease { get; set; }
    public ReleaseAsset? SelectedAsset { get; set; }
    public List<ReleaseAsset> OptionalAssetsToDownload { get; set; } = new();
    public SystemCheckResult? SystemCheckResult { get; set; }
    public string? DownloadedFilePath { get; set; }
    public InstallationResult? InstallationResult { get; set; }
}
