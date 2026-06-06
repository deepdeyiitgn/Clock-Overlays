using System;
using System.Threading;
using System.Threading.Tasks;
using ClockInstaller.Models;

namespace ClockInstaller.Services;

public interface IDownloadService
{
    Task<string> DownloadAsync(
        string url,
        string destinationPath,
        IProgress<DownloadProgress> progress,
        CancellationToken ct = default);
}
