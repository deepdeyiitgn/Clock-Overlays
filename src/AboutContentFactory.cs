using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TransparentClock
{
    public static class AboutContentFactory
    {
        private const string WikiSiteUrl = "https://qlynk.vercel.app";
        private const string DharmanagarWikiUrl = "https://en.wikipedia.org/wiki/Dharmanagar";
        private const string JEEWikiUrl = "https://en.wikipedia.org/wiki/Joint_Entrance_Examination";
        private const string AppLogoUrl = "https://qlynk.vercel.app/Clock-Overlays.png";
        private const string DeveloperPortraitUrl = "https://qlynk.vercel.app/wiki-images/Deep_Dey_New.png";
        private const string QuickLinkLogoUrl = "https://qlynk.vercel.app/wiki-images/Quicklink-logo.png";
        private const string SignatureUrl = "https://qlynk.vercel.app/wiki-images/Deep_Dey_IITK_Image1.jpg";
        private const string SpotifyWidgetUrl = "https://6klabs.com/widget/spotify/1df4b229dbac1d186ee5c0fbf87d4582d7535095a5d3f18824559466bcd8fa7b";
        private const string SpotifyWidgetAltUrl = "https://6klabs.com/widget/spotify/3a527ea5812aee9245441bbee13e33cbff4c5b0a6c15595788db87d68e6d5e7b";
        private const string YoutubeUrl = "https://www.youtube.com/channel/UCrh1Mx5CTTbbkgW5O6iS2Tw";
        private const string GithubUrl = "https://github.com/deepdeyiitgn";
        private const string InstagramUrl = "https://www.instagram.com/deepdey.official/";
        private const string AllLinksUrl = "https://qlynk.vercel.app/alllinks";
        private const string PlaylistStudyMainUrl = "https://open.spotify.com/playlist/6KIXCU0MCMP86td8GmLgxj?si=4c2PhCL5QZaB3zXqzrtgEg";
        private const string PlaylistFavUrl = "https://open.spotify.com/playlist/148O9r4X3UuekPoPY3cs70?si=_tSNPDFyQF6I_3aCjfqmUw";
        private const string PlaylistStudyAltUrl = "https://open.spotify.com/playlist/4WfyY6HWh2tAZpzcIBaqlc?si=PcPVThOfSfW44Z4zc3D1HA";
        private const string PlaylistPersonalUrl = "https://open.spotify.com/playlist/5TDJBbIoYxdv120nwkKeJa?si=MXLP1qCDSDmFDvWHrxPCKw";
        private const string PlaylistDurgaPujaUrl = "https://open.spotify.com/playlist/1COK7ewFoKyCCs5oc11EGE?si=UFEYLBCZR7KjncjEPk_RTw";

        public static Control CreateAboutContent(bool includeCloseButton, Action? onClose)
        {
            var wrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            EnsureBrowserEmulation();

            var browser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true,
                AllowWebBrowserDrop = false,
                IsWebBrowserContextMenuEnabled = false,
                WebBrowserShortcutsEnabled = false
            };

            browser.Navigating += (_, e) =>
            {
                if (e.Url == null)
                {
                    return;
                }

                if (string.Equals(e.Url.Scheme, "about", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                e.Cancel = true;
                OpenExternalLink(e.Url.ToString());
            };

            browser.DocumentText = BuildWikiHtml();
            wrapper.Controls.Add(browser);

            if (includeCloseButton)
            {
                var closePanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.RightToLeft,
                    AutoSize = true,
                    Dock = DockStyle.Bottom,
                    Margin = new Padding(0, 8, 0, 0)
                };

                var closeButton = new Button
                {
                    Text = "Close",
                    Width = 90,
                    Height = 30
                };
                closeButton.Click += (_, __) => onClose?.Invoke();

                closePanel.Controls.Add(closeButton);
                wrapper.Controls.Add(closePanel);
            }

            return wrapper;
        }

        public static Control CreateCompactAboutContent(bool includeCloseButton, Action? onClose)
        {
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                Padding = new Padding(16)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var title = new Label
            {
                Text = "Transparent Clock / Clock Overlays",
                Font = new Font("Times New Roman", 16F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 2)
            };

            var titleRule = new Panel
            {
                Height = 1,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(162, 169, 177),
                Margin = new Padding(0, 0, 0, 6)
            };

            var subtitle = new Label
            {
                Text = "About the developer and the app",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(95, 95, 95),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12)
            };

            var version = new Label
            {
                Text = $"Version: {AppInfo.DisplayVersion}",
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12)
            };

            var imageRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 16)
            };

            var appLogo = BuildPictureBox(120, 120);
            var portrait = BuildPictureBox(120, 120);
            imageRow.Controls.Add(appLogo);
            imageRow.Controls.Add(portrait);

            _ = ImageCacheHelper.LoadIntoPictureBoxAsync(appLogo, AppLogoUrl, "app-logo");
            _ = ImageCacheHelper.LoadIntoPictureBoxAsync(portrait, DeveloperPortraitUrl, "developer-portrait");

            layout.Controls.Add(title);
            layout.Controls.Add(titleRule);
            layout.Controls.Add(subtitle);
            layout.Controls.Add(version);
            layout.Controls.Add(imageRow);

            layout.Controls.Add(BuildSectionHeader("Developer"));
            layout.Controls.Add(BuildParagraph("Deep Dey"));

            layout.Controls.Add(BuildSectionHeader("About the App"));
            layout.Controls.Add(BuildParagraph(
                "Transparent Clock / Clock Overlays keeps a minimal, always-on-top clock visible while a calm dashboard provides Pomodoro sessions, To-Do tracking, Focus Insights, and utilities."));

            layout.Controls.Add(BuildSectionHeader("Links"));
            layout.Controls.Add(BuildLinksPanel());

            if (includeCloseButton)
            {
                var closePanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.RightToLeft,
                    AutoSize = true,
                    Dock = DockStyle.Top,
                    Margin = new Padding(0, 16, 0, 0)
                };

                var closeButton = new Button
                {
                    Text = "Close",
                    Width = 90,
                    Height = 30
                };
                closeButton.Click += (_, __) => onClose?.Invoke();

                closePanel.Controls.Add(closeButton);
                layout.Controls.Add(closePanel);
            }

            scroll.Controls.Add(layout);
            return scroll;
        }

        private static void EnsureBrowserEmulation()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(
                    "Software\\Microsoft\\Internet Explorer\\Main\\FeatureControl\\FEATURE_BROWSER_EMULATION");
                if (key == null)
                {
                    return;
                }

                string exeName = Path.GetFileName(Application.ExecutablePath);
                key.SetValue(exeName, 11001, RegistryValueKind.DWord);
            }
            catch
            {
            }
        }

        private static void OpenExternalLink(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }

        private static string BuildWikiHtml()
        {
            string lead = ExpandText(
                "Deep Dey (born 21 October 2008) is an Indian student from Dharmanagar, Tripura. He is currently an aspirant for the Joint Entrance Examination (JEE) targeting the class of 2027. He is known for documenting his structured study practices and digital projects.",
                2);

            string early = ExpandText(
                "Dey was born in Dharmanagar, North Tripura. He completed his primary and secondary education under the Tripura Board of Secondary Education (TBSE) curriculum at New Shishu Bihar Higher Secondary School. During this formative period, he demonstrated a strong aptitude for mathematics and general sciences, securing over 80% in his Madhyamik (10th Board) examinations.",
                3);

            string academic = ExpandText(
                "Following his matriculation, Dey transitioned to the Central Board of Secondary Education (CBSE) curriculum at Golden Valley High School to better align his studies with the syllabus of national engineering entrance requirements. His academic records indicate a primary focus on Physics, Chemistry, and Mathematics (PCM).",
                3);

            string prep = ExpandText(
                "Dey's preparation methodology for the 2027 Joint Entrance Examination involves a structured daily regimen heavily influenced by the 'Arjuna' and 'Manzil' batches of Physics Wallah. Key aspects include time-blocked study sessions, regular mock examinations, and detailed error analysis.",
                3);

            string creation = ExpandText(
                "In parallel with his studies, Dey maintains a digital log of his academic journey. He records study sessions and occasional tutorials that illustrate problem-solving steps and time management approaches for peers. These materials are offered as examples of habit-based study rather than formal instruction.",
                2);

            string projects = ExpandText(
                "As part of independent learning, Dey explores small web projects and demonstrator code to reinforce practical programming skills and application-level understanding. His flagship project, QuickLink, serves as a central hub for his digital presence.",
                3);

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("  <meta charset=\"utf-8\" />");
            html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
            html.AppendLine("  <title>Deep Dey - Wiki</title>");
            html.AppendLine("  <style>");
            html.AppendLine("    body { margin: 0; background: #ffffff; font-family: sans-serif, Arial, 'Helvetica Neue'; color: #000000; }");
            html.AppendLine("    .wiki-scope { width: 100%; overflow-x: hidden; }");
            html.AppendLine("    .wiki-wrap { min-height: 100vh; background: #ffffff; padding-bottom: 60px; }");
            html.AppendLine("    .wiki-container { max-width: 1150px; margin: 0 auto; display: flex; gap: 24px; align-items: flex-start; padding: 20px 24px 0; }");
            html.AppendLine("    .wiki-main { flex: 1 1 0%; min-width: 0; background: #ffffff; padding: 32px 40px; border: 1px solid #a2a9b1; box-shadow: 0 1px 4px rgba(0,0,0,0.08); line-height: 1.6; font-size: 15px; }");
            html.AppendLine("    .wiki-title { font-family: 'Linux Libertine','Georgia','Times',serif; font-size: 2.1rem; font-weight: 400; margin: 0 0 0.25em; border-bottom: 1px solid #a2a9b1; padding-bottom: 0.25em; color: #000; }");
            html.AppendLine("    .wiki-byline { font-size: 13px; color: #54595d; margin-bottom: 24px; font-style: italic; }");
            html.AppendLine("    .wiki-section { font-family: 'Linux Libertine','Georgia','Times',serif; font-size: 1.6rem; font-weight: 400; margin-top: 1.8em; margin-bottom: 0.6em; border-bottom: 1px solid #a2a9b1; padding-bottom: 0.3em; color: #000; }");
            html.AppendLine("    .wiki-h3 { font-weight: 700; font-size: 1.2rem; margin: 1.4em 0 0.6em 0; color: #000; font-family: sans-serif; }");
            html.AppendLine("    .wiki-p { margin: 0.5em 0 0.8em; text-align: justify; }");
            html.AppendLine("    .wiki-ul { margin-left: 1.6em; margin-bottom: 0.8em; margin-top: 0.5em; }");
            html.AppendLine("    .wiki-li { margin-bottom: 0.4em; }");
            html.AppendLine("    .wiki-toc { border: 1px solid #a2a9b1; padding: 12px 24px; margin-bottom: 24px; background: #f8f9fa; display: inline-block; min-width: 260px; }");
            html.AppendLine("    .wiki-toc-title { text-align: center; font-weight: 700; margin-bottom: 12px; font-size: 14px; }");
            html.AppendLine("    .wiki-infobox-wrap { width: 320px; flex: 0 0 320px; max-width: 320px; }");
            html.AppendLine("    .wiki-infobox { background: #f8f9fa; border: 1px solid #a2a9b1; padding: 12px; box-shadow: 0 2px 5px rgba(0,0,0,0.05); }");
            html.AppendLine("    .wiki-infobox img { max-width: 100%; height: auto; border: 1px solid #c8ccd1; background: #fff; padding: 3px; }");
            html.AppendLine("    .wiki-link { color: #0645ad; text-decoration: none; }");
            html.AppendLine("    .wiki-link:visited { color: #0b0080; }");
            html.AppendLine("    .wiki-link:hover { text-decoration: underline; }");
            html.AppendLine("    * { user-select: none; -webkit-user-select: none; -ms-user-select: none; }");
            html.AppendLine("    img { -webkit-user-drag: none; user-drag: none; }");
            html.AppendLine("    .small-btns { display: flex; gap: 8px; flex-wrap: wrap; justify-content: center; margin-top: 12px; }");
            html.AppendLine("    .small-btn { padding: 4px 10px; border: 1px solid #a2a9b1; background: #fff; color: #0645ad; font-size: 12px; text-decoration: none; font-weight: 600; }");
            html.AppendLine("    .wiki-footer { margin-top: 60px; font-size: 12px; color: #54595d; border-top: 1px solid #a2a9b1; padding-top: 16px; font-style: italic; }");
            html.AppendLine("    @media (max-width: 900px) { .wiki-container { flex-direction: column-reverse; padding: 10px; } .wiki-infobox-wrap { width: 100%; max-width: 100%; margin-bottom: 24px; } .wiki-main { padding: 20px 16px; } }");
            html.AppendLine("  </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body oncontextmenu=\"return false\" ondragstart=\"return false\" onselectstart=\"return false\"> ");
            html.AppendLine("<div class=\"wiki-scope\"><div class=\"wiki-wrap\"><div class=\"wiki-container\">");
            html.AppendLine("<article class=\"wiki-main\">");
            html.AppendLine("  <h1 class=\"wiki-title\">Deep Dey</h1>");
            html.AppendLine("  <div class=\"wiki-byline\">From QuickLink, the free personal encyclopedia</div>");
            html.AppendLine($"  <p class=\"wiki-p\"><strong>Deep Dey</strong> (born 21 October 2008) is an Indian student from <a class=\"wiki-link\" href=\"{DharmanagarWikiUrl}\">Dharmanagar</a>, Tripura. He is currently an aspirant for the <a class=\"wiki-link\" href=\"{JEEWikiUrl}\">Joint Entrance Examination</a> (JEE) targeting the class of 2027. He is known for documenting his structured study practices and digital projects. {lead}</p>");

            html.AppendLine("  <div class=\"wiki-toc\"><div class=\"wiki-toc-title\">Contents</div><ul style=\"list-style:none;padding-left:0;margin:0;font-size:13px;\">");
            html.AppendLine("    <li>1 <a class=\"wiki-link\" href=\"#early-life\">Early life and education</a></li>");
            html.AppendLine("    <li>2 <a class=\"wiki-link\" href=\"#academic-record\">Academic career</a></li>");
            html.AppendLine("    <li>3 <a class=\"wiki-link\" href=\"#preparation\">Examination preparation</a></li>");
            html.AppendLine("    <li>4 <a class=\"wiki-link\" href=\"#content-creation\">Content creation</a></li>");
            html.AppendLine("    <li>5 <a class=\"wiki-link\" href=\"#personal-interests\">Personal interests</a></li>");
            html.AppendLine("    <li style=\"margin-left:1em;\">5.1 Music and curation</li>");
            html.AppendLine("    <li style=\"margin-left:1em;\">5.2 Travel and media</li>");
            html.AppendLine("    <li>6 <a class=\"wiki-link\" href=\"#projects\">Projects</a></li>");
            html.AppendLine("    <li>7 <a class=\"wiki-link\" href=\"#public-presence\">Public presence</a></li>");
            html.AppendLine("    <li>8 <a class=\"wiki-link\" href=\"#about-app\">About this app</a></li>");
            html.AppendLine("    <li>9 <a class=\"wiki-link\" href=\"#references\">References</a></li>");
            html.AppendLine("  </ul></div>");

            html.AppendLine("  <section id=\"early-life\"><div class=\"wiki-section\">Early life and education</div>");
            html.AppendLine($"    <p class=\"wiki-p\">{early}</p>");
            html.AppendLine("    <p class=\"wiki-p\">He completed his early schooling under the Tripura Board of Secondary Education (TBSE) at <strong>New Shishu Bihar Higher Secondary School</strong>. On 16 November 2025, he was honored by the Vice Principal of his former school for securing over 80% in the Madhyamik examinations, a milestone that reinforced his academic discipline.</p>");
            html.AppendLine("    <p class=\"wiki-p\">Later, he transitioned his academic focus toward the Central Board of Secondary Education (CBSE) curriculum at <strong>Golden Valley High School</strong> to better align with national-level engineering entrance requirements.</p>");
            html.AppendLine("  </section>");

            html.AppendLine("  <section id=\"academic-record\"><div class=\"wiki-section\">Academic career</div>");
            html.AppendLine($"    <p class=\"wiki-p\">{academic}</p>");
            html.AppendLine("    <p class=\"wiki-p\">His study logs reflect a rigorous focus on the core sciences. He maintains detailed summaries of subject-wise progress, prioritizing daily revision cycles and cumulative testing to track performance over time. His specific academic targets include <strong>IIT Kharagpur</strong>, <strong>IIT Kanpur</strong>, or <strong>IIT Gandhinagar</strong>.</p>");
            html.AppendLine("  </section>");

            html.AppendLine("  <section id=\"preparation\"><div class=\"wiki-section\">Examination preparation</div>");
            html.AppendLine($"    <p class=\"wiki-p\">{prep}</p>");
            html.AppendLine("    <p class=\"wiki-p\">His strategy is segmented by subject depth:</p>");
            html.AppendLine("    <ul class=\"wiki-ul\">");
            html.AppendLine("      <li class=\"wiki-li\"><strong>Foundation:</strong> Chapters requiring conceptual depth (e.g., Mechanics, Calculus) are covered via the 'Arjuna' batch.</li>");
            html.AppendLine("      <li class=\"wiki-li\"><strong>Coverage:</strong> Topics requiring broad overview or revision (e.g., Thermal Properties) are supplemented via the 'Manzil' marathon series.</li>");
            html.AppendLine("      <li class=\"wiki-li\"><strong>Routine:</strong> His daily schedule begins at 6:00 AM with Physics, followed by Chemistry theory, and concludes with Mathematics practice and revision in the evening.</li>");
            html.AppendLine("    </ul>");
            html.AppendLine("  </section>");

            html.AppendLine("  <section id=\"content-creation\"><div class=\"wiki-section\">Content creation</div>");
            html.AppendLine($"    <p class=\"wiki-p\">{creation}</p>");
            html.AppendLine("    <p class=\"wiki-p\">Originally operating under the handle 'Tarzan The Gamer' with Minecraft content, Dey rebranded to focus on educational vlogs. He adheres to a strict upload schedule (3-4 times per month) to ensure his primary focus remains on JEE preparation.</p>");
            html.AppendLine("  </section>");

            html.AppendLine("  <section id=\"personal-interests\"><div class=\"wiki-section\">Personal interests</div>");
            html.AppendLine("    <div class=\"wiki-h3\">Music and curation</div>");
            html.AppendLine("    <p class=\"wiki-p\">Dey curates music across multiple digital profiles. His listening habits are functional, often utilizing instrumental Lofi beats to aid concentration during long study hours, as well as regional music for cultural events like Durga Puja.</p>");
            html.AppendLine("    <div class=\"wiki-h3\">Notable Playlists</div>");
            html.AppendLine("    <ul class=\"wiki-ul\">");
            html.AppendLine($"      <li class=\"wiki-li\"><strong>Primary Study Compilation:</strong> <a class=\"wiki-link\" href=\"{PlaylistStudyMainUrl}\">View Playlist</a></li>");
            html.AppendLine($"      <li class=\"wiki-li\"><strong>Secondary Focus List:</strong> <a class=\"wiki-link\" href=\"{PlaylistStudyAltUrl}\">View Playlist</a></li>");
            html.AppendLine($"      <li class=\"wiki-li\"><strong>Cultural Selection:</strong> <a class=\"wiki-link\" href=\"{PlaylistDurgaPujaUrl}\">View Playlist</a></li>");
            html.AppendLine($"      <li class=\"wiki-li\"><strong>Personal Favorites:</strong> <a class=\"wiki-link\" href=\"{PlaylistFavUrl}\">View Collection 1</a> &bull; <a class=\"wiki-link\" href=\"{PlaylistPersonalUrl}\">View Collection 2</a></li>");
            html.AppendLine("    </ul>");
            html.AppendLine("    <div class=\"wiki-h3\">Travel and media</div>");
            html.AppendLine("    <p class=\"wiki-p\">Dey has expressed a strong desire to visit London, United Kingdom, influenced by the film <i>Loveyatri</i>. His cinematic preferences also include <i>Satyaprem Ki Katha</i> and the <i>Bhool Bhulaiyaa</i> franchise.</p>");
            html.AppendLine("  </section>");

            html.AppendLine("  <section id=\"projects\"><div class=\"wiki-section\">Projects and technical work</div>");
            html.AppendLine($"    <p class=\"wiki-p\">{projects}</p>");
            html.AppendLine("    <p class=\"wiki-p\">He operates under the handle <i>QuickLink</i> for his web projects. These repositories serve as practical demonstrations of his coding skills, balancing his engineering preparation with technical application.</p>");
            html.AppendLine("  </section>");

            html.AppendLine("  <section id=\"public-presence\"><div class=\"wiki-section\">Public presence and channels</div>");
            html.AppendLine("    <p class=\"wiki-p\">Dey maintains a set of public profiles and a link hub used to publish study content and project updates.</p>");
            html.AppendLine("    <ul class=\"wiki-ul\">");
            html.AppendLine($"      <li class=\"wiki-li\">YouTube — <a class=\"wiki-link\" href=\"{YoutubeUrl}\">{YoutubeUrl}</a></li>");
            html.AppendLine($"      <li class=\"wiki-li\">GitHub — <a class=\"wiki-link\" href=\"{GithubUrl}\">{GithubUrl}</a></li>");
            html.AppendLine($"      <li class=\"wiki-li\">All-links hub — <a class=\"wiki-link\" href=\"{AllLinksUrl}\">{AllLinksUrl}</a></li>");
            html.AppendLine("    </ul>");
            html.AppendLine("  </section>");

            html.AppendLine("  <section id=\"about-app\"><div class=\"wiki-section\">About this app</div>");
            html.AppendLine("    <p class=\"wiki-p\"><strong>Transparent Clock / Clock Overlays</strong> keeps a minimal, always-on-top clock visible while a calm dashboard provides Pomodoro sessions, To-Do tracking, Focus Insights, and utilities. It reduces context switching and keeps attention on the task at hand.</p>");
            html.AppendLine("    <p class=\"wiki-p\">The app is lightweight, privacy-first, and tracking-free. Data is stored locally by default, and optional tools use the internet only when you choose to run them.</p>");
            html.AppendLine("  </section>");

            html.AppendLine("  <section id=\"references\"><div class=\"wiki-section\">References</div>");
            html.AppendLine("    <ol style=\"font-size:13px;padding-left:32px;\">");
            html.AppendLine($"      <li><b>^</b> <a class=\"wiki-link\" href=\"{AllLinksUrl}\">QuickLink Official Hub</a>. Retrieved 2025.</li>");
            html.AppendLine($"      <li><b>^</b> <a class=\"wiki-link\" href=\"{DharmanagarWikiUrl}\">Dharmanagar Geography</a>, Wikipedia. Retrieved 2025.</li>");
            html.AppendLine($"      <li><b>^</b> <a class=\"wiki-link\" href=\"{YoutubeUrl}\">YouTube Channel Statistics</a>, Deep Dey. Retrieved 2025.</li>");
            html.AppendLine("    </ol>");
            html.AppendLine("  </section>");

            html.AppendLine("  <div class=\"wiki-footer\">This page was last edited on 5 December 2025, at 19:52 (IST). Text is available under the Creative Commons Attribution-ShareAlike License; additional terms may apply.</div>");
            html.AppendLine("</article>");

            html.AppendLine("<aside class=\"wiki-infobox-wrap\"><div class=\"wiki-infobox\">");
            html.AppendLine("  <div style=\"font-size:18px;margin-bottom:8px;text-align:center;font-family:'Linux Libertine','Georgia',serif;border-bottom:1px solid #a2a9b1;padding-bottom:6px;\">Deep Dey</div>");
            html.AppendLine($"  <div style=\"text-align:center;padding:4px 0 14px 0;\"><img src=\"{DeveloperPortraitUrl}\" alt=\"Deep Dey portrait\" /><div style=\"font-size:11px;margin-top:4px;\">Dey in 2025</div></div>");
            html.AppendLine($"  <div style=\"margin-bottom:12px;color:#111827;font-size:13px;\"><strong>Born</strong><br/>21 October 2008 (age 17)<br/><a class=\"wiki-link\" href=\"{DharmanagarWikiUrl}\">Dharmanagar</a>, Tripura, India</div>");
            html.AppendLine("  <table style=\"width:100%;font-size:13px;border-collapse:collapse;\"><tbody>");
            html.AppendLine("    <tr><td style=\"font-weight:700;width:35%;padding-right:0.8em;\">Nationality</td><td>Indian</td></tr>");
            html.AppendLine("    <tr><td style=\"font-weight:700;padding-right:0.8em;\">Occupation</td><td>Student</td></tr>");
            html.AppendLine("    <tr><td style=\"font-weight:700;padding-right:0.8em;\">Education</td><td>New Shishu Bihar H.S. School (TBSE)<br/>Golden Valley High School (CBSE)</td></tr>");
            html.AppendLine("    <tr><td style=\"font-weight:700;padding-right:0.8em;\">Family</td><td>Biman Dey (Father)<br/>Jaya Dey (Mother)<br/>Puja Dey (Sister)<br/>Parul Rani Dey (Grandmother)</td></tr>");
            html.AppendLine("    <tr><td style=\"font-weight:700;padding-right:0.8em;\">Targets</td><td><a class=\"wiki-link\" href=\"https://www.iitkgp.ac.in/\">IIT Kharagpur</a><br/><a class=\"wiki-link\" href=\"https://www.iitk.ac.in/\">IIT Kanpur</a><br/><a class=\"wiki-link\" href=\"https://iitgn.ac.in/\">IIT Gandhinagar</a></td></tr>");
            html.AppendLine("  </tbody></table>");
            html.AppendLine($"  <div style=\"margin-top:16px;\"><div style=\"font-weight:700;margin-bottom:6px;\">Signature</div><img src=\"{SignatureUrl}\" alt=\"Signature\" style=\"max-width:140px;width:100%;\" /></div>");
            html.AppendLine("  <div style=\"margin-top:16px;padding:8px;background:#fff;border:1px solid #c8ccd1;\"><div style=\"font-weight:700;font-size:11px;color:#54595d;text-transform:uppercase;\">Favourite Quote</div><div style=\"margin-top:4px;font-style:italic;font-size:12px;\">“✨ Remember: 100% effort + extra 1% = Dream Achieved”</div></div>");
            html.AppendLine("  <div style=\"margin-top:16px;border-top:1px solid #a2a9b1;padding-top:8px;text-align:center;\">");
            html.AppendLine("    <div style=\"font-weight:700;font-size:12px;\">External Links</div>");
            html.AppendLine("    <div class=\"small-btns\">");
            html.AppendLine($"      <a class=\"small-btn\" href=\"{YoutubeUrl}\">YouTube</a>");
            html.AppendLine($"      <a class=\"small-btn\" href=\"{GithubUrl}\">GitHub</a>");
            html.AppendLine($"      <a class=\"small-btn\" href=\"{AllLinksUrl}\">Website</a>");
            html.AppendLine("    </div>");
            html.AppendLine("  </div>");
            html.AppendLine("</div></aside>");
            html.AppendLine("</div></div></div>");
            html.AppendLine("<script>");
            html.AppendLine("document.addEventListener('click', function(e) { var link = e.target.closest('a'); if (!link) { e.preventDefault(); } });");
            html.AppendLine("document.addEventListener('selectstart', function(e) { e.preventDefault(); });");
            html.AppendLine("document.addEventListener('dragstart', function(e) { e.preventDefault(); });");
            html.AppendLine("</script>");
            html.AppendLine("</body></html>");

            return html.ToString();
        }

        private static string ExpandText(string baseText, int times)
        {
            string[] expansions =
            {
                "The methodology emphasizes a systematic approach to problem-solving, prioritizing conceptual clarity over rote memorization.",
                "This period was marked by rigorous academic discipline and the development of structured daily routines.",
                "Reports indicate a consistent focus on iterative revision cycles, aligning with standard preparation strategies for national-level competitive examinations.",
                "Public documentation of these activities serves as a repository of study patterns and time-management techniques.",
                "The approach incorporates elements of the 'Feynman Technique' for concept reinforcement and active recall methods."
            };

            var builder = new StringBuilder(baseText);
            for (int i = 0; i < times; i++)
            {
                builder.Append(' ');
                builder.Append(expansions[i % expansions.Length]);
            }

            return builder.ToString();
        }

        private static PictureBox BuildPictureBox(int width, int height)
        {
            return new PictureBox
            {
                Width = width,
                Height = height,
                Margin = new Padding(0, 0, 12, 0),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(245, 245, 245)
            };
        }

        private static Control BuildSectionHeader(string text)
        {
            var wrapper = new Panel
            {
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 6)
            };

            var label = new Label
            {
                Text = text,
                Font = new Font("Times New Roman", 12F, FontStyle.Regular),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };

            var rule = new Panel
            {
                Height = 1,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(162, 169, 177)
            };

            wrapper.Controls.Add(rule);
            wrapper.Controls.Add(label);
            label.BringToFront();
            return wrapper;
        }

        private static Label BuildParagraph(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F),
                AutoSize = true,
                MaximumSize = new Size(640, 0),
                Margin = new Padding(0, 0, 0, 12)
            };
        }

        private static Control BuildNowPlayingPanel()
        {
            var panel = new Panel
            {
                AutoSize = true,
                BackColor = Color.White
            };

            bool isOnline = NetworkInterface.GetIsNetworkAvailable();
            if (!isOnline)
            {
                var offlineLabel = new Label
                {
                    Text = "Offline: it is showing what the developer is listening to right now.",
                    AutoSize = true,
                    ForeColor = Color.FromArgb(140, 60, 60),
                    Margin = new Padding(0, 0, 0, 12)
                };
                panel.Controls.Add(offlineLabel);
                return panel;
            }

            var browser = new WebBrowser
            {
                Width = 480,
                Height = 170,
                ScriptErrorsSuppressed = true
            };
            browser.Navigate(SpotifyWidgetUrl);

            panel.Controls.Add(browser);
            return panel;
        }

        private static Control BuildLinksPanel()
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 12)
            };

            panel.Controls.Add(BuildLink("Website", "https://qlynk.vercel.app"));
            panel.Controls.Add(BuildLink("All Links", "https://qlynk.vercel.app/alllinks"));
            panel.Controls.Add(BuildLink("Repository", "https://github.com/deepdeyiitgn/Clock-Overlays"));
            panel.Controls.Add(BuildLink("YouTube", "https://qlynk.vercel.app/yt"));
            panel.Controls.Add(BuildLink("Instagram", "https://qlynk.vercel.app/insta"));

            return panel;
        }

        private static LinkLabel BuildLink(string label, string url)
        {
            var link = new LinkLabel
            {
                Text = label,
                AutoSize = true,
                LinkColor = Color.FromArgb(26, 115, 232),
                ActiveLinkColor = Color.FromArgb(20, 90, 190),
                VisitedLinkColor = Color.FromArgb(88, 80, 141),
                Margin = new Padding(0, 0, 12, 8)
            };

            link.Click += (_, __) =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show(
                        "Unable to open the link in a browser.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            };

            return link;
        }
    }

    internal static class ImageCacheHelper
    {
        private static readonly HttpClient Client = new HttpClient();
        private static readonly string CacheRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Clock-Overlays",
            "cache",
            "images");

        public static async Task LoadIntoPictureBoxAsync(PictureBox pictureBox, string url, string cacheKey)
        {
            if (pictureBox == null)
            {
                return;
            }

            Directory.CreateDirectory(CacheRoot);
            string cachePath = Path.Combine(CacheRoot, BuildCacheFileName(cacheKey));

            if (File.Exists(cachePath))
            {
                TrySetImageFromFile(pictureBox, cachePath);
            }

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                EnsurePlaceholder(pictureBox);
                return;
            }

            try
            {
                byte[] data = await Client.GetByteArrayAsync(url).ConfigureAwait(false);
                File.WriteAllBytes(cachePath, data);
                TrySetImageFromBytes(pictureBox, data);
            }
            catch
            {
                EnsurePlaceholder(pictureBox);
            }
        }

        private static void TrySetImageFromFile(PictureBox pictureBox, string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                using var image = Image.FromStream(stream);
                SetImage(pictureBox, new Bitmap(image));
            }
            catch
            {
                EnsurePlaceholder(pictureBox);
            }
        }

        private static void TrySetImageFromBytes(PictureBox pictureBox, byte[] data)
        {
            try
            {
                using var stream = new MemoryStream(data);
                using var image = Image.FromStream(stream);
                SetImage(pictureBox, new Bitmap(image));
            }
            catch
            {
                EnsurePlaceholder(pictureBox);
            }
        }

        private static void SetImage(PictureBox pictureBox, Image image)
        {
            if (pictureBox.InvokeRequired)
            {
                pictureBox.BeginInvoke(new Action(() => SetImage(pictureBox, image)));
                return;
            }

            var old = pictureBox.Image;
            pictureBox.Image = image;
            old?.Dispose();
        }

        private static void EnsurePlaceholder(PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                return;
            }

            SetImage(pictureBox, BuildPlaceholderImage(pictureBox.Width, pictureBox.Height));
        }

        private static Image BuildPlaceholderImage(int width, int height)
        {
            var bitmap = new Bitmap(Math.Max(1, width), Math.Max(1, height));
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.FromArgb(235, 235, 235));

            using var pen = new Pen(Color.FromArgb(200, 200, 200));
            graphics.DrawRectangle(pen, 0, 0, bitmap.Width - 1, bitmap.Height - 1);

            using var font = new Font("Segoe UI", 7F, FontStyle.Regular);
            var text = "Image unavailable offline";
            var size = graphics.MeasureString(text, font);
            var point = new PointF(
                (bitmap.Width - size.Width) / 2,
                (bitmap.Height - size.Height) / 2);
            using var brush = new SolidBrush(Color.FromArgb(120, 120, 120));
            graphics.DrawString(text, font, brush, point);

            return bitmap;
        }

        private static string BuildCacheFileName(string input)
        {
            using var sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var builder = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString() + ".img";
        }
    }
}
