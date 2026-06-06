using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using ClockInstaller.Controls;
using ClockInstaller.Utilities;
using ClockInstaller.Services;
using ClockInstaller.Diagnostics;

namespace ClockInstaller.Forms;

public class FailureForm : UserControl
{
    public FailureForm() { this.ParentChanged += OnShow; }
    
    private void OnShow(object? s, EventArgs e)
    {
        if (this.Parent == null) return;
        
        var res = AppServices.State.InstallationResult;

        // BUG FIX: If installation SUCCEEDED, skip the failure page and go straight to the Optional Downloads!
        if (res != null && res.Success) 
        { 
            this.BeginInvoke(new Action(() => MainForm.Instance.GoNext())); 
            return; 
        }
        
        DiagnosticsHelper.GenerateReport();
        
        // FIX: Safe Unicode X that cannot be corrupted
        var icon = UIHelper.MakeLabel("\u2715", 64f, ThemeColors.Error);
        icon.Location = new Point(400, 120);
        var title = UIHelper.MakeLabel("Installation Failed", 24f, ThemeColors.TextPrimary, true);
        title.Location = new Point(330, 210);
        var msg = UIHelper.MakeLabel(res?.Message ?? "Unknown Error", 12f, ThemeColors.TextSecondary);
        msg.Location = new Point(350, 250);

        var btnLog = new RoundedButton { Text = "Open Logs", Style = RoundedButton.ButtonStyle.Secondary, Location = new Point(340, 300) };
        btnLog.Click += (sender, ev) => Process.Start("explorer.exe", Constants.LogFolder);

        Controls.Add(icon); Controls.Add(title); Controls.Add(msg); Controls.Add(btnLog);
        MainForm.Instance.ConfigureNav(true, false, "Failed", true);
    }
}
