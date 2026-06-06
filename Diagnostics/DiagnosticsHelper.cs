using System;
using System.IO;
using ClockInstaller.Services;
using ClockInstaller.Utilities;

namespace ClockInstaller.Diagnostics;

public static class DiagnosticsHelper
{
    public static void GenerateReport()
    {
        if (!AppServices.State.IncludeDiagnostics) return;
        try
        {
            Directory.CreateDirectory(Constants.LogFolder);
            string file = Path.Combine(Constants.LogFolder, $"Diagnostics_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            using var w = new StreamWriter(file);
            w.WriteLine("=== Clock Overlays Diagnostics Report ===");
            w.WriteLine($"Date: {DateTime.Now}");
            w.WriteLine($"Installer Version: {Constants.AppVersion}");
            
            w.WriteLine("\n--- System Check ---");
            if (AppServices.State.SystemCheckResult != null) {
                foreach(var item in AppServices.State.SystemCheckResult.Items) {
                    w.WriteLine($"[{item.Status}] {item.Name}: {item.Value}");
                }
            }
            
            w.WriteLine("\n--- Installation Details ---");
            w.WriteLine($"Target Version: {AppServices.State.SelectedRelease?.TagName}");
            w.WriteLine($"Asset: {AppServices.State.SelectedAsset?.Name}");
            w.WriteLine($"Success: {AppServices.State.InstallationResult?.Success}");
            w.WriteLine($"Message: {AppServices.State.InstallationResult?.Message}");
        }
        catch { /* Fail silently */ }
    }
}
