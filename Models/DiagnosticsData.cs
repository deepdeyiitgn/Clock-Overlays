namespace ClockInstaller.Models;

/// <summary>
/// Diagnostics data collected locally. Only shared if the user explicitly
/// chooses to (e.g. attaching to a GitHub issue or support email).
/// </summary>
public class DiagnosticsData
{
    // Always collected
    public string WindowsVersion    { get; set; } = "";
    public string RamInfo           { get; set; } = "";
    public string DiskInfo          { get; set; } = "";
    public string InstallerVersion  { get; set; } = "";
    public string DownloadStatus    { get; set; } = "";
    public string ErrorMessages     { get; set; } = "";
    public string SelectedRelease   { get; set; } = "";

    // Optional — only if IncludeNetworkInfo == true
    public bool    IncludeNetworkInfo { get; set; }
    public string? PublicIp          { get; set; }
    public string? Country           { get; set; }
    public bool?   VpnDetected       { get; set; }
}
