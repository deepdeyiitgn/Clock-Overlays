using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClockInstaller.Controls;
using ClockInstaller.Services;
using ClockInstaller.Utilities;

namespace ClockInstaller.Forms;

public class SystemCheckForm : UserControl
{
    private FlowLayoutPanel _layout;
    private LoadingSpinner _spinner;
    private Label _lblDownloads;

    public SystemCheckForm()
    {
        Padding = new Padding(48);
        var title = UIHelper.MakeLabel("System Requirements", 24f, ThemeColors.TextPrimary, true);
        title.Location = new Point(48, 48);

        _spinner = new LoadingSpinner { Location = new Point(400, 200) };
        _layout = new FlowLayoutPanel { Location = new Point(48, 100), Size = new Size(780, 360), Visible = false };
        _lblDownloads = UIHelper.MakeLabel("", 14f, ThemeColors.AccentLight, true);
        _lblDownloads.Location = new Point(48, 480); _lblDownloads.Visible = false;

        Controls.Add(title); Controls.Add(_spinner); Controls.Add(_layout); Controls.Add(_lblDownloads);
        this.Load += RunChecks;
    }

    private async void RunChecks(object? s, EventArgs e)
    {
        MainForm.Instance.ConfigureNav(true, false, "Next", false);
        var result = await AppServices.SystemCheck.RunAllChecksAsync();
        AppServices.State.SystemCheckResult = result;
        long totalDls = AppServices.State.AllReleases?.Sum(r => r.TotalDownloads) ?? 0;

        this.SafeInvoke(() => {
            _spinner.Visible = false; _layout.Visible = true;
            foreach (var item in result.Items) { _layout.Controls.Add(new StatusCard { Item = item, Margin = new Padding(10) }); }
            if (totalDls > 0) { _lblDownloads.Text = $"[ Global Downloads: {totalDls:N0} ]"; _lblDownloads.Visible = true; }
            MainForm.Instance.ConfigureNav(true, result.CanProceed, "Next", false);
        });
    }
}
