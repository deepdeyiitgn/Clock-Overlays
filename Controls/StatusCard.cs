using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ClockInstaller.Models;
using ClockInstaller.Utilities;

namespace ClockInstaller.Controls;

/// <summary>
/// A visually rich card showing a single system check result: name, value,
/// status icon (coloured circle) and optional detail text.
/// </summary>
public sealed class StatusCard : Control
{
    private CheckItem?  _item;
    private bool        _isHovered;

    public CheckItem? Item
    {
        get => _item;
        set { _item = value; Invalidate(); }
    }

    public StatusCard()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint |
            ControlStyles.SupportsTransparentBackColor, true); // <-- Added this flag!
            
        BackColor = Color.Transparent;
        Size      = new Size(260, 84);
    }

    protected override void OnMouseEnter(EventArgs e)
    { base.OnMouseEnter(e); _isHovered = true;  Invalidate(); }
    protected override void OnMouseLeave(EventArgs e)
    { base.OnMouseLeave(e); _isHovered = false; Invalidate(); }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_item == null) return;

        var g = e.Graphics;
        UIHelper.SetAntiAlias(g);

        var bg    = new Rectangle(0, 0, Width - 1, Height - 1);
        Color cardBg = _isHovered ? ThemeColors.CardHover : ThemeColors.Card;

        // Card background
        using (var bgBrush = new SolidBrush(cardBg))
            UIHelper.FillRoundedRect(g, bgBrush, bg, 12);

        // Border — tinted by status
        Color borderCol = _item.Status switch
        {
            CheckStatus.Pass    => Color.FromArgb(60, ThemeColors.Success),
            CheckStatus.Warning => Color.FromArgb(60, ThemeColors.Warning),
            CheckStatus.Fail    => Color.FromArgb(80, ThemeColors.Error),
            _                   => ThemeColors.Border
        };
        using (var borderPen = new Pen(borderCol, 1.5f))
            UIHelper.DrawRoundedRect(g, borderPen, bg, 12);

        // Status colour
        Color statusColor = _item.Status switch
        {
            CheckStatus.Pass    => ThemeColors.Success,
            CheckStatus.Warning => ThemeColors.Warning,
            CheckStatus.Fail    => ThemeColors.Error,
            _                   => ThemeColors.TextDim
        };

        // Status indicator circle (left side)
        int cx = 22, cy = Height / 2;
        int cr = 10;
        var circRect = new Rectangle(cx - cr, cy - cr, cr * 2, cr * 2);
        using (var dimBrush = new SolidBrush(Color.FromArgb(40, statusColor)))
            g.FillEllipse(dimBrush, circRect);
        using (var circPen = new Pen(statusColor, 1.5f))
            g.DrawEllipse(circPen, circRect);

        // Symbol inside circle
        string sym = _item.Status switch
        {
            CheckStatus.Pass    => "✓",
            CheckStatus.Warning => "!",
            CheckStatus.Fail    => "✕",
            _                   => "?"
        };
        using var symFont   = UIHelper.BoldFont(10f);
        using var symBrush  = new SolidBrush(statusColor);
        var symRect = new Rectangle(cx - cr, cy - cr, cr * 2, cr * 2);
        using var sf = new StringFormat
            { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(sym, symFont, symBrush, symRect, sf);

        // Text area
        int tx = 46, ty = 10, tw = Width - tx - 8;

        // Name
        using var nameFont  = UIHelper.BoldFont(13f);
        using var nameBrush = new SolidBrush(ThemeColors.TextPrimary);
        g.DrawString(_item.Name, nameFont, nameBrush,
            new RectangleF(tx, ty, tw, 20));

        // Value
        using var valFont  = UIHelper.RegularFont(12f);
        using var valBrush = new SolidBrush(statusColor);
        g.DrawString(_item.Value, valFont, valBrush,
            new RectangleF(tx, ty + 20, tw, 18));

        // Detail
        if (!string.IsNullOrEmpty(_item.Detail))
        {
            using var detFont  = UIHelper.SmallFont(11f);
            using var detBrush = new SolidBrush(ThemeColors.TextSecondary);
            g.DrawString(_item.Detail, detFont, detBrush,
                new RectangleF(tx, ty + 40, tw, 28));
        }
    }
}
