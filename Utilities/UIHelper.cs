using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ClockInstaller.Utilities;

/// <summary>
/// Static helpers: dark-mode title bar, GDI+ rounded drawing, font factory,
/// thread-safe Control.Invoke, and styled control factory methods.
/// </summary>
public static class UIHelper
{
    // ── Dark title bar (DWM) ─────────────────────────────────────────────────
    [DllImport("dwmapi.dll", PreserveSig = false)]
    private static extern void DwmSetWindowAttribute(
        IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_WIN11 = 20;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_WIN10 = 19;

    /// <summary>Applies the dark title bar to the given form handle.</summary>
    public static void ApplyDarkTitleBar(IntPtr hwnd)
    {
        try
        {
            int dark = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_WIN11,
                ref dark, sizeof(int));
        }
        catch
        {
            try
            {
                int dark = 1;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_WIN10,
                    ref dark, sizeof(int));
            }
            catch { /* Silently ignore on very old Windows */ }
        }
    }

    // ── Thread-safe invoke ───────────────────────────────────────────────────
    public static void SafeInvoke(this Control ctrl, Action action)
    {
        if (ctrl.IsDisposed) return;
        if (ctrl.InvokeRequired)
            ctrl.BeginInvoke(action);
        else
            action();
    }

    // ── GDI+ drawing helpers ─────────────────────────────────────────────────
    public static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void DrawRoundedRect(Graphics g, Pen pen, Rectangle r, int radius)
    {
        using var path = RoundedRect(r, radius);
        g.DrawPath(pen, path);
    }

    public static void FillRoundedRect(Graphics g, Brush brush, Rectangle r, int radius)
    {
        using var path = RoundedRect(r, radius);
        g.FillPath(brush, path);
    }

    public static void SetAntiAlias(Graphics g)
    {
        g.SmoothingMode      = SmoothingMode.AntiAlias;
        g.TextRenderingHint  = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.PixelOffsetMode    = PixelOffsetMode.HighQuality;
        g.InterpolationMode  = InterpolationMode.HighQualityBicubic;
    }

    // ── Font factory ─────────────────────────────────────────────────────────
    public static Font TitleFont(float size = 24f)
        => new("Segoe UI", size, FontStyle.Regular, GraphicsUnit.Pixel);

    public static Font BoldFont(float size = 14f)
        => new("Segoe UI Semibold", size, FontStyle.Regular, GraphicsUnit.Pixel);

    public static Font RegularFont(float size = 13f)
        => new("Segoe UI", size, FontStyle.Regular, GraphicsUnit.Pixel);

    public static Font SmallFont(float size = 11f)
        => new("Segoe UI", size, FontStyle.Regular, GraphicsUnit.Pixel);

    public static Font MonoFont(float size = 11f)
        => new("Consolas", size, FontStyle.Regular, GraphicsUnit.Pixel);

    // ── Styled control builders ───────────────────────────────────────────────
    public static Label MakeLabel(
        string text, float size, Color? color = null, bool bold = false)
    {
        return new Label
        {
            Text      = text,
            Font      = bold ? BoldFont(size) : RegularFont(size),
            ForeColor = color ?? ThemeColors.TextPrimary,
            BackColor = Color.Transparent,
            AutoSize  = true
        };
    }

    public static Panel MakeCard(int radius = 12)
    {
        var p = new Panel
        {
            BackColor = ThemeColors.Card,
            Padding   = new Padding(16)
        };
        p.Paint += (s, e) =>
        {
            var g = e.Graphics;
            SetAntiAlias(g);
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            using var borderBrush = new SolidBrush(ThemeColors.Card);
            FillRoundedRect(g, borderBrush, rect, radius);
            using var borderPen = new Pen(ThemeColors.Border, 1f);
            DrawRoundedRect(g, borderPen, rect, radius);
        };
        p.Region = System.Drawing.Region.FromHrgn(
            CreateRoundRectRgn(0, 0, 1, 1, radius, radius));
        p.Resize += (s, _) =>
        {
            p.Region = System.Drawing.Region.FromHrgn(
                CreateRoundRectRgn(0, 0, p.Width, p.Height, radius, radius));
        };
        return p;
    }

    [DllImport("Gdi32.dll")]
    private static extern IntPtr CreateRoundRectRgn(
        int x1, int y1, int x2, int y2, int cx, int cy);

    public static Panel MakeSeparator(bool horizontal = true)
    {
        var p = new Panel { BackColor = ThemeColors.Separator };
        if (horizontal)
        {
            p.Height = 1;
            p.Dock = DockStyle.Top;
        }
        else
        {
            p.Width = 1;
            p.Dock = DockStyle.Left;
        }
        return p;
    }

    // ── Colour helpers ────────────────────────────────────────────────────────
    public static Color WithAlpha(this Color c, int alpha)
        => Color.FromArgb(alpha, c.R, c.G, c.B);

    public static Color Blend(Color a, Color b, float t) =>
        Color.FromArgb(
            (int)(a.A + (b.A - a.A) * t),
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
}
