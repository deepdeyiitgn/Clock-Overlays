using System.Drawing;

namespace ClockInstaller.Utilities;

/// <summary>
/// Central colour palette for the dark installer theme. Change once, propagates everywhere.
/// </summary>
public static class ThemeColors
{
    // ── Backgrounds ──────────────────────────────────────────────────────────
    public static readonly Color Background   = Color.FromArgb( 13,  13,  26);   // #0D0D1A
    public static readonly Color Surface      = Color.FromArgb( 22,  22,  42);   // #16162A
    public static readonly Color Card         = Color.FromArgb( 30,  30,  54);   // #1E1E36
    public static readonly Color CardHover    = Color.FromArgb( 37,  37,  72);   // #252548
    public static readonly Color CardActive   = Color.FromArgb( 44,  26,  80);   // #2C1A50

    // ── Accent ───────────────────────────────────────────────────────────────
    public static readonly Color Accent       = Color.FromArgb(124,  58, 237);   // #7C3AED
    public static readonly Color AccentLight  = Color.FromArgb(157, 100, 245);   // #9D64F5
    public static readonly Color AccentDark   = Color.FromArgb( 91,  33, 182);   // #5B21B6
    public static readonly Color AccentGlow   = Color.FromArgb( 50, 124,  58, 237);

    // ── Text ─────────────────────────────────────────────────────────────────
    public static readonly Color TextPrimary  = Color.FromArgb(241, 245, 249);   // #F1F5F9
    public static readonly Color TextSecondary= Color.FromArgb(148, 163, 184);   // #94A3B8
    public static readonly Color TextDim      = Color.FromArgb(100, 116, 139);   // #64748B

    // ── Semantic ─────────────────────────────────────────────────────────────
    public static readonly Color Success      = Color.FromArgb( 34, 197,  94);   // #22C55E
    public static readonly Color SuccessDim   = Color.FromArgb( 20,  83,  45);
    public static readonly Color Warning      = Color.FromArgb(234, 179,   8);   // #EAB308
    public static readonly Color WarningDim   = Color.FromArgb( 92,  66,   2);
    public static readonly Color Error        = Color.FromArgb(239,  68,  68);   // #EF4444
    public static readonly Color ErrorDim     = Color.FromArgb( 90,  21,  21);

    // ── Structure ────────────────────────────────────────────────────────────
    public static readonly Color Border       = Color.FromArgb( 45,  45,  80);   // #2D2D50
    public static readonly Color BorderLight  = Color.FromArgb( 61,  61, 104);   // #3D3D68
    public static readonly Color ProgressBg   = Color.FromArgb( 38,  38,  68);   // #262644
    public static readonly Color Separator    = Color.FromArgb( 35,  35,  62);
}
