using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ClockInstaller.Utilities;
using Timer = System.Windows.Forms.Timer; // Add this line

namespace ClockInstaller.Controls;

/// <summary>
/// An animated spinning arc control used on loading and installation pages.
/// </summary>
public sealed class LoadingSpinner : Control
{
    private float  _angle = 0f;
    private int    _arcLength = 260;
    private Color  _color     = ThemeColors.Accent;
    private Color  _trackColor= ThemeColors.ProgressBg;
    private float  _thickness = 4f;
    private readonly Timer _timer;

    public Color SpinnerColor
    { get => _color; set { _color = value; Invalidate(); } }
    public float Thickness
    { get => _thickness; set { _thickness = value; Invalidate(); } }

    public LoadingSpinner()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint |
            ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Size      = new Size(48, 48);

        _timer = new Timer { Interval = 16 };
        _timer.Tick += (_, _) =>
        {
            _angle = (_angle + 6f) % 360f;
            Invalidate();
        };
        _timer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        UIHelper.SetAntiAlias(g);

        int margin = (int)(_thickness / 2) + 2;
        var rect   = new RectangleF(margin, margin,
            Width  - margin * 2,
            Height - margin * 2);

        // Track
        using var trackPen = new Pen(_trackColor, _thickness);
        g.DrawArc(trackPen, rect, 0, 360);

        // Spinning arc
        using var arcPen = new Pen(_color, _thickness) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        g.DrawArc(arcPen, rect, _angle, _arcLength);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}
