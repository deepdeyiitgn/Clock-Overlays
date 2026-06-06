using System;

namespace ClockInstaller.Models;

/// <summary>Outcome reported by the installer launch and wait.</summary>
public class InstallationResult
{
    public bool       Success     { get; set; }
    public int        ExitCode    { get; set; }
    public string     Message     { get; set; } = "";
    public DateTime   CompletedAt { get; set; } = DateTime.Now;
    public TimeSpan   Duration    { get; set; }
}
