using System;
namespace ClockInstaller.Models;

public class DownloadProgress
{
    public long BytesReceived { get; set; }
    public long TotalBytes { get; set; }
    public double SpeedBytesPerSecond { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
    public string StatusMessage { get; set; } = "";

    public double Percentage => TotalBytes > 0 ? ((double)BytesReceived / TotalBytes) * 100 : 0;
    
    public string FormattedReceived => FormatBytes(BytesReceived);
    public string FormattedTotal => FormatBytes(TotalBytes);
    public string FormattedSpeed => FormatBytes((long)SpeedBytesPerSecond) + "/s";
    
    // THE FIX: Properly formats the ETA into readable minutes and seconds
    public string FormattedEta {
        get {
            if (EstimatedTimeRemaining.TotalSeconds <= 1) return "almost done";
            if (EstimatedTimeRemaining.TotalHours >= 1) return $"{(int)EstimatedTimeRemaining.TotalHours}h {EstimatedTimeRemaining.Minutes}m";
            return $"{EstimatedTimeRemaining.Minutes}m {EstimatedTimeRemaining.Seconds}s";
        }
    }

    private static string FormatBytes(long b) => b switch {
        >= 1_073_741_824 => $"{b / 1_073_741_824.0:F2} GB",
        >= 1_048_576     => $"{b / 1_048_576.0:F1} MB",
        >= 1_024         => $"{b / 1_024.0:F1} KB",
        _                => $"{b} B"
    };
}
