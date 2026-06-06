using System;
using System.Drawing;
using System.Windows.Forms;
using ClockInstaller.Controls;
using ClockInstaller.Services;
using ClockInstaller.Utilities;

namespace ClockInstaller.Forms;

public class SplashForm : UserControl
{
    public SplashForm()
    {
        var logo = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Size = new Size(120, 120), Location = new Point(380, 130) };
        
        // THE FIX: Loads the local icon file securely, or extracts it from the executable directly!
        try {
            if (System.IO.File.Exists(@"Assets\clock.ico")) {
                logo.Image = new Icon(@"Assets\clock.ico", new Size(128, 128)).ToBitmap();
            } else {
                logo.Image = Icon.ExtractAssociatedIcon(Application.ExecutablePath)?.ToBitmap();
            }
        } catch { }
        
        var title = UIHelper.MakeLabel(Constants.AppName, 24f, ThemeColors.TextPrimary, true);
        title.Location = new Point(345, 270);

        var spinner = new LoadingSpinner { Location = new Point(415, 340), Size = new Size(32, 32) };
        var status = UIHelper.MakeLabel("Initializing services...", 12f, ThemeColors.TextDim);
        status.Location = new Point(370, 390);

        this.Controls.AddRange(new Control[] { logo, title, spinner, status });
        this.Load += InitAsync;
    }

    private async void InitAsync(object? s, EventArgs e)
    {
        MainForm.Instance.ConfigureNav(false);
        try {
            AppServices.State.AllReleases = await AppServices.GitHub.GetReleasesAsync();
            MainForm.Instance.GoNext();
        } catch (Exception ex) {
            MessageBox.Show("Failed to initialize: " + ex.Message);
        }
    }
}
