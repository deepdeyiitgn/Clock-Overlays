using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ClockInstaller.Logging;
using ClockInstaller.Models;
using ClockInstaller.Utilities;
using Newtonsoft.Json;

namespace ClockInstaller.Services;

public sealed class GitHubService : IGitHubService
{
    private readonly HttpClient  _http;
    private readonly AppLogger   _logger;

    public GitHubService(HttpClient http, AppLogger logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<List<GitHubRelease>> GetReleasesAsync(CancellationToken ct = default)
    {
        _logger.LogGitHub($"Fetching releases from {Constants.GitHubReleasesApi}");
        try
        {
            var json  = await _http.GetStringAsync(Constants.GitHubReleasesApi, ct);
            var list  = JsonConvert.DeserializeObject<List<GitHubRelease>>(json)
                        ?? new List<GitHubRelease>();

            // Filter out draft releases
            list.RemoveAll(r => r.Draft);

            _logger.LogGitHub($"Fetched {list.Count} releases successfully.");
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch GitHub releases", ex);
            throw;
        }
    }

    public async Task<bool> CheckApiConnectivityAsync(CancellationToken ct = default)
    {
        try
        {
            using var response = await _http.GetAsync(
                "https://api.github.com", ct);
            var ok = response.IsSuccessStatusCode
                  || (int)response.StatusCode == 403 // rate-limited but reachable
                  || (int)response.StatusCode == 301;
            _logger.LogGitHub($"GitHub API connectivity: {(ok ? "OK" : "FAIL")} ({(int)response.StatusCode})");
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogGitHub($"GitHub API unreachable: {ex.Message}");
            return false;
        }
    }
}
