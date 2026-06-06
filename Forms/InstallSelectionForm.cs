using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClockInstaller.Controls;
using ClockInstaller.Services;
using ClockInstaller.Utilities;
using ClockInstaller.Models;

namespace ClockInstaller.Forms;

public class InstallSelectionForm : UserControl
{
    private VersionOptionCard _cardStable, _cardBeta, _cardManual;
    private ComboBox _cboManual;

    public InstallSelectionForm()
    {
        Padding = new Padding(48);
        var title = UIHelper.MakeLabel("Select Version", 24f, ThemeColors.TextPrimary, true);
        title.Location = new Point(48, 48);
        
        _cardStable = new VersionOptionCard { Text = "Latest Stable", Subtitle = "Recommended for most users", Badge = "Stable", Location = new Point(48, 120), Selected = true };
        _cardBeta = new VersionOptionCard { Text = "Latest Beta", Subtitle = "Early access to new features", Badge = "Beta", BadgeColor = ThemeColors.Warning, Location = new Point(320, 120) };
        _cardManual = new VersionOptionCard { Text = "Manual Selection", Subtitle = "Choose a specific version", Badge = "Custom", BadgeColor = ThemeColors.TextDim, Location = new Point(48, 230) };

        _cboManual = new ComboBox {
            Location = new Point(320, 260),
            Size = new Size(350, 30),
            Font = UIHelper.RegularFont(14f),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Visible = false,
            DisplayMember = "DisplayName",
            BackColor = ThemeColors.Surface,
            ForeColor = ThemeColors.TextPrimary
        };
        
        _cardStable.CardClicked += (s,e) => SelectCard(_cardStable);
        _cardBeta.CardClicked += (s,e) => SelectCard(_cardBeta);
        _cardManual.CardClicked += (s,e) => SelectCard(_cardManual);
        _cboManual.SelectedIndexChanged += (s,e) => UpdateState();

        Controls.Add(title);
        Controls.Add(_cardStable);
        Controls.Add(_cardBeta);
        Controls.Add(_cardManual);
        Controls.Add(_cboManual);
        
        // THE FIX: Trigger when the page is added to the screen!
        this.ParentChanged += OnShow;
    }

    private void SelectCard(VersionOptionCard c)
    {
        _cardStable.Selected = c == _cardStable;
        _cardBeta.Selected = c == _cardBeta;
        _cardManual.Selected = c == _cardManual;
        _cboManual.Visible = _cardManual.Selected;
        UpdateState();
    }

    private void OnShow(object? s, EventArgs e)
    {
        if (this.Parent == null) return; // Ignore when being removed
        
        _cboManual.Items.Clear();
        var releases = AppServices.State.AllReleases;
        
        if (releases == null || releases.Count == 0)
        {
            _cboManual.Items.Add("No releases found on GitHub!");
            _cboManual.SelectedIndex = 0;
            _cboManual.Enabled = false;
        }
        else
        {
            _cboManual.Enabled = true;
            foreach(var r in releases) _cboManual.Items.Add(r);
            _cboManual.SelectedIndex = 0;
        }
        
        UpdateState();
    }

    private void UpdateState()
    {
        var releases = AppServices.State.AllReleases;
        
        if (releases == null || releases.Count == 0)
        {
            AppServices.State.SelectedRelease = null;
            AppServices.State.SelectedAsset = null;
            MainForm.Instance.ConfigureNav(true, false, "Next", true);
            return;
        }
        
        if (_cardStable.Selected)
            AppServices.State.SelectedRelease = releases.FirstOrDefault(r => !r.Prerelease) ?? releases.FirstOrDefault();
        else if (_cardBeta.Selected)
            AppServices.State.SelectedRelease = releases.FirstOrDefault(r => r.Prerelease) ?? releases.FirstOrDefault();
        else if (_cardManual.Selected && _cboManual.SelectedItem is GitHubRelease r)
            AppServices.State.SelectedRelease = r;
        
        AppServices.State.SelectedAsset = AppServices.State.SelectedRelease?.PrimaryAsset;

        bool isValid = AppServices.State.SelectedRelease != null && AppServices.State.SelectedAsset != null;
        MainForm.Instance.ConfigureNav(true, isValid, "Next", true);
    }
}
