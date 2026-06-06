using System;
using System.Threading;
using System.Threading.Tasks;
using ClockInstaller.Models;

namespace ClockInstaller.Services;

public interface IInstallerService
{
    Task<InstallationResult> RunInstallerAsync(
        string filePath,
        IProgress<string> progress,
        CancellationToken ct = default);
}
