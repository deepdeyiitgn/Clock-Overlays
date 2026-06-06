using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ClockInstaller.Utilities;

namespace ClockInstaller.Controls;

/// <summary>
/// A fully custom-painted dark-theme button with rounded corners,
/// hover + press states, optional leading icon, and smooth rendering.
/// </summary>
public sealed class RoundedButton : Control
{
    // ── Properties ────────────────────────────────────────────────────────────
    private Color _baseColor      = ThemeColors.Accent;
    private Color _hoverColor     = ThemeColors.AccentLight;
    private Color _pressColor     = ThemeColors.AccentDark;
    private Color _textColor      = ThemeColors.TextPrimary;
    private Color _borderColor    = Color.Transparent;
    private int   _cornerRadius   = 10;
    private bool  _isHovered;
    private bool  _isPressed;
    private string _icon          = "";
    private ButtonStyle _style    = ButtonStyle.Primary;

    public enum ButtonStyle { Primary, Secondary, Ghost, Danger }

    public Color BaseColor    { get => _baseColor;    set { _baseColor    = value; Invalidate(); } }
    public Color HoverColor   { get => _hoverColor;   set { _hoverColor   = value; Invalidate(); } }
    public Color PressColor   { get => _pressColor;   set { _pressColor   = value; Invalidate(); } }
    public Color TextColor    { get => _textColor;    set { _textColor    = value; Invalidate(); } }
    public Color BorderColor  { get => _borderColor;  set { _borderColor  = value; Invalidate(); } }
    public int   CornerRadius { get => _cornerRadius; set { _cornerRadius = value; Invalidate(); } }
    public string Icon        { get => _icon;         set { _icon         = value; Invalidate(); } }

    public ButtonStyle Style
    {
        get => _style;
        set
        {
            _style = value;
            ApplyStyle(value);
            Invalidate();
        }
    }

    public RoundedButton()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint |
            ControlStyles.SupportsTransparentBackColor, true);

        Cursor   = Cursors.Hand;
        BackColor= Color.Transparent;
        Font     = UIHelper.BoldFont(13f);
        Size     = new Size(180, 44);
    }

    private void ApplyStyle(ButtonStyle s)
    {
        switch (s)
        {
            case ButtonStyle.Primary:
                _baseColor   = ThemeColors.Accent;
                _hoverColor  = ThemeColors.AccentLight;
                _pressColor  = ThemeColors.AccentDark;
                _textColor   = ThemeColors.TextPrimary;
                _borderColor = Color.Transparent;
                break;
            case ButtonStyle.Secondary:
                _baseColor   = ThemeColors.Card;
                _hoverColor  = ThemeColors.CardHover;
                _pressColor  = ThemeColors.CardActive;
                _textColor   = ThemeColors.TextPrimary;
                _borderColor = ThemeColors.Border;
                break;
            case ButtonStyle.Ghost:
                _baseColor   = Color.Transparent;
                _hoverColor  = Color.FromArgb(30, ThemeColors.Accent);
                _pressColor  = Color.FromArgb(50, ThemeColors.Accent);
                _textColor   = ThemeColors.AccentLight;
                _borderColor = ThemeColors.AccentLight;
                break;
            case ButtonStyle.Danger:
                _baseColor   = ThemeColors.ErrorDim;
                _hoverColor  = ThemeColors.Error;
                _pressColor  = Color.FromArgb(180, 80, 30, 30);
                _textColor   = ThemeColors.TextPrimary;
                _borderColor = ThemeColors.Error;
                break;
        }
    }

    // ── Mouse events ──────────────────────────────────────────────────────────
    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e); _isHovered = true; Invalidate();
    }
    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e); _isHovered = false; _isPressed = false; Invalidate();
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left) { _isPressed = true; Invalidate(); }
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e); _isPressed = false; Invalidate();
    }

    // ── Painting ──────────────────────────────────────────────────────────────
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        UIHelper.SetAntiAlias(g);

        var r = new Rectangle(1, 1, Width - 2, Height - 2);
        Color bg = _isPressed ? _pressColor : _isHovered ? _hoverColor : _baseColor;

        // Background
        using var bgBrush = new SolidBrush(bg);
        UIHelper.FillRoundedRect(g, bgBrush, r, _cornerRadius);

        // Border
        if (_borderColor != Color.Transparent)
        {
            using var borderPen = new Pen(_borderColor, 1.5f);
            UIHelper.DrawRoundedRect(g, borderPen, r, _cornerRadius);
        }

        // Glow effect when hovered (Primary only)
        if (_isHovered && _style == ButtonStyle.Primary)
        {
            using var glow = new SolidBrush(Color.FromArgb(25, ThemeColors.AccentLight));
            UIHelper.FillRoundedRect(g, glow,
                new Rectangle(-2, -2, Width + 3, Height + 3), _cornerRadius + 2);
        }

        // Content layout
        string display = string.IsNullOrEmpty(_icon) ? Text : $"{_icon}  {Text}";
        using var tf = new StringFormat
        {
            Alignment     = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        using var textBrush = new SolidBrush(Enabled ? _textColor : ThemeColors.TextDim);
        g.DrawString(display, Font, textBrush, ClientRectangle, tf);
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData == Keys.Enter || keyData == Keys.Space || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
            OnClick(EventArgs.Empty);
    }
}
