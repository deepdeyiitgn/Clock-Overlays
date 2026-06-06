using Newtonsoft.Json;

namespace ClockInstaller.Models;

public class ReleaseAsset
{
    [JsonProperty("id")]                   public long   Id             { get; set; }
    [JsonProperty("name")]                 public string? Name          { get; set; }
    [JsonProperty("size")]                 public long   Size           { get; set; }
    [JsonProperty("download_count")]       public int    DownloadCount  { get; set; }
    [JsonProperty("browser_download_url")] public string? DownloadUrl   { get; set; }

    public string FormattedSize => Id == -1 ? "Dynamic Size" : FormatBytes(Size);

    private static string FormatBytes(long b) => b switch {
        >= 1_073_741_824 => $"{b / 1_073_741_824.0:F2} GB",
        >= 1_048_576     => $"{b / 1_048_576.0:F1} MB",
        >= 1_024         => $"{b / 1_024.0:F1} KB",
        _                => $"{b} B"
    };

    // THE FIX: This forces WinForms Checkboxes to ALWAYS display the file name!
    public override string ToString() => Name ?? "Unknown File";
}
