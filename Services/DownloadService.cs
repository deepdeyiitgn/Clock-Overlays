using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ClockInstaller.Logging;
using ClockInstaller.Models;

namespace ClockInstaller.Services;

public sealed class DownloadService : IDownloadService
{
    private readonly AppLogger _logger;
    public DownloadService(AppLogger logger) => _logger = logger;

    public async Task<string> DownloadAsync(string url, string destinationPath, IProgress<DownloadProgress> progress, CancellationToken ct = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "ClockInstaller-WinForms");

        long existingLength = 0;
        if (File.Exists(destinationPath))
        {
            existingLength = new FileInfo(destinationPath).Length;
            _logger.LogDownload($"Resuming download from {existingLength} bytes.");
            http.DefaultRequestHeaders.Range = new RangeHeaderValue(existingLength, null);
        }

        using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        
        if (existingLength > 0 && response.StatusCode == System.Net.HttpStatusCode.RequestedRangeNotSatisfiable)
        {
            File.Delete(destinationPath);
            existingLength = 0;
            http.DefaultRequestHeaders.Range = null;
            return await DownloadAsync(url, destinationPath, progress, ct); // Restart
        }

        response.EnsureSuccessStatusCode();

        long totalBytes = (response.Content.Headers.ContentLength ?? 0) + existingLength;
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        
        FileMode mode = existingLength > 0 ? FileMode.Append : FileMode.Create;
        using var file = new FileStream(destinationPath, mode, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        long received = existingLength;
        int read;
        var sw = Stopwatch.StartNew();
        DateTime lastReport = DateTime.UtcNow;
        double speedSmooth = 0;

        while ((read = await stream.ReadAsync(buffer, ct)) > 0)
        {
            await file.WriteAsync(buffer.AsMemory(0, read), ct);
            received += read;

            if ((DateTime.UtcNow - lastReport).TotalMilliseconds < 150) continue;
            lastReport = DateTime.UtcNow;

            double elapsed = sw.Elapsed.TotalSeconds;
            double instantSpeed = elapsed > 0 ? (received - existingLength) / elapsed : 0;
            speedSmooth = speedSmooth == 0 ? instantSpeed : speedSmooth * 0.7 + instantSpeed * 0.3;

            progress.Report(new DownloadProgress {
                BytesReceived = received, TotalBytes = totalBytes,
                SpeedBytesPerSecond = speedSmooth,
                EstimatedTimeRemaining = speedSmooth > 0 ? TimeSpan.FromSeconds((totalBytes - received) / speedSmooth) : TimeSpan.Zero,
                StatusMessage = "Downloading…"
            });
        }
        
        progress.Report(new DownloadProgress { BytesReceived = received, TotalBytes = totalBytes, StatusMessage = "Complete" });
        return destinationPath;
    }
}
