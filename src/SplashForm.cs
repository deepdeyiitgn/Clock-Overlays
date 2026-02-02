using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

public class SplashForm : Form
{
    private readonly Label factLabel;
    private readonly Timer closeTimer;
    private readonly Timer factTimer;
    private int factIndex;

    private static readonly string[] Facts =
    {
        "Small steps every day build big results.",
        "Focused work beats long hours.",
        "A clear desk helps a clear mind.",
        "Short breaks improve long-term focus.",
        "Consistency is more powerful than intensity.",
        "Start with the task you can finish.",
        "Time blocking reduces decision fatigue.",
        "Distraction-free tools protect attention.",
        "One task at a time is a superpower.",
        "Deep work grows with practice.",
        "Progress > perfection.",
        "The best plan is the one you follow.",
        "A 25-minute sprint beats a 2-hour stall.",
        "Momentum builds motivation.",
        "Small wins reset your energy.",
        "Your future self will thank you for starting.",
        "Focus creates quality.",
        "A calm setup supports calm thinking.",
        "Minimal tools reduce mental clutter.",
        "Rest is part of the work.",
        "The clock is a guide, not a judge.",
        "Clarity comes from action.",
        "You don’t need more time, just fewer distractions.",
        "Plan the work, then work the plan.",
        "A good day starts with a clear first task.",
        "Finish strong, even if you start small.",
        "Short sessions make big projects possible.",
        "Focus is a habit you can train.",
        "Simple systems are easier to keep.",
        "Protect your study hours.",
        "Energy follows attention.",
        "A tidy screen helps a tidy mind.",
        "Checklists reduce stress.",
        "Break big tasks into tiny steps.",
        "Time is most valuable when you guard it.",
        "Good posture helps better thinking.",
        "Silence is a productivity tool.",
        "Small breaks prevent burnout.",
        "Start where you are, use what you have.",
        "The first 5 minutes matter most.",
        "Daily review sharpens focus.",
        "You are allowed to start imperfectly.",
        "When in doubt, simplify.",
        "Your attention is your greatest asset.",
        "A single timer can change your day.",
        "Clock-Overlays stays offline and private.",
        "Built for students and focused work.",
        "A clean overlay keeps time in sight.",
        "Simple tools encourage deep focus.",
        "Work in short bursts, then recover."
    };

    public SplashForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(520, 260);
        BackColor = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 4
        };

        var logo = new PictureBox
        {
            Size = new Size(100, 100),
            SizeMode = PictureBoxSizeMode.Zoom,
            Anchor = AnchorStyles.Top
        };

        logo.Image = TryLoadLogoImage();

        var title = new Label
        {
            Text = "Loading Transparent Clock...",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.Top
        };

        factIndex = GetRandomFactIndex();
        factLabel = new Label
        {
            Text = FormatFact(Facts[factIndex]),
            Font = new Font("Segoe UI", 10, FontStyle.Regular),
            AutoSize = true,
            MaximumSize = new Size(460, 0)
        };

        layout.Controls.Add(logo);
        layout.Controls.Add(title);
        layout.Controls.Add(factLabel);
        Controls.Add(layout);

        closeTimer = new Timer { Interval = 5000 };
        closeTimer.Tick += (_, __) =>
        {
            closeTimer.Stop();
            factTimer.Stop();
            Close();
        };
        closeTimer.Start();

        factTimer = new Timer { Interval = 1200 };
        factTimer.Tick += (_, __) =>
        {
            factIndex = (factIndex + 1) % Facts.Length;
            factLabel.Text = FormatFact(Facts[factIndex]);
        };
        factTimer.Start();
    }

    private static Image? TryLoadLogoImage()
    {
        Image? image = TryLoadLocalLogo();
        if (image != null)
        {
            return image;
        }

        try
        {
            var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            return icon?.ToBitmap();
        }
        catch
        {
            return null;
        }
    }

    private static Image? TryLoadLocalLogo()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string relativePath = Path.Combine("assets", "logos", "logo.png");

        for (int i = 0; i < 5; i++)
        {
            string candidate = Path.Combine(baseDir, relativePath);
            if (File.Exists(candidate))
            {
                try
                {
                    return Image.FromFile(candidate);
                }
                catch
                {
                    return null;
                }
            }

            var parent = Directory.GetParent(baseDir);
            if (parent == null)
            {
                break;
            }

            baseDir = parent.FullName;
        }

        return null;
    }

    private static int GetRandomFactIndex()
    {
        var rng = new Random();
        return rng.Next(Facts.Length);
    }

    private static string FormatFact(string fact)
    {
        return $"Did you know? {fact}";
    }
}
