using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

public class AboutForm : Form
{
    public AboutForm()
    {
        Text = "About Transparent Clock";
        Size = new Size(520, 420);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        // ===== LOGO =====
        var logo = new PictureBox
        {
            Size = new Size(120, 120),
            Location = new Point(370, 20),
            SizeMode = PictureBoxSizeMode.Zoom
        };

        try
        {
            using var wc = new WebClient();
            using var stream = wc.OpenRead("https://qlynk.vercel.app/Clock-Overlays.png");
            logo.Image = Image.FromStream(stream);
        }
        catch
        {
            // fail silently (no crash)
        }

        // ===== TEXT =====
        var title = new Label
        {
            Text = "Transparent Clock",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var subtitle = new Label
        {
            Text = "An app by Deep",
            Font = new Font("Segoe UI", 9, FontStyle.Italic),
            AutoSize = true,
            Location = new Point(22, 55)
        };

        var version = new Label
        {
            Text = $"Version: {TransparentClock.AppInfo.CurrentVersion}",
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(22, 75)
        };

        var desc = new Label
        {
            Text =
                "Transparent Clock is a lightweight, always-on-top\n" +
                "desktop clock designed for students and professionals\n" +
                "who value focus, minimalism, and productivity.",
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(20, 115)
        };

        // ===== LINKS =====
        var website = CreateLink(
            "🌐 Website: https://qlynk.vercel.app",
            "https://qlynk.vercel.app", 20, 185);

        var allLinks = CreateLink(
            "🔗 All Links: https://qlynk.vercel.app/alllinks",
            "https://qlynk.vercel.app/alllinks", 20, 215);

        var insta = CreateLink(
            "📸 Instagram: https://qlynk.vercel.app/insta",
            "https://qlynk.vercel.app/insta", 20, 245);

        var yt = CreateLink(
            "▶ YouTube: https://qlynk.vercel.app/yt",
            "https://qlynk.vercel.app/yt", 20, 275);

        var repo = CreateLink(
            "💻 Repo: https://github.com/deepdeyiitgn/Clock-Overlays",
            "https://github.com/deepdeyiitgn/Clock-Overlays", 20, 305);

        // ===== CLOSE =====
        var closeBtn = new Button
        {
            Text = "Close",
            Width = 90,
            Height = 30,
            Location = new Point(400, 340)
        };
        closeBtn.Click += (s, e) => Close();

        // ===== COPYRIGHT (NEW – FADED) =====
        var copyright = new Label
        {
            Text = "© 2026 Deep Dey • All Rights Reserved",
            Font = new Font("Segoe UI", 8),
            ForeColor = Color.Gray,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Bottom,
            Height = 24
        };

        // ===== ADD =====
        Controls.Add(title);
        Controls.Add(subtitle);
        Controls.Add(version);
        Controls.Add(desc);
        Controls.Add(website);
        Controls.Add(allLinks);
        Controls.Add(insta);
        Controls.Add(yt);
        Controls.Add(repo);
        Controls.Add(closeBtn);
        Controls.Add(logo);
        Controls.Add(copyright);
    }

    // ===== LINK HELPER =====
    private LinkLabel CreateLink(string text, string url, int x, int y)
    {
        var link = new LinkLabel
        {
            Text = text,
            AutoSize = true,
            Location = new Point(x, y),
            LinkColor = Color.DeepSkyBlue,
            ActiveLinkColor = Color.DodgerBlue,
            VisitedLinkColor = Color.MediumPurple
        };

        link.Click += (s, e) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show(
                    "Unable to open link in browser.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        };

        return link;
    }
}
