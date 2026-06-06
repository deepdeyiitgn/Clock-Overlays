using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ClockInstaller.Controls;
using ClockInstaller.Models;
using ClockInstaller.Services;
using ClockInstaller.Utilities;

namespace ClockInstaller.Forms;

public class DownloadForm : UserControl
{
    private AnimatedProgressBar _progress;
    private Label _lblStatus, _lblSpeed;
    private RoundedButton _btnPause, _btnStop, _btnRestart;
    private CancellationTokenSource? _cts;
    private bool _isDownloading = false;
    private string _destPath = "";

    public DownloadForm()
    {
        var title = UIHelper.MakeLabel("Downloading...", 24f, ThemeColors.TextPrimary, true);
        title.Location = new Point(48, 48);

        _progress = new AnimatedProgressBar { Location = new Point(48, 160), Size = new Size(760, 20) };
        _lblStatus = UIHelper.MakeLabel("Starting...", 12f, ThemeColors.TextSecondary);
        _lblStatus.Location = new Point(48, 190);
        _lblSpeed = UIHelper.MakeLabel("", 12f, ThemeColors.TextDim);
        _lblSpeed.Location = new Point(700, 190);

        _btnPause = new RoundedButton { Text = "Pause", Style = RoundedButton.ButtonStyle.Secondary, Size = new Size(100, 36), Location = new Point(48, 250) };
        _btnRestart = new RoundedButton { Text = "Restart", Style = RoundedButton.ButtonStyle.Secondary, Size = new Size(100, 36), Location = new Point(160, 250) };
        _btnStop = new RoundedButton { Text = "Stop & Cancel", Style = RoundedButton.ButtonStyle.Danger, Size = new Size(130, 36), Location = new Point(272, 250) };

        _btnPause.Click += (s, e) => { if (_isDownloading) Pause(); else Resume(); };
        _btnRestart.Click += (s, e) => Restart();
        _btnStop.Click += (s, e) => CancelAndGoBack();

        Controls.Add(title); Controls.Add(_progress); Controls.Add(_lblStatus); Controls.Add(_lblSpeed);
        Controls.Add(_btnPause); Controls.Add(_btnRestart); Controls.Add(_btnStop);
        this.ParentChanged += OnShow;
    }

    private void ResetUI() {
        _progress.Value = 0; _lblStatus.Text = "Starting..."; _lblStatus.ForeColor = ThemeColors.TextSecondary;
        _lblSpeed.Text = ""; _btnPause.Text = "Pause"; _btnPause.Enabled = false; _btnRestart.Enabled = false; _btnStop.Enabled = false;
    }

    private void OnShow(object? s, EventArgs e)
    {
        if (this.Parent == null) return;
        var asset = AppServices.State.SelectedAsset;
        if (asset == null) return;

        string newDest = Path.Combine(Constants.UserDownloads, string.IsNullOrEmpty(asset.Name) ? "ClockSetup.exe" : asset.Name);
        if (_destPath != newDest || !_isDownloading) ResetUI();
        _destPath = newDest;

        if (File.Exists(_destPath) && !_isDownloading) {
            long existingLength = new FileInfo(_destPath).Length;
            if (existingLength >= asset.Size) {
                _lblStatus.Text = "Download Complete! Click 'Launch Setup'.";
                _lblStatus.ForeColor = ThemeColors.Success;
                MainForm.Instance.ConfigureNav(true, true, "Launch Setup", true);
                return;
            }
        }
        if (!_isDownloading) StartDownload();
    }

    private async void StartDownload()
    {
        if (_isDownloading) return;
        _isDownloading = true;
        _btnPause.Text = "Pause"; _btnPause.Enabled = true; _btnRestart.Enabled = true; _btnStop.Enabled = true;
        MainForm.Instance.ConfigureNav(true, false, "Downloading...", false);
        
        _cts = new CancellationTokenSource();
        var progress = new Progress<DownloadProgress>(p => {
            _progress.Value = p.Percentage;
            // THE FIX: Uses the new FormattedEta property to show time remaining!
            _lblStatus.Text = $"{p.Percentage:F0}%  —  {p.FormattedReceived} / {p.FormattedTotal} ({p.FormattedEta} left)";
            _lblStatus.ForeColor = ThemeColors.TextSecondary;
            _lblSpeed.Text = p.FormattedSpeed;
        });

        try {
            AppServices.State.DownloadedFilePath = await AppServices.Download.DownloadAsync(AppServices.State.SelectedAsset!.DownloadUrl, _destPath, progress, _cts.Token);
            _lblStatus.Text = "Download Complete! Click 'Launch Setup' below.";
            _lblStatus.ForeColor = ThemeColors.Success;
            _btnPause.Enabled = false; _btnRestart.Enabled = false; _btnStop.Enabled = false;
            MainForm.Instance.ConfigureNav(true, true, "Launch Setup", true);
        } 
        catch (OperationCanceledException) { _lblStatus.Text = "Paused."; _lblStatus.ForeColor = ThemeColors.Warning; _lblSpeed.Text = ""; }
        catch (Exception ex) { _lblStatus.Text = "Error: " + ex.Message; _lblStatus.ForeColor = ThemeColors.Error; _lblSpeed.Text = ""; }
        finally { _isDownloading = false; }
    }

    private void Pause() { _cts?.Cancel(); _btnPause.Text = "Resume"; }
    private void Resume() { StartDownload(); }
    private void Restart() { _cts?.Cancel(); if (File.Exists(_destPath)) File.Delete(_destPath); ResetUI(); StartDownload(); }
    private void CancelAndGoBack() { _cts?.Cancel(); if (File.Exists(_destPath)) File.Delete(_destPath); AppServices.State.DownloadedFilePath = null; ResetUI(); MainForm.Instance.GoBack(); }
}
