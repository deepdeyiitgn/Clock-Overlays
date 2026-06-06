using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ClockInstaller.Controls;
using ClockInstaller.Utilities;

namespace ClockInstaller.Forms;

public partial class MainForm : Form
{
    public static MainForm Instance { get; private set; } = null!;
    private Panel _content, _bottomNav, _footer;
    private RoundedButton _btnNext, _btnBack, _btnCancel;
    private UserControl[] _pages = Array.Empty<UserControl>();
    private int _currentIndex = 0;

    public MainForm() { Instance = this; InitializeUI(); }
    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); UIHelper.ApplyDarkTitleBar(this.Handle); }

    private void InitializeUI()
    {
        this.Text = Constants.AppName;
        this.Size = new Size(Constants.MainFormWidth, Constants.MainFormHeight);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = ThemeColors.Background;

        _content = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        _bottomNav = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = ThemeColors.Surface, Visible = false };
        
        Color lightBg = Color.FromArgb(248, 250, 252);
        Color darkText = Color.FromArgb(15, 23, 42);
        
        _footer = new Panel { Dock = DockStyle.Bottom, Height = 36, BackColor = lightBg };

        var flpLeft = new FlowLayoutPanel { Dock = DockStyle.Left, AutoSize = true, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(16, 8, 0, 0) };
        var linkCopy = new LinkLabel { Text = $"Copyright © {DateTime.Now.Year} Quicklink x Transparent Clock", LinkColor = darkText, ActiveLinkColor = ThemeColors.AccentDark, VisitedLinkColor = darkText, LinkBehavior = LinkBehavior.HoverUnderline, AutoSize = true, Font = UIHelper.RegularFont(10f) };
        linkCopy.Links.Add(linkCopy.Text.IndexOf("Quicklink"), 9, "https://qlynk.vercel.app/");
        linkCopy.Links.Add(linkCopy.Text.IndexOf("Transparent Clock"), 17, "http://studyclock.vercel.app/");
        linkCopy.LinkClicked += (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = e.Link.LinkData!.ToString(), UseShellExecute = true });
        flpLeft.Controls.Add(linkCopy);

        // FIX: WrapContents = false prevents "of Deep Dey" from being pushed to a hidden second line!
        var flpRight = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 8, 16, 0) };
        var lblMade = new Label { Text = "Made With", ForeColor = Color.FromArgb(71, 85, 105), AutoSize = true, Font = UIHelper.RegularFont(10f), Margin = new Padding(0, 1, 3, 0) };
        
        var pnlHeart = new Panel { Size = new Size(16, 16), Margin = new Padding(1, 2, 1, 0) };
        pnlHeart.Paint += (s, e) => {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = new GraphicsPath();
            path.AddBezier(8, 4, 4, -2, -4, 5, 8, 14);
            path.AddBezier(8, 14, 20, 5, 12, -2, 8, 4);
            e.Graphics.FillPath(Brushes.Crimson, path);
        };

        // FIX: Added "Dey" and updated the link offsets
        var linkDeep = new LinkLabel { Text = "of Deep Dey", LinkColor = darkText, ActiveLinkColor = ThemeColors.AccentDark, VisitedLinkColor = darkText, LinkBehavior = LinkBehavior.HoverUnderline, AutoSize = true, Font = UIHelper.RegularFont(10f), Margin = new Padding(3, 1, 0, 0) };
        linkDeep.Links.Add(3, 8, "https://deepdey.vercel.app/");
        linkDeep.LinkClicked += (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = e.Link.LinkData!.ToString(), UseShellExecute = true });
        
        flpRight.Controls.Add(lblMade); flpRight.Controls.Add(pnlHeart); flpRight.Controls.Add(linkDeep);
        
        _footer.Controls.Add(flpLeft); _footer.Controls.Add(flpRight);
        
        this.Controls.Add(_content); this.Controls.Add(_bottomNav); this.Controls.Add(_footer);
        _footer.BringToFront(); _bottomNav.BringToFront();

        _bottomNav.Controls.Add(UIHelper.MakeSeparator(true));
        _btnCancel = new RoundedButton { Text = "Cancel", Style = RoundedButton.ButtonStyle.Ghost, Location = new Point(48, 18) };
        _btnCancel.Click += (s, e) => this.Close();
        _btnNext = new RoundedButton { Text = "Next", Style = RoundedButton.ButtonStyle.Primary, Location = new Point(_bottomNav.Width - 168, 18), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        _btnNext.Click += (s, e) => GoNext();
        _btnBack = new RoundedButton { Text = "Back", Style = RoundedButton.ButtonStyle.Secondary, Location = new Point(_btnNext.Left - 192, 18), Anchor = AnchorStyles.Top | AnchorStyles.Right };
        _btnBack.Click += (s, e) => GoBack();
        _bottomNav.Controls.Add(_btnCancel); _bottomNav.Controls.Add(_btnBack); _bottomNav.Controls.Add(_btnNext);
    }

    public void SetPages(UserControl[] pages) { _pages = pages; ShowPage(0); }
    public void ShowPage(int index) {
        if (index < 0 || index >= _pages.Length) return;
        _currentIndex = index; _content.Controls.Clear();
        var page = _pages[_currentIndex]; page.Dock = DockStyle.Fill; _content.Controls.Add(page);
    }
    public void GoNext() => ShowPage(_currentIndex + 1);
    public void GoBack() => ShowPage(_currentIndex - 1);
    public void ConfigureNav(bool show, bool enableNext = true, string nextText = "Next", bool enableBack = true) {
        this.SafeInvoke(() => { _bottomNav.Visible = show; _btnNext.Enabled = enableNext; _btnNext.Text = nextText; _btnBack.Enabled = enableBack; });
    }
}
