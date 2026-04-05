using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TransparentClock
{
    public static class UpdateChecker
    {
        private const string VersionUrl = "https://clock.qlynk.me/version.json";
        private static bool hasChecked;
        private static bool hasPrompted;

        public static void CheckForUpdatesAsync(Form owner)
        {
            if (hasChecked)
            {
                return;
            }

            hasChecked = true;

            _ = Task.Run(async () =>
            {
                try
                {
                    using var httpClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromSeconds(5)
                    };

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
                    using var response = await httpClient.GetAsync(VersionUrl, cts.Token).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        return;
                    }

                    string json = await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return;
                    }

                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("version", out var versionEl) ||
                        !root.TryGetProperty("downloadUrl", out var downloadUrlEl))
                    {
                        return;
                    }

                    string latestVersion = versionEl.GetString() ?? string.Empty;
                    string downloadUrl = downloadUrlEl.GetString() ?? string.Empty;
                    string releaseNotes = root.TryGetProperty("releaseNotes", out var notesEl)
                        ? notesEl.GetString() ?? string.Empty
                        : string.Empty;

                    // Compares the dynamic text version (e.g. V05.04.2026) directly
                    string currentVersion = AppInfo.CurrentVersion;
                    if (string.Equals(latestVersion, currentVersion, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    if (hasPrompted)
                    {
                        return;
                    }

                    hasPrompted = true;

                    owner.Invoke(new Action(() =>
                    {
                        using var prompt = new UpdatePromptForm(latestVersion, downloadUrl);
                        prompt.SetNotes(releaseNotes);
                        prompt.ShowDialog(owner);
                    }));
                }
                catch
                {
                    // Fail silently in background
                }
            });
        }

        private static void OpenUrl(string url)
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
                // Fallback or ignore
            }
        }

        private sealed class UpdatePromptForm : Form
        {
            private readonly Label notesLabel;

            public UpdatePromptForm(string latestVersion, string downloadUrl)
            {
                Text = "Mission Update Available";
                Size = new System.Drawing.Size(420, 260);
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowIcon = false;
                BackColor = System.Drawing.Color.FromArgb(245, 247, 250); // Premium light background
                Font = new System.Drawing.Font("Segoe UI", 9.5F);

                var title = new Label
                {
                    Text = $"New Tactical Update: {latestVersion}",
                    Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(33, 37, 41),
                    AutoSize = true,
                    Location = new System.Drawing.Point(15, 15)
                };

                var subtitle = new Label
                {
                    Text = "A new version of the War-Room Tracker is ready for deployment.",
                    AutoSize = true,
                    ForeColor = System.Drawing.Color.FromArgb(80, 80, 80),
                    Location = new System.Drawing.Point(18, 45)
                };

                notesLabel = new Label
                {
                    Location = new System.Drawing.Point(18, 75),
                    Size = new System.Drawing.Size(370, 50),
                    ForeColor = System.Drawing.Color.FromArgb(0, 102, 204),
                    Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Italic)
                };

                var instruction = new Label
                {
                    Text = "Click below to download the latest setup file.",
                    AutoSize = true,
                    Location = new System.Drawing.Point(18, 140)
                };

                var downloadButton = new Button
                {
                    Text = "Download Update",
                    Width = 150,
                    Height = 40,
                    Location = new System.Drawing.Point(20, 170),
                    BackColor = System.Drawing.Color.FromArgb(0, 123, 255),
                    ForeColor = System.Drawing.Color.White,
                    Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                downloadButton.FlatAppearance.BorderSize = 0;
                downloadButton.Click += (_, __) =>
                {
                    OpenUrl(downloadUrl);
                    Close();
                };

                var laterButton = new Button
                {
                    Text = "Dismiss",
                    Width = 90,
                    Height = 40,
                    Location = new System.Drawing.Point(180, 170),
                    BackColor = System.Drawing.Color.FromArgb(220, 220, 220),
                    ForeColor = System.Drawing.Color.Black,
                    Font = new System.Drawing.Font("Segoe UI", 10F),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                laterButton.FlatAppearance.BorderSize = 0;
                laterButton.Click += (_, __) => Close();

                Controls.Add(title);
                Controls.Add(subtitle);
                Controls.Add(notesLabel);
                Controls.Add(instruction);
                Controls.Add(downloadButton);
                Controls.Add(laterButton);
            }

            public void SetNotes(string notes)
            {
                if (string.IsNullOrWhiteSpace(notes))
                {
                    notesLabel.Text = "Standard performance improvements and bug fixes.";
                    return;
                }

                notesLabel.Text = notes;
            }
        }
    }
}