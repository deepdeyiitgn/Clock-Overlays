using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ClockInstaller.Logging;
using ClockInstaller.Models;

namespace ClockInstaller.Services;

/// <summary>
/// Launches the downloaded installer executable, waits for it to complete,
/// and returns the exit code and status.
/// </summary>
public sealed class InstallerService : IInstallerService
{
    private readonly AppLogger _logger;

    public InstallerService(AppLogger logger) => _logger = logger;

    public async Task<InstallationResult> RunInstallerAsync(
        string filePath,
        IProgress<string> progress,
        CancellationToken ct = default)
    {
        _logger.LogInstall($"Launching installer: {filePath}");

        if (!File.Exists(filePath))
        {
            var msg = $"Installer file not found: {filePath}";
            _logger.LogError(msg);
            return new InstallationResult { Success = false, Message = msg };
        }

        var sw = Stopwatch.StartNew();
        progress.Report("Preparing to launch installer…");

        var psi = new ProcessStartInfo(filePath)
        {
            UseShellExecute = true   // Allows UAC prompt if installer requests elevation
        };

        Process? proc = null;
        try
        {
            proc = Process.Start(psi)
                ?? throw new InvalidOperationException("Process.Start returned null.");

            _logger.LogInstall($"Installer process started. PID: {proc.Id}");
            progress.Report($"Installer running (PID {proc.Id})…");

            // Wait for installer to exit, polling for cancellation
            while (!proc.WaitForExit(500))
            {
                ct.ThrowIfCancellationRequested();
                progress.Report($"Installer running (PID {proc.Id})…");
            }

            sw.Stop();
            int code = proc.ExitCode;
            _logger.LogInstall($"Installer exited. Code: {code}  Duration: {sw.Elapsed:mm\\:ss}");

            bool success = code == 0;
            return new InstallationResult
            {
                Success     = success,
                ExitCode    = code,
                Message     = success
                    ? "Installation completed successfully."
                    : $"Installer exited with code {code}.",
                Duration    = sw.Elapsed,
                CompletedAt = DateTime.Now
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogInstall("Installation cancelled by user.");
            try { proc?.Kill(); } catch { }
            return new InstallationResult
            {
                Success  = false, ExitCode = -1,
                Message  = "Installation was cancelled.",
                Duration = sw.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Installer launch failed", ex);
            return new InstallationResult
            {
                Success  = false, ExitCode = -2,
                Message  = $"Failed to launch installer: {ex.Message}",
                Duration = sw.Elapsed
            };
        }
        finally
        {
            proc?.Dispose();
        }
    }
}
