using System.Threading;
using System.Threading.Tasks;
using ClockInstaller.Models;

namespace ClockInstaller.Services;

public interface ISystemCheckService
{
    Task<SystemCheckResult> RunAllChecksAsync(CancellationToken ct = default);
}
