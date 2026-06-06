using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ClockInstaller.Utilities;

namespace ClockInstaller.Logging;

/// <summary>
/// Structured file-based logger. Writes a single log file with named sections.
/// Thread-safe via a lock object. All sections are flushed atomically.
/// </summary>
public class AppLogger
{
    private readonly string _logFilePath;
    private readonly object _writeLock = new();

    private readonly StringBuilder _secInstaller    = new();
    private readonly StringBuilder _secSystem       = new();
    private readonly StringBuilder _secDiagnostics  = new();
    private readonly StringBuilder _secGitHub       = new();
    private readonly StringBuilder _secDownload     = new();
    private readonly StringBuilder _secInstall      = new();
    private readonly StringBuilder _secSummary      = new();
    private readonly List<string>  _secErrors       = new();

    public string LogFilePath => _logFilePath;

    public AppLogger()
    {
        try
        {
            Directory.CreateDirectory(Constants.LogFolder);
            var ts = DateTime.Now;
            var file = $"Transparent-Clock-Installer-{Constants.AppVersion}-Logs__{ts:yyyy-MM-dd}_{ts:HH-mm-ss}.txt";
            _logFilePath = Path.Combine(Constants.LogFolder, file);
        }
        catch
        {
            // Fall back to %TEMP%
            var ts = DateTime.Now;
            _logFilePath = Path.Combine(
                Path.GetTempPath(),
                $"ClockInstaller-{ts:yyyy-MM-dd_HH-mm-ss}.txt");
        }

        Write(_secInstaller, $"Clock Installer v{Constants.AppVersion}");
        Write(_secInstaller, $"Session started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }

    // ── Section writers ───────────────────────────────────────────────────────
    public void LogInstallerInfo(string msg) { Write(_secInstaller, msg);   Flush(); }
    public void LogSystemInfo   (string msg) { Write(_secSystem, msg);      Flush(); }
    public void LogDiagnostics  (string msg) { Write(_secDiagnostics, msg); Flush(); }
    public void LogGitHub       (string msg) { Write(_secGitHub, msg);      Flush(); }
    public void LogDownload     (string msg) { Write(_secDownload, msg);    Flush(); }
    public void LogInstall      (string msg) { Write(_secInstall, msg);     Flush(); }
    public void LogSummary      (string msg) { Write(_secSummary, msg);     Flush(); }

    public void LogError(string msg, Exception? ex = null)
    {
        lock (_writeLock)
        {
            _secErrors.Add($"{Ts()} ERROR: {msg}");
            if (ex != null)
            {
                _secErrors.Add($"{Ts()}   Type: {ex.GetType().FullName}");
                _secErrors.Add($"{Ts()}   Message: {ex.Message}");
                if (ex.InnerException != null)
                    _secErrors.Add($"{Ts()}   Inner: {ex.InnerException.Message}");
                _secErrors.Add($"{Ts()}   Stack:\n{ex.StackTrace}");
            }
        }
        Flush();
    }

    // ── Flush ─────────────────────────────────────────────────────────────────
    public void Flush()
    {
        lock (_writeLock)
        {
            try { File.WriteAllText(_logFilePath, Build(), Encoding.UTF8); }
            catch { /* Best-effort */ }
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────
    private static string Ts() => $"[{DateTime.Now:HH:mm:ss.fff}]";

    private void Write(StringBuilder sb, string message)
    {
        lock (_writeLock) sb.AppendLine($"{Ts()} {message}");
    }

    private string Build()
    {
        var sb = new StringBuilder();
        void Section(string title, string body)
        {
            sb.AppendLine(new string('=', 70));
            sb.AppendLine($"=== {title} ===");
            sb.AppendLine(new string('=', 70));
            sb.AppendLine(string.IsNullOrWhiteSpace(body) ? "  (no entries)" : body);
            sb.AppendLine();
        }

        Section("Installer Information",      _secInstaller.ToString());
        Section("System Information",         _secSystem.ToString());
        Section("Diagnostics Information",    _secDiagnostics.ToString());
        Section("GitHub Release Information", _secGitHub.ToString());
        Section("Download Activity",          _secDownload.ToString());
        Section("Installation Activity",      _secInstall.ToString());
        Section("Errors",                     string.Join(Environment.NewLine, _secErrors));
        Section("Summary",                    _secSummary.ToString());

        return sb.ToString();
    }
}
