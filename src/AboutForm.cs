using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

public class AboutForm : Form
{
    public AboutForm()
    {
        Text = "About Transparent Clock";
        Size = new Size(480, 420); // thoda bada
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

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
            Text = "Version: v29.01.2026",
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(22, 75)
        };

        var desc = new Label
        {
            Text =
                "Transparent Clock is a lightweight, always-on-top\n" +
                "desktop clock designed for focus, minimalism,\n" +
                "and productivity without distractions.",
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(20, 110)
        };

        // Links
        var website = CreateLink(
            "🌐 Website: https://qlynk.vercel.app",
            "https://qlynk.vercel.app",
            20, 180
        );

        var allLinks = CreateLink(
            "🔗 All Links: https://qlynk.vercel.app/alllinks",
            "https://qlynk.vercel.app/alllinks",
            20, 210
        );

        var insta = CreateLink(
            "📸 Instagram: https://qlynk.vercel.app/insta",
            "https://qlynk.vercel.app/insta",
            20, 240
        );

        var yt = CreateLink(
            "▶ YouTube: https://qlynk.vercel.app/yt",
            "https://qlynk.vercel.app/yt",
            20, 270
        );

        var repo = CreateLink(
            "💻 Repo link: https://github.com/deepdeyiitgn/Clock-Overlays",
            "https://github.com/deepdeyiitgn/Clock-Overlays",
            20, 300
        );

        var closeBtn = new Button
        {
            Text = "Close",
            Width = 90,
            Height = 30,
            Location = new Point(360, 340)
        };
        closeBtn.Click += (s, e) => Close();

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
    }

    // 🔗 Browser-opening LinkLabel (IMPORTANT PART)
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
