using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ClockInstaller.Models;

public class GitHubRelease
{
    [JsonProperty("id")]           public long   Id          { get; set; }
    [JsonProperty("tag_name")]     public string? TagName    { get; set; }
    [JsonProperty("name")]         public string? Name       { get; set; }
    [JsonProperty("body")]         public string? Body       { get; set; }
    [JsonProperty("prerelease")]   public bool   Prerelease  { get; set; }
    [JsonProperty("draft")]        public bool   Draft       { get; set; }
    [JsonProperty("published_at")] public DateTime? PublishedAt { get; set; }
    [JsonProperty("html_url")]     public string? HtmlUrl    { get; set; }
    [JsonProperty("zipball_url")]  public string? ZipballUrl { get; set; }
    [JsonProperty("assets")]       public List<ReleaseAsset> Assets { get; set; } = new();

    public string VersionLabel => !string.IsNullOrEmpty(TagName) ? TagName : (!string.IsNullOrEmpty(Name) ? Name : "Unknown Version");
    public string Channel      => Prerelease ? "Beta" : "Stable";
    public string DisplayName  => $"{VersionLabel}  •  {Channel}  •  {(PublishedAt.HasValue ? PublishedAt.Value.ToString("MMM dd, yyyy") : "No Date")}";

    public ReleaseAsset? PrimaryAsset =>
        Assets.FirstOrDefault(a => (a.Name ?? "").EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        ?? Assets.FirstOrDefault();

    public long TotalDownloads => Assets.Sum(a => (long)a.DownloadCount);

    // FIX: Dynamically adds the GitHub Source Code Zip to the list of available files!
    public List<ReleaseAsset> GetAllAssets()
    {
        var list = new List<ReleaseAsset>(Assets);
        if (!string.IsNullOrEmpty(ZipballUrl))
        {
            list.Add(new ReleaseAsset {
                Id = -1, // Use -1 to identify auto-generated source code
                Name = $"{VersionLabel}-SourceCode.zip",
                DownloadUrl = ZipballUrl,
                Size = 0,
                DownloadCount = 0
            });
        }
        return list;
    }

    public override string ToString() => DisplayName; 
}
