using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClockInstaller.Models;

namespace ClockInstaller.Services;

public interface IGitHubService
{
    Task<List<GitHubRelease>> GetReleasesAsync(CancellationToken ct = default);
    Task<bool> CheckApiConnectivityAsync(CancellationToken ct = default);
}
