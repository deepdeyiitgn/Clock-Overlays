using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ClockInstaller.Controls;
using ClockInstaller.Utilities;
using ClockInstaller.Services;
using ClockInstaller.Diagnostics;
using Timer = System.Windows.Forms.Timer;

namespace ClockInstaller.Forms;

public class SuccessForm : UserControl
{
    private Label _lblAnimated;
    private Timer _typewriter;
    private string _fullText = "Thank you for installing Clock Overlays!\nYour setup is complete and ready to use.";
    private int _typeIndex = 0;

    public SuccessForm() { this.ParentChanged += OnShow; }

    private void OnShow(object? s, EventArgs e)
    {
        if (this.Parent == null) { _typewriter?.Stop(); return; }
        
        var res = AppServices.State.InstallationResult;
        
        // BUG FIX: If installation FAILED, skip the success page and go straight to the Failure page!
        if (res != null && !res.Success) 
        { 
            this.BeginInvoke(new Action(() => MainForm.Instance.GoNext())); 
            return; 
        }

        DiagnosticsHelper.GenerateReport();
        Controls.Clear();

        // FIX: Safe Unicode Checkmark that cannot be corrupted
        var icon = UIHelper.MakeLabel("\u2713", 64f, ThemeColors.Success);
        icon.Location = new Point(400, 40);
        
        _lblAnimated = UIHelper.MakeLabel("", 18f, ThemeColors.TextPrimary, true);
        _lblAnimated.TextAlign = ContentAlignment.MiddleCenter; _lblAnimated.AutoSize = false;
        _lblAnimated.Size = new Size(600, 100); _lblAnimated.Location = new Point(140, 140);

        var btnWeb = new RoundedButton { Text = "Visit Website", Style = RoundedButton.ButtonStyle.Primary, Location = new Point(200, 280) };
        btnWeb.Click += (sender, ev) => Process.Start(new ProcessStartInfo { FileName = Constants.WebsiteUrl, UseShellExecute = true });

        var btnDelete = new RoundedButton { Text = "Delete Setup File", Style = RoundedButton.ButtonStyle.Danger, Location = new Point(480, 280) };
        btnDelete.Click += (sender, ev) => {
            try { if(File.Exists(AppServices.State.DownloadedFilePath)) File.Delete(AppServices.State.DownloadedFilePath!); 
                  btnDelete.Text = "Deleted!"; btnDelete.Style = RoundedButton.ButtonStyle.Ghost; btnDelete.Enabled = false; } catch { btnDelete.Text = "In Use"; }
        };

        Controls.Add(icon); Controls.Add(_lblAnimated); Controls.Add(btnWeb); Controls.Add(btnDelete);
        
        bool hasExtras = AppServices.State.OptionalAssetsToDownload.Count > 0;
        if (hasExtras) {
            MainForm.Instance.ConfigureNav(true, true, "Next: Download Extras", false);
        } else {
            var btnClose = new RoundedButton { Text = "Finish", Style = RoundedButton.ButtonStyle.Ghost, Location = new Point(340, 360) };
            btnClose.Click += (sender, ev) => Application.Exit();
            Controls.Add(btnClose);
            MainForm.Instance.ConfigureNav(false);
        }

        _typeIndex = 0;
        _typewriter = new Timer { Interval = 35 };
        _typewriter.Tick += (sender, ev) => {
            if (_typeIndex < _fullText.Length) { _lblAnimated.Text += _fullText[_typeIndex]; _typeIndex++; } 
            else _typewriter.Stop();
        };
        _typewriter.Start();
    }
}
