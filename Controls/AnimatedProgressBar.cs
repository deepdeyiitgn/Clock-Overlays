using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ClockInstaller.Utilities;
using Timer = System.Windows.Forms.Timer; // Add this line

namespace ClockInstaller.Controls;

/// <summary>
/// Custom progress bar with gradient fill and an animated shimmer overlay.
/// Thread-safe: use SetProgress() from any thread.
/// </summary>
public sealed class AnimatedProgressBar : Control
{
    private double  _value      = 0;        // 0–100
    private int     _shimmerX   = -200;
    private readonly Timer _shimmerTimer;
    private int     _cornerRadius = 8;
    private bool    _indeterminate;
    private int     _indeterminateTick;

    public double Value
    {
        get => _value;
        set { _value = Math.Clamp(value, 0, 100); this.SafeInvoke(Invalidate); }
    }

    public bool Indeterminate
    {
        get => _indeterminate;
        set { _indeterminate = value; Invalidate(); }
    }

    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = value; Invalidate(); }
    }

    public AnimatedProgressBar()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint |
            ControlStyles.SupportsTransparentBackColor, true); // <-- Added this flag!

        Height = 10;
        BackColor = Color.Transparent;

        _shimmerTimer = new Timer { Interval = 16 };
        _shimmerTimer.Tick += (_, _) =>
        {
            _shimmerX += 4;
            if (_shimmerX > Width + 200) _shimmerX = -200;
            _indeterminateTick = (_indeterminateTick + 2) % 360;
            Invalidate();
        };
        _shimmerTimer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        UIHelper.SetAntiAlias(g);

        int h = Height, w = Width;
        var bg = new Rectangle(0, 0, w - 1, h - 1);

        // Track background
        using (var bgBrush = new SolidBrush(ThemeColors.ProgressBg))
            UIHelper.FillRoundedRect(g, bgBrush, bg, _cornerRadius);

        if (_indeterminate)
        {
            // Bouncing segment
            double t   = Math.Sin(_indeterminateTick * Math.PI / 180.0) * 0.5 + 0.5;
            int segW   = w / 3;
            int segX   = (int)((w - segW) * t);
            var seg    = new Rectangle(segX, 0, segW, h - 1);
            using var grad = new LinearGradientBrush(
                new Point(0, 0), new Point(w, 0),
                Color.Transparent, Color.Transparent)
            {
                InterpolationColors = new ColorBlend
                {
                    Colors = new[]
                    {
                        Color.Transparent,
                        ThemeColors.Accent,
                        ThemeColors.AccentLight,
                        ThemeColors.Accent,
                        Color.Transparent
                    },
                    Positions = new[] { 0f, 0.2f, 0.5f, 0.8f, 1f }
                }
            };
            g.SetClip(seg);
            using var path = UIHelper.RoundedRect(bg, _cornerRadius);
            g.SetClip(path);
            g.FillRectangle(grad, seg);
            g.ResetClip();
        }
        else if (_value > 0)
        {
            int fillW = Math.Max(2 * _cornerRadius,
                (int)(w * _value / 100.0));
            var fill = new Rectangle(0, 0, fillW - 1, h - 1);

            using var grad = new LinearGradientBrush(
                fill.IsEmpty ? new Rectangle(0, 0, 1, 1) : fill,
                ThemeColors.AccentDark, ThemeColors.AccentLight,
                LinearGradientMode.Horizontal);

            g.SetClip(UIHelper.RoundedRect(bg, _cornerRadius));
            g.FillRectangle(grad, fill);

            // Shimmer overlay
            if (_shimmerX > 0)
            {
                var shimRect = new Rectangle(_shimmerX - 100, 0, 180, h);
                using var shimGrad = new LinearGradientBrush(
                    shimRect.IsEmpty ? new Rectangle(0,0,1,1) : shimRect,
                    Color.Transparent,
                    Color.FromArgb(50, Color.White),
                    LinearGradientMode.Horizontal);
                shimGrad.SetSigmaBellShape(0.5f, 1f);
                g.FillRectangle(shimGrad, shimRect);
            }

            g.ResetClip();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _shimmerTimer.Dispose();
        base.Dispose(disposing);
    }
}
