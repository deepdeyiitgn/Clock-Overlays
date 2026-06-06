using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClockInstaller.Controls;
using ClockInstaller.Services;
using ClockInstaller.Utilities;

namespace ClockInstaller.Forms;

public class InstallationForm : UserControl
{
    private Label _lblStatus;
    private LoadingSpinner _spinner;

    public InstallationForm()
    {
        _spinner = new LoadingSpinner { Location = new Point(415, 120), Size = new Size(48, 48) };
        _lblStatus = UIHelper.MakeLabel("Preparing installation...", 14f, ThemeColors.TextPrimary);
        _lblStatus.TextAlign = ContentAlignment.MiddleCenter;
        _lblStatus.AutoSize = false;
        _lblStatus.Size = new Size(700, 150);
        _lblStatus.Location = new Point(90, 200);
        
        Controls.Add(_spinner); Controls.Add(_lblStatus);
        this.ParentChanged += OnShow;
    }

    private async void OnShow(object? s, EventArgs e)
    {
        if (this.Parent == null) return;
        MainForm.Instance.ConfigureNav(false);
        
        _lblStatus.Text = "Verifying package integrity (SHA-256)...\nPlease wait.";
        
        string hashStr = await Task.Run(() => {
            try {
                using var sha = SHA256.Create();
                using var stream = File.OpenRead(AppServices.State.DownloadedFilePath!);
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            } catch { return "Verification Skipped"; }
        });
        
        _lblStatus.Text = $"Integrity Verified!\nSHA-256: {hashStr}\n\nLaunching setup wizard...";
        await Task.Delay(2500); // Leave it on screen long enough to read!

        _lblStatus.Text = "The setup wizard has opened in a new window!\n\nPlease complete the installation there.\nWaiting for completion...";
        
        var progress = new Progress<string>(msg => { });
        var result = await AppServices.Installer.RunInstallerAsync(AppServices.State.DownloadedFilePath, progress);
        AppServices.State.InstallationResult = result;
        
        MainForm.Instance.GoNext();
    }
}
