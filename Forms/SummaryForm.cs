using System;
using System.Drawing;
using System.Windows.Forms;
using ClockInstaller.Services;
using ClockInstaller.Utilities;

namespace ClockInstaller.Forms;

public class SummaryForm : UserControl
{
    private RichTextBox _rtbSummary;

    public SummaryForm()
    {
        var title = UIHelper.MakeLabel("Installation Summary", 24f, ThemeColors.TextPrimary, true);
        title.Location = new Point(48, 48);

        _rtbSummary = new RichTextBox {
            Location = new Point(48, 100), Size = new Size(760, 320),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = ThemeColors.Surface, ForeColor = ThemeColors.TextPrimary,
            Font = UIHelper.RegularFont(12f), ReadOnly = true, BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };

        Controls.Add(title); Controls.Add(_rtbSummary);
        this.ParentChanged += OnShow;
    }

    private void OnShow(object? s, EventArgs e)
    {
        if (this.Parent == null) return;
        
        // Setup navigation for "Download" button!
        MainForm.Instance.ConfigureNav(true, true, "Start Download", true);

        var r = AppServices.State.SelectedRelease;
        var mainAsset = AppServices.State.SelectedAsset;
        var extras = AppServices.State.OptionalAssetsToDownload;

        string dest = Constants.UserDownloads;
        string txt = "Please review your installation details before proceeding.\n\n";

        txt += "=== CORE INSTALLATION ===\n";
        txt += $"Version:       {r?.TagName ?? "Unknown"}\n";
        txt += $"Installer:     {mainAsset?.Name ?? "Unknown"}\n";
        txt += $"File Size:     {mainAsset?.FormattedSize ?? "0 MB"}\n";
        txt += $"Save Path:     {dest}\n\n";

        if (extras.Count > 0)
        {
            txt += "=== OPTIONAL DOWNLOADS ===\n";
            foreach(var ex in extras)
            {
                txt += $"• {ex.Name}\n";
                txt += $"  Size: {ex.FormattedSize} | Total Downloads: {ex.DownloadCount:N0}\n";
                txt += $"  Published: {r?.PublishedAt?.ToString("MMM dd, yyyy")}\n\n";
            }
            txt += $"Total Extras Pending: {extras.Count} file(s)\n";
        }
        else
        {
            txt += "=== OPTIONAL DOWNLOADS ===\nNone selected.\n";
        }

        _rtbSummary.Text = txt;

        // Apply beautiful styling to the headers
        HighlightText("=== CORE INSTALLATION ===");
        HighlightText("=== OPTIONAL DOWNLOADS ===");
    }

    private void HighlightText(string search)
    {
        int idx = _rtbSummary.Text.IndexOf(search);
        if (idx >= 0) {
            _rtbSummary.Select(idx, search.Length);
            _rtbSummary.SelectionFont = UIHelper.BoldFont(12f);
            _rtbSummary.SelectionColor = ThemeColors.AccentLight;
        }
    }
}
