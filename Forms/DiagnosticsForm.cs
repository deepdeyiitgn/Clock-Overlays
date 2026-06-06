using System;
using System.Drawing;
using System.Windows.Forms;
using ClockInstaller.Services;
using ClockInstaller.Utilities;
using ClockInstaller.Models;

namespace ClockInstaller.Forms;

public class DiagnosticsForm : UserControl
{
    private CheckBox _chkDiag;
    private CheckedListBox _clbAssets;
    private Label _lblAssetsTitle;

    public DiagnosticsForm()
    {
        var title = UIHelper.MakeLabel("Options & Diagnostics", 24f, ThemeColors.TextPrimary, true);
        title.Location = new Point(48, 48);
        
        _chkDiag = new CheckBox {
            Text = " Include Network and Diagnostic Information to help improve Clock Overlays",
            ForeColor = ThemeColors.TextPrimary, Font = UIHelper.RegularFont(12f),
            AutoSize = true, Location = new Point(48, 110)
        };
        _chkDiag.CheckedChanged += (s,e) => AppServices.State.IncludeDiagnostics = _chkDiag.Checked;

        var btnPrivacy = new LinkLabel { Text = "What data is collected?", LinkColor = ThemeColors.AccentLight, AutoSize = true, Location = new Point(48, 140) };
        btnPrivacy.LinkClicked += (s,e) => new PrivacyPopupForm().ShowDialog();

        _lblAssetsTitle = UIHelper.MakeLabel("Optional Downloads (Select files to download after installation):", 12f, ThemeColors.TextSecondary);
        _lblAssetsTitle.Location = new Point(48, 200);

        _clbAssets = new CheckedListBox {
            Location = new Point(48, 230), Size = new Size(760, 200),
            BackColor = ThemeColors.Surface, ForeColor = ThemeColors.TextPrimary,
            Font = UIHelper.RegularFont(12f), BorderStyle = BorderStyle.None, CheckOnClick = true,
            DisplayMember = "Name" 
        };

        Controls.Add(title);
        Controls.Add(_chkDiag);
        Controls.Add(btnPrivacy);
        Controls.Add(_lblAssetsTitle);
        Controls.Add(_clbAssets);
        
        this.ParentChanged += OnShow;
    }

    private void OnShow(object? s, EventArgs e)
    {
        if (this.Parent == null) {
            AppServices.State.OptionalAssetsToDownload.Clear();
            foreach (ReleaseAsset item in _clbAssets.CheckedItems) { AppServices.State.OptionalAssetsToDownload.Add(item); }
            return;
        }

        MainForm.Instance.ConfigureNav(true, true, "Download & Install", true);

        _clbAssets.Items.Clear();
        var r = AppServices.State.SelectedRelease;
        var mainAsset = AppServices.State.SelectedAsset;

        if (r != null) {
            // FIX: Use GetAllAssets() so the Source Code Zip shows up in the checkboxes!
            foreach(var a in r.GetAllAssets()) {
                if (mainAsset != null && a.Id == mainAsset.Id) continue; 
                _clbAssets.Items.Add(a);
            }
        }

        bool hasExtras = _clbAssets.Items.Count > 0;
        _lblAssetsTitle.Visible = hasExtras;
        _clbAssets.Visible = hasExtras;
    }
}
