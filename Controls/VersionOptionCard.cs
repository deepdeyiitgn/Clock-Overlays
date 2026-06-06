using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ClockInstaller.Utilities;

namespace ClockInstaller.Controls;

/// <summary>
/// Selectable option card used on the Install Selection page.
/// Draws a rounded card with title, description and a selection indicator.
/// </summary>
public sealed class VersionOptionCard : Control
{
    private bool   _selected;
    private bool   _hovered;
    private string _subtitle  = "";
    private string _badge     = "";
    private Color  _badgeColor= ThemeColors.Success;
    private string _icon      = "⬇";

    public bool Selected
    { get => _selected; set { _selected = value; Invalidate(); } }

    public string Subtitle
    { get => _subtitle; set { _subtitle = value; Invalidate(); } }

    public string Badge
    { get => _badge; set { _badge = value; Invalidate(); } }

    public Color BadgeColor
    { get => _badgeColor; set { _badgeColor = value; Invalidate(); } }

    public string CardIcon
    { get => _icon; set { _icon = value; Invalidate(); } }

    public event EventHandler? CardClicked;

    public VersionOptionCard()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint |
            ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Cursor    = Cursors.Hand;
        Size      = new Size(240, 92);
    }

    protected override void OnMouseEnter(EventArgs e)
    { base.OnMouseEnter(e); _hovered = true;  Invalidate(); }
    protected override void OnMouseLeave(EventArgs e)
    { base.OnMouseLeave(e); _hovered = false; Invalidate(); }
    protected override void OnClick(EventArgs e)
    { base.OnClick(e); CardClicked?.Invoke(this, e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        UIHelper.SetAntiAlias(g);

        var bg = new Rectangle(0, 0, Width - 1, Height - 1);
        Color fill = _selected  ? ThemeColors.CardActive
                   : _hovered   ? ThemeColors.CardHover
                   :              ThemeColors.Card;

        using (var fillBrush = new SolidBrush(fill))
            UIHelper.FillRoundedRect(g, fillBrush, bg, 12);

        Color border = _selected ? ThemeColors.Accent : ThemeColors.Border;
        using (var borderPen = new Pen(border, _selected ? 2f : 1f))
            UIHelper.DrawRoundedRect(g, borderPen, bg, 12);

        // Selection dot (top-right)
        int dr = 7, dm = 10;
        var dotRect = new Rectangle(Width - dm - dr * 2, dm, dr * 2, dr * 2);
        if (_selected)
        {
            using var dotBg = new SolidBrush(ThemeColors.Accent);
            g.FillEllipse(dotBg, dotRect);
            using var check = new SolidBrush(ThemeColors.TextPrimary);
            using var sf    = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var cf    = UIHelper.BoldFont(9f);
            g.DrawString("✓", cf, check, dotRect, sf);
        }
        else
        {
            using var dotBorder = new Pen(ThemeColors.Border, 1.5f);
            g.DrawEllipse(dotBorder, dotRect);
        }

        // Icon
        using var iconFont  = UIHelper.TitleFont(22f);
        using var iconBrush = new SolidBrush(_selected ? ThemeColors.AccentLight : ThemeColors.TextSecondary);
        g.DrawString(_icon, iconFont, iconBrush, new RectangleF(14, 18, 36, 36),
            new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

        // Title
        using var titleFont  = UIHelper.BoldFont(14f);
        using var titleBrush = new SolidBrush(ThemeColors.TextPrimary);
        g.DrawString(Text, titleFont, titleBrush, new RectangleF(58, 16, Width - 78, 22));

        // Subtitle
        using var subFont  = UIHelper.RegularFont(11f);
        using var subBrush = new SolidBrush(ThemeColors.TextSecondary);
        g.DrawString(_subtitle, subFont, subBrush, new RectangleF(58, 38, Width - 78, 20));

        // Badge
        if (!string.IsNullOrEmpty(_badge))
        {
            using var badgeFont  = UIHelper.SmallFont(10f);
            using var badgeBrush = new SolidBrush(Color.FromArgb(40, _badgeColor));
            var badgeSf          = new StringFormat { Alignment = StringAlignment.Near };
            var badgeSize        = g.MeasureString(_badge, badgeFont);
            var badgeRect        = new RectangleF(56, 62, badgeSize.Width + 12, 16);
            UIHelper.FillRoundedRect(g, badgeBrush,
                Rectangle.Round(badgeRect), 4);
            using var badgePen = new Pen(Color.FromArgb(80, _badgeColor), 1f);
            UIHelper.DrawRoundedRect(g, badgePen,
                Rectangle.Round(badgeRect), 4);
            using var badgeTextBrush = new SolidBrush(_badgeColor);
            g.DrawString(_badge, badgeFont, badgeTextBrush,
                new RectangleF(badgeRect.X + 6, badgeRect.Y + 1,
                    badgeRect.Width, badgeRect.Height));
        }
    }
}
