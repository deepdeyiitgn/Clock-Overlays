using System;
using System.Windows.Forms;
using ClockInstaller.Forms;
using ClockInstaller.Logging;
using ClockInstaller.Services;
using ClockInstaller.Utilities;

namespace ClockInstaller;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        ApplicationConfiguration.Initialize();

        Application.ThreadException += (_, e) => {
            AppServices.Logger?.LogError("Unhandled UI thread exception", e.Exception);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, e) => {
            if (e.ExceptionObject is Exception ex) AppServices.Logger?.LogError("Unhandled AppDomain exception", ex);
        };

        AppServices.Initialize();
        AppServices.Logger.LogInstallerInfo($"Clock Installer v{Constants.AppVersion} started");

        var main = new MainForm();
        
        // CRITICAL FIX: The exact correct sequence of pages!
        main.SetPages(new UserControl[] {
            new SplashForm(),
            new SystemCheckForm(),
            new InstallSelectionForm(),
            new ReleaseInfoForm(),
            new DiagnosticsForm(),
            new SummaryForm(),
            new DownloadForm(),
            new InstallationForm(),
            new SuccessForm(),
            new FailureForm(),
            new OptionalDownloadsForm()
        });
        Application.Run(main);

        AppServices.Logger.LogSummary("Application session ended normally.");
        AppServices.Logger.Flush();
    }
}
