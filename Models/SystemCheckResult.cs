using System.Collections.Generic;
using System.Linq;

namespace ClockInstaller.Models;

public enum CheckStatus { Pass, Warning, Fail }

/// <summary>Result of a single system requirement check.</summary>
public record CheckItem(
    string Name,
    string Description,
    string Value,
    CheckStatus Status,
    string? Detail = null);

/// <summary>Aggregated result of all system requirement checks.</summary>
public class SystemCheckResult
{
    public List<CheckItem> Items { get; set; } = new();

    /// <summary>True unless at least one check has FAIL status.</summary>
    public bool CanProceed   => Items.All(i => i.Status != CheckStatus.Fail);

    /// <summary>True if any check has WARNING status.</summary>
    public bool HasWarnings  => Items.Any(i => i.Status == CheckStatus.Warning);

    public int PassCount    => Items.Count(i => i.Status == CheckStatus.Pass);
    public int WarningCount => Items.Count(i => i.Status == CheckStatus.Warning);
    public int FailCount    => Items.Count(i => i.Status == CheckStatus.Fail);
}
