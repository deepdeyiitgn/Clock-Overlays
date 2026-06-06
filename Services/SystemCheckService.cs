using System;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ClockInstaller.Logging;
using ClockInstaller.Models;
using ClockInstaller.Utilities;

namespace ClockInstaller.Services;

public sealed class SystemCheckService : ISystemCheckService
{
    private readonly HttpClient _http;
    private readonly AppLogger  _logger;

    public SystemCheckService(HttpClient http, AppLogger logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<SystemCheckResult> RunAllChecksAsync(CancellationToken ct = default)
    {
        var result = new SystemCheckResult();

        result.Items.Add(CheckWindowsVersion());
        result.Items.Add(CheckRam());
        result.Items.Add(CheckDiskSpace());
        result.Items.Add(CheckStorageType());
        result.Items.Add(await CheckInternetAsync(ct));
        result.Items.Add(await CheckGitHubAsync(ct));

        foreach (var item in result.Items)
            _logger.LogSystemInfo($"  [{item.Status}] {item.Name}: {item.Value}" +
                (item.Detail != null ? $" — {item.Detail}" : ""));

        return result;
    }

    private static CheckItem CheckWindowsVersion()
    {
        var os  = Environment.OSVersion;
        var ver = os.Version;
        var str = $"Windows {(ver.Major >= 10 ? (ver.Build >= 22000 ? "11" : "10") : ver.ToString())} ({ver})";

        bool ok = ver.Major >= 10;
        return new CheckItem("Windows Version", "Requires Windows 10 or later", str,
            ok ? CheckStatus.Pass : CheckStatus.Fail,
            ok ? null : "Windows 10 or higher is required.");
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MEMORYSTATUSEX
    {
        public uint  dwLength;
        public uint  dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    private static CheckItem CheckRam()
    {
        long totalMB = 0;
        try {
            var mem = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
            if (GlobalMemoryStatusEx(ref mem)) totalMB = (long)(mem.ullTotalPhys / 1_048_576);
        } catch { }

        string val  = totalMB > 0 ? $"{totalMB / 1024.0:F1} GB" : "Unknown";
        if (totalMB == 0) return new CheckItem("Available RAM", "Min RAM", val, CheckStatus.Warning, "Could not read RAM.");
        if (totalMB < Constants.MinRamMB) return new CheckItem("Available RAM", "Min RAM", val, CheckStatus.Fail, $"Minimum {Constants.MinRamMB / 1024} GB required.");
        if (totalMB < Constants.RecommendedRamMB) return new CheckItem("Available RAM", "Min RAM", val, CheckStatus.Warning, $"Recommended {Constants.RecommendedRamMB / 1024} GB.");
        return new CheckItem("Available RAM", $"Min {Constants.MinRamMB / 1024} GB · Rec {Constants.RecommendedRamMB / 1024} GB", val, CheckStatus.Pass, null);
    }

    private static CheckItem CheckDiskSpace()
    {
        long freeMB = 0;
        string drive = "";
        try {
            string target = Constants.UserDownloads;
            Directory.CreateDirectory(target);
            var di = new DriveInfo(Path.GetPathRoot(target) ?? "C:\\");
            freeMB = di.AvailableFreeSpace / 1_048_576;
            drive  = di.Name;
        } catch { }

        string val = freeMB > 0 ? $"{freeMB / 1024.0:F1} GB free on {drive}" : "Unknown";
        if (freeMB == 0) return new CheckItem("Free Disk Space", "Min Space", val, CheckStatus.Warning, "Could not read disk.");
        if (freeMB < Constants.MinDiskMB) return new CheckItem("Free Disk Space", "Min Space", val, CheckStatus.Fail, $"Need at least {Constants.MinDiskMB} MB free.");
        if (freeMB < Constants.RecommendedDiskMB) return new CheckItem("Free Disk Space", "Min Space", val, CheckStatus.Warning, $"Recommended {Constants.RecommendedDiskMB / 1024} GB.");
        return new CheckItem("Free Disk Space", $"Min {Constants.MinDiskMB} MB · Rec {Constants.RecommendedDiskMB / 1024} GB", val, CheckStatus.Pass, null);
    }

    private static CheckItem CheckStorageType()
    {
        try
        {
            // NEW API: Accurate SSD / NVMe detection
            using var searcher = new ManagementObjectSearcher(@"\\.\ROOT\Microsoft\Windows\Storage", "SELECT MediaType FROM MSFT_PhysicalDisk");
            bool hasSsd = false, hasHdd = false;
            foreach (ManagementObject obj in searcher.Get())
            {
                var mt = obj["MediaType"]?.ToString();
                if (mt == "4") hasSsd = true; // 4 = SSD
                if (mt == "3") hasHdd = true; // 3 = HDD
            }

            if (hasSsd) return new CheckItem("Storage Type", "SSD recommended", "SSD / NVMe Detected", CheckStatus.Pass);
            if (hasHdd) return new CheckItem("Storage Type", "SSD recommended", "HDD Detected", CheckStatus.Warning, "An SSD is recommended for better performance.");
        }
        catch { }

        // Fallback to old Win32 API if permissions block the new one
        try {
            using var searcher = new ManagementObjectSearcher("SELECT MediaType FROM Win32_DiskDrive");
            bool hasSsd = false, hasHdd = false;
            foreach (ManagementObject obj in searcher.Get()) {
                var mt = obj["MediaType"]?.ToString() ?? "";
                if (mt.IndexOf("Solid", StringComparison.OrdinalIgnoreCase) >= 0 || mt.IndexOf("SSD", StringComparison.OrdinalIgnoreCase) >= 0) hasSsd = true;
                else if (mt.IndexOf("Fixed", StringComparison.OrdinalIgnoreCase) >= 0 || mt.IndexOf("HDD", StringComparison.OrdinalIgnoreCase) >= 0) hasHdd = true;
            }
            if (hasSsd) return new CheckItem("Storage Type", "SSD recommended", "SSD Detected", CheckStatus.Pass);
            if (hasHdd) return new CheckItem("Storage Type", "SSD recommended", "HDD / Fixed Disk Detected", CheckStatus.Warning, "An SSD is recommended.");
            return new CheckItem("Storage Type", "SSD recommended", "Unknown", CheckStatus.Warning, "Could not detect storage type.");
        } catch {
            return new CheckItem("Storage Type", "SSD recommended", "Unknown", CheckStatus.Warning, "Query failed.");
        }
    }

    private async Task<CheckItem> CheckInternetAsync(CancellationToken ct)
    {
        try {
            using var req = new HttpRequestMessage(HttpMethod.Head, "https://www.google.com");
            using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            bool ok = (int)resp.StatusCode < 500;
            return new CheckItem("Internet", "Required", ok ? "Connected" : "Unreachable", ok ? CheckStatus.Pass : CheckStatus.Fail, ok ? null : "No connection.");
        } catch {
            return new CheckItem("Internet", "Required", "Disconnected", CheckStatus.Fail, "No connection.");
        }
    }

    private async Task<CheckItem> CheckGitHubAsync(CancellationToken ct)
    {
        try {
            using var resp = await _http.GetAsync("https://api.github.com", ct);
            bool ok = (int)resp.StatusCode < 500;
            return new CheckItem("GitHub API", "Required", ok ? "Accessible" : "Unreachable", ok ? CheckStatus.Pass : CheckStatus.Warning, ok ? null : "API blocked.");
        } catch {
            return new CheckItem("GitHub API", "Required", "Unavailable", CheckStatus.Warning, "API blocked.");
        }
    }
}