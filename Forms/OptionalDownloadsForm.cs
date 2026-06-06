using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using ClockInstaller.Controls;
using ClockInstaller.Models;
using ClockInstaller.Services;
using ClockInstaller.Utilities;

namespace ClockInstaller.Forms;

public class OptionalDownloadsForm : UserControl
{
    private AnimatedProgressBar _progress;
    private Label _lblStatus, _lblSpeed, _lblTitle;
    private RoundedButton _btnPause, _btnStop, _btnRestart;
    private CancellationTokenSource? _cts;
    
    private bool _isDownloading = false;
    private bool _isPaused = false;
    private int _currentIndex = 0;
    private string _currentDestPath = "";

    public OptionalDownloadsForm()
    {
        _lblTitle = UIHelper.MakeLabel("Downloading Extras...", 24f, ThemeColors.TextPrimary, true);
        _lblTitle.Location = new Point(48, 48);

        _progress = new AnimatedProgressBar { Location = new Point(48, 160), Size = new Size(760, 20) };
        _lblStatus = UIHelper.MakeLabel("Starting...", 12f, ThemeColors.TextSecondary);
        _lblStatus.Location = new Point(48, 190);
        
        _lblSpeed = UIHelper.MakeLabel("", 12f, ThemeColors.TextDim);
        _lblSpeed.Location = new Point(700, 190);

        _btnPause = new RoundedButton { Text = "Pause", Style = RoundedButton.ButtonStyle.Secondary, Size = new Size(100, 36), Location = new Point(48, 250) };
        _btnRestart = new RoundedButton { Text = "Restart File", Style = RoundedButton.ButtonStyle.Secondary, Size = new Size(130, 36), Location = new Point(160, 250) };
        _btnStop = new RoundedButton { Text = "Stop & Cancel All", Style = RoundedButton.ButtonStyle.Danger, Size = new Size(160, 36), Location = new Point(302, 250) };

        _btnPause.Click += (s, e) => { if (!_isPaused) Pause(); else Resume(); };
        _btnRestart.Click += (s, e) => RestartCurrent();
        _btnStop.Click += (s, e) => CancelAll();

        Controls.Add(_lblTitle); Controls.Add(_progress); Controls.Add(_lblStatus); Controls.Add(_lblSpeed);
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
        
        _currentIndex = 0;
        _isDownloading = false;
        _isPaused = false;
        ResetUI();

        if (AppServices.State.OptionalAssetsToDownload.Count == 0)
        {
            ShowFinishedUI("No optional files selected.", ThemeColors.TextSecondary);
            return;
        }

        StartDownloads();
    }

    private async void StartDownloads()
    {
        if (_isDownloading) return;
        _isDownloading = true;
        _isPaused = false;
        
        _btnPause.Text = "Pause"; _btnPause.Enabled = true; _btnRestart.Enabled = true; _btnStop.Enabled = true;
        MainForm.Instance.ConfigureNav(false);

        var extras = AppServices.State.OptionalAssetsToDownload;

        while (_currentIndex < extras.Count)
        {
            var asset = extras[_currentIndex];
            if (string.IsNullOrEmpty(asset.DownloadUrl)) 
            {
                _currentIndex++;
                continue;
            }

            string fileName = string.IsNullOrEmpty(asset.Name) ? $"ExtraFile_{_currentIndex}.zip" : asset.Name;
            _currentDestPath = Path.Combine(Constants.UserDownloads, fileName);
            
            _cts = new CancellationTokenSource();
            
            var progress = new Progress<DownloadProgress>(p => {
                _progress.Value = p.Percentage;
                _lblStatus.Text = $"File {_currentIndex + 1} of {extras.Count}: {fileName}  —  {p.Percentage:F0}% ({p.FormattedReceived} / {p.FormattedTotal}) ({p.FormattedEta} left)";
                _lblStatus.ForeColor = ThemeColors.TextSecondary;
                _lblSpeed.Text = p.FormattedSpeed;
            });

            try 
            {
                await AppServices.Download.DownloadAsync(asset.DownloadUrl, _currentDestPath, progress, _cts.Token);
                _currentIndex++; // Successfully finished, move to the next file
            } 
            catch (OperationCanceledException) 
            {
                _isDownloading = false;
                _lblStatus.Text = $"Paused on file {_currentIndex + 1} of {extras.Count}: {fileName}";
                _lblStatus.ForeColor = ThemeColors.Warning;
                _lblSpeed.Text = "";
                return; // Exit loop, but preserve the index so it can resume later
            }
            catch (Exception ex) 
            {
                _lblStatus.Text = $"Error on {fileName}: {ex.Message}";
                _lblStatus.ForeColor = ThemeColors.Error;
                _lblSpeed.Text = "";
                _btnPause.Enabled = false;
                
                // If one file fails, pause briefly so the user sees the error, then skip to the next file!
                await System.Threading.Tasks.Task.Delay(2500);
                _currentIndex++;
            }
        }

        _isDownloading = false;
        ShowFinishedUI("All extras downloaded successfully!", ThemeColors.Success);
    }

    private void Pause() { 
        _isPaused = true;
        _cts?.Cancel(); 
        _btnPause.Text = "Resume"; 
    }
    
    private void Resume() { 
        StartDownloads(); 
    }
    
    private void RestartCurrent() { 
        _cts?.Cancel(); 
        if (File.Exists(_currentDestPath)) { File.Delete(_currentDestPath); } 
        _progress.Value = 0; 
        StartDownloads(); 
    }
    
    private void CancelAll() { 
        _cts?.Cancel(); 
        if (File.Exists(_currentDestPath)) { File.Delete(_currentDestPath); } 
        _isDownloading = false;
        ShowFinishedUI("Downloads cancelled.", ThemeColors.Warning);
    }

    private void ShowFinishedUI(string message, Color textColor)
    {
        Controls.Clear();
        
        var title = UIHelper.MakeLabel("Downloads Complete", 24f, ThemeColors.TextPrimary, true);
        title.Location = new Point(48, 48);

        var lbl = UIHelper.MakeLabel(message, 14f, textColor);
        lbl.Location = new Point(48, 120);

        var btnFolder = new RoundedButton { Text = "Open Downloads", Style = RoundedButton.ButtonStyle.Primary, Location = new Point(48, 180), Size = new Size(180, 44) };
        btnFolder.Click += (sender, ev) => System.Diagnostics.Process.Start("explorer.exe", Constants.UserDownloads);
        
        var btnClose = new RoundedButton { Text = "Finish", Style = RoundedButton.ButtonStyle.Ghost, Location = new Point(240, 180), Size = new Size(120, 44) };
        btnClose.Click += (sender, ev) => Application.Exit();
        
        Controls.Add(title); Controls.Add(lbl); Controls.Add(btnFolder); Controls.Add(btnClose);
    }
}
