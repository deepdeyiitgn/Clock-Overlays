using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ClockInstaller.Services;
using ClockInstaller.Utilities;
using ClockInstaller.Models;

namespace ClockInstaller.Forms;

public class ReleaseInfoForm : UserControl
{
    private Panel _topPanel, _fillPanel;
    private WebBrowser _wbNotes;

    public ReleaseInfoForm()
    {
        _topPanel = new Panel { Dock = DockStyle.Top, Height = 55 };
        var title = UIHelper.MakeLabel("Release Details", 24f, ThemeColors.TextPrimary, true);
        title.Location = new Point(48, 10);
        _topPanel.Controls.Add(title);

        _fillPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(48, 0, 48, 20) };
        _wbNotes = new WebBrowser {
            Dock = DockStyle.Fill, ScriptErrorsSuppressed = true,
            IsWebBrowserContextMenuEnabled = false, WebBrowserShortcutsEnabled = false
        };
        _fillPanel.Controls.Add(_wbNotes);

        Controls.Add(_fillPanel); 
        Controls.Add(_topPanel); 
        
        _topPanel.SendToBack();
        _fillPanel.BringToFront();

        this.ParentChanged += OnShow;
    }

    private void OnShow(object? s, EventArgs e)
    {
        if (this.Parent == null) return; 
        
        var r = AppServices.State.SelectedRelease;
        if (r == null) return;

        string date = r.PublishedAt.HasValue ? r.PublishedAt.Value.ToString("MMMM dd, yyyy • HH:mm tt") : "Unknown Time";
        
        // FIX: Display Total Downloads and iterate through ALL assets (including Source Code)
        string fullContent = $"**Published:** {date}\n**Total Version Downloads:** {r.TotalDownloads:N0}\n\n**Available Files:**\n";
        
        foreach(var a in r.GetAllAssets()) {
            string dlCount = a.Id == -1 ? "GitHub Auto-Archive" : $"{a.DownloadCount:N0} downloads";
            fullContent += $"- **{a.Name}** ({a.FormattedSize}) — {dlCount}\n";
        }
        
        string rawMd = string.IsNullOrWhiteSpace(r.Body) ? "No release notes provided." : r.Body;
        fullContent += $"\n**Release Notes:**\n{rawMd}";

        string htmlBody = ParseMarkdownToHtml(fullContent);
        
        string fullHtml = $@"
        <html><head>
        <style>
            body {{ background-color: #1a1a2e; color: #e2e8f0; font-family: 'Segoe UI', sans-serif; font-size: 14px; padding: 5px; 
                   -webkit-user-select: none; -moz-user-select: none; -ms-user-select: none; user-select: none; cursor: default; }}
            h1, h2, h3 {{ color: #a277ff; border-bottom: 1px solid #333; padding-bottom: 5px; }}
            img {{ max-width: 100%; height: auto; border-radius: 6px; margin: 10px 0; -webkit-user-drag: none; pointer-events: none; }}
            a {{ color: #7f5af0; text-decoration: none; pointer-events: none; }}
            code {{ background-color: #0f0f1a; padding: 2px 5px; border-radius: 4px; font-family: Consolas, monospace; }}
            pre {{ background-color: #0f0f1a; padding: 10px; border-radius: 6px; overflow-x: auto; }}
        </style>
        <script>
            document.oncontextmenu = function() {{ return false; }};
            document.onselectstart = function() {{ return false; }};
            document.ondragstart = function() {{ return false; }};
        </script>
        </head><body>{htmlBody}</body></html>";
        
        _wbNotes.DocumentText = fullHtml;
    }

    private string ParseMarkdownToHtml(string md) {
        string html = md;
        html = Regex.Replace(html, @"!\[([^\]]*)\]\(([^\)]+)\)", "<img src=\"$2\" alt=\"$1\" />");
        html = Regex.Replace(html, @"\[([^\]]+)\]\(([^\)]+)\)", "<a href=\"#\">$1</a>");
        html = Regex.Replace(html, @"\*\*([^\*]+)\*\*", "<b>$1</b>");
        html = Regex.Replace(html, @"\`\`\`(.*?)\`\`\`", "<pre><code>$1</code></pre>", RegexOptions.Singleline);
        html = Regex.Replace(html, @"\`([^\`]+)\`", "<code>$1</code>");
        html = Regex.Replace(html, @"^\#\#\# (.*?)$", "<h3>$1</h3>", RegexOptions.Multiline);
        html = Regex.Replace(html, @"^\#\# (.*?)$", "<h2>$1</h2>", RegexOptions.Multiline);
        html = Regex.Replace(html, @"^\# (.*?)$", "<h1>$1</h1>", RegexOptions.Multiline);
        return html.Replace("\r\n", "<br/>").Replace("\n", "<br/>");
    }
}
